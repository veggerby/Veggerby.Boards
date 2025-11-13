using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.Internal;

/// <summary>
/// Provides a deterministic, canonical, allocation-minimized serialization of artifact state objects
/// for hashing purposes. This replaces transient <c>ToString()</c> usage to ensure stable cross-platform
/// ordering and representation.
/// </summary>
/// <remarks>
/// Rules:
/// 1. Properties only (public instance, readable) included; fields ignored.
/// 2. Ordered lexicographically by property name (ordinal, case sensitive).
/// 3. Each value encoded with a type tag byte followed by its little-endian representation or UTF-8 bytes.
/// 4. Null values encoded as a single 0x00 tag (TypeCode.Object + null marker). Non-null start with a non-zero tag.
/// 5. Strings: UTF-8 length (varint) + bytes.
/// 6. Enums serialized as underlying integral type (same as numeric path) with distinct enum type tag.
/// 7. Collections (IEnumerable but excluding string) serialized as: collection tag (0xF0), element count (varint), elements recursively.
///    For phase 1 we only dive one level (no cycle detection); deeper graphs should remain shallow in current engine design.
/// 8. Guid encoded as 16 bytes little-endian.
/// 9. Floating point: IEEE 754 little-endian directly.
/// This is intentionally conservative; future optimization can introduce hand-written serializers for hot state types.
/// </remarks>
internal static class CanonicalStateSerializer
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();

    public static void WriteObject(object instance, ref IncrementalHashWriter writer)
    {
        // Root call initializes visited set with reference equality to break cycles deterministically.
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        WriteObjectInternal(instance, ref writer, visited, 0);
    }

    private static void WriteObjectInternal(object instance, ref IncrementalHashWriter writer, HashSet<object> visited, int depth)
    {
        if (instance is null)
        {
            writer.WriteByte(0); // null marker
            return;
        }

        if (depth > 32) // defensive depth cap (graph should be shallow)
        {
            writer.WriteByte(0xDD); // depth overflow tag
            return;
        }

        var type = instance.GetType();

        // Primitive fast-paths
        if (TryWritePrimitive(instance, type, ref writer))
        {
            return;
        }

        // Artifact identity fast-path (emit id only)
        if (instance is Artifact artifact)
        {
            writer.WriteByte(0xAD); // artifact tag
            writer.WriteUtf8(artifact.Id);
            return;
        }

        // Type metadata fast-path: serialize as name only to avoid reflective property getters (e.g., DeclaringMethod) throwing.
        if (instance is Type typeInstance)
        {
            writer.WriteByte(0xA1); // type tag
            writer.WriteUtf8(typeInstance.FullName ?? typeInstance.Name);
            return;
        }

        // Cycle detection (only for non-primitives / non-artifacts)
        if (!type.IsValueType)
        {
            if (!visited.Add(instance))
            {
                writer.WriteByte(0xCE); // cycle tag
                writer.WriteUtf8(type.FullName ?? type.Name);
                return;
            }
        }

        if (type.IsEnum)
        {
            writer.WriteByte(0x0E); // enum tag
            var underlying = Convert.ChangeType(instance, Enum.GetUnderlyingType(type));
            WriteIntegral(Convert.ToInt64(underlying), ref writer);
            return;
        }

        if (instance is System.Collections.IEnumerable enumerable && instance is not string)
        {
            writer.WriteByte(0xF0); // collection tag
            int count = 0;
            foreach (var _ in enumerable)
            {
                count++;
            }
            writer.WriteVarUInt((uint)count);
            foreach (var item in enumerable)
            {
                WriteObjectInternal(item!, ref writer, visited, depth + 1);
            }
            return;
        }

        // Complex object: write type tag + ordered properties
        writer.WriteByte(0xC0); // complex object tag
        var props = _propertyCache.GetOrAdd(type, static t => t
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.CanRead && p.GetMethod!.GetParameters().Length == 0)
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .ToArray());
        writer.WriteVarUInt((uint)props.Length);
        foreach (var p in props)
        {
            writer.WriteUtf8(p.Name);
            var value = p.GetValue(instance);
            WriteObjectInternal(value!, ref writer, visited, depth + 1);
        }
    }

    private static bool TryWritePrimitive(object value, Type type, ref IncrementalHashWriter writer)
    {
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Boolean:
                writer.WriteByte(0x01);
                writer.WriteByte((bool)value ? (byte)1 : (byte)0);
                return true;
            case TypeCode.Byte:
                writer.WriteByte(0x02);
                writer.WriteByte((byte)value);
                return true;
            case TypeCode.SByte:
                writer.WriteByte(0x12);
                writer.WriteByte(unchecked((byte)(sbyte)value));
                return true;
            case TypeCode.Int16:
                writer.WriteByte(0x03);
                writer.WriteLittleEndian(unchecked((ushort)(short)value));
                return true;
            case TypeCode.UInt16:
                writer.WriteByte(0x13);
                writer.WriteLittleEndian((ushort)value);
                return true;
            case TypeCode.Int32:
                writer.WriteByte(0x04);
                writer.WriteLittleEndian(unchecked((uint)(int)value));
                return true;
            case TypeCode.UInt32:
                writer.WriteByte(0x14);
                writer.WriteLittleEndian((uint)value);
                return true;
            case TypeCode.Int64:
                writer.WriteByte(0x05);
                writer.WriteLittleEndian(unchecked((ulong)(long)value));
                return true;
            case TypeCode.UInt64:
                writer.WriteByte(0x15);
                writer.WriteLittleEndian((ulong)value);
                return true;
            case TypeCode.String:
                writer.WriteByte(0x06);
                writer.WriteUtf8((string)value);
                return true;
            case TypeCode.Single:
                writer.WriteByte(0x07);
                writer.WriteLittleEndian(MemoryMarshal.AsBytes(new ReadOnlySpan<float>([(float)value])));
                return true;
            case TypeCode.Double:
                writer.WriteByte(0x08);
                writer.WriteLittleEndian(MemoryMarshal.AsBytes(new ReadOnlySpan<double>([(double)value])));
                return true;
            case TypeCode.Decimal:
                writer.WriteByte(0x09);
                var bits = decimal.GetBits((decimal)value);
                foreach (var part in bits)
                {
                    writer.WriteLittleEndian(unchecked((uint)part));
                }
                return true;
            case TypeCode.DateTime:
                writer.WriteByte(0x0A);
                writer.WriteLittleEndian(unchecked((ulong)((DateTime)value).ToUniversalTime().Ticks));
                return true;
            case TypeCode.Char:
                writer.WriteByte(0x0B);
                writer.WriteLittleEndian((ushort)(char)value);
                return true;
            case TypeCode.Object:
                if (value is Guid g)
                {
                    writer.WriteByte(0x0C);
                    var span = MemoryMarshal.AsBytes(new ReadOnlySpan<Guid>([g]));
                    writer.Write(span);
                    return true;
                }
                break;
        }
        return false;
    }

    private static void WriteIntegral(long v, ref IncrementalHashWriter writer)
    {
        // Zig-zag encode then varint for compactness
        ulong zigzag = (ulong)((v << 1) ^ (v >> 63));
        writer.WriteVarUInt(zigzag);
    }
}

