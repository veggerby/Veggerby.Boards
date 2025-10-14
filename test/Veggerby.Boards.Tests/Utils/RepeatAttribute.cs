using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Xunit.Sdk;

namespace Veggerby.Boards.Tests.Utils;

public class RepeatAttribute : DataAttribute
{
    private readonly int _count;

    public RepeatAttribute(int count)
    {
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count),
                "Repeat count must be greater than 0.");
        }

        _count = count;
    }

    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
#pragma warning disable RS0030 // Do not use banned APIs
        return [.. Enumerable
            .Range(0, _count)
            .Select(x => new object[] { Guid.NewGuid() })];
#pragma warning restore RS0030 // Do not use banned APIs
    }
}