/// <summary>
/// Lightweight writer that feeds bytes directly into FNV-1a incremental hash without large intermediate buffers.
/// </summary>
internal ref struct IncrementalHashWriter(ulong seed)
{
    public ulong Hash { get; private set; } = seed;

    public void Write(ReadOnlySpan<byte> data)
    {
        foreach (var b in data)
        {
            Hash ^= b;
            Hash *= 1099511628211UL; // FNV prime
        }
    }

    public void WriteByte(byte b)
    {
        Hash ^= b;
        Hash *= 1099511628211UL;
    }

    public void WriteLittleEndian(ulong value)
    {
        Span<byte> buf = stackalloc byte[8];
        BitConverter.TryWriteBytes(buf, value);
        Write(buf.ToArray());
    }

    public void WriteLittleEndian(uint value)
    {
        Span<byte> buf = stackalloc byte[4];
        BitConverter.TryWriteBytes(buf, value);
        Write(buf.ToArray());
    }

    public void WriteLittleEndian(ushort value)
    {
        Span<byte> buf = stackalloc byte[2];
        BitConverter.TryWriteBytes(buf, value);
        Write(buf.ToArray());
    }

    public void WriteLittleEndian(ReadOnlySpan<byte> raw)
    {
        Write(raw);
    }

    public void WriteUtf8(string text)
    {
        if (text is null)
        {
            WriteByte(0); // null string marker
            return;
        }
        var byteCount = Encoding.UTF8.GetByteCount(text);
        WriteVarUInt((uint)byteCount);
        Span<byte> buffer = byteCount <= 256 ? stackalloc byte[byteCount] : new byte[byteCount];
        Encoding.UTF8.GetBytes(text, buffer);
        Write(buffer.ToArray());
    }

    public void WriteVarUInt(ulong value)
    {
        // Standard LEB128 unsigned
        while (value >= 0x80)
        {
            WriteByte((byte)(value | 0x80));
            value >>= 7;
        }
        WriteByte((byte)value);
    }
}

/// <summary>
/// Reference equality comparer to track visited objects for cycle detection without invoking potentially overridden equality semantics.
/// </summary>
internal sealed class ReferenceEqualityComparer : IEqualityComparer<object>
{
    public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

    private ReferenceEqualityComparer()
    {
    }

    public new bool Equals(object? x, object? y)
    {
        return ReferenceEquals(x, y);
    }

    public int GetHashCode(object obj)
    {
        return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }
}
