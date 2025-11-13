using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Composes Markdown content in discrete segments (headings, paragraphs, lists, tables) and renders deterministically.
/// Segments are accumulated then flushed to a <see cref="TextWriter"/> via <see cref="Build"/>.
/// </summary>
public sealed class MarkdownBuilder
{
    private readonly List<IMarkdownSegment> _segments = new List<IMarkdownSegment>();

    /// <summary>
    /// Heading levels supported by <see cref="AddHeading"/>.
    /// </summary>
    /// <summary>
    /// Markdown heading levels (mapped directly to number of leading '#' characters).
    /// </summary>
    public enum HeadingLevel
    {
        /// <summary>
        /// Level 1 heading (#) – top document title.
        /// </summary>
        Level1 = 1,

        /// <summary>
        /// Level 2 heading (##) – primary section.
        /// </summary>
        Level2 = 2,

        /// <summary>
        /// Level 3 heading (###) – subsection.
        /// </summary>
        Level3 = 3,

        /// <summary>
        /// Level 4 heading (####) – nested subsection.
        /// </summary>
        Level4 = 4,

        /// <summary>
        /// Level 5 heading (#####) – rarely used deeper nesting.
        /// </summary>
        Level5 = 5,

        /// <summary>
        /// Level 6 heading (######) – deepest standard markdown heading.
        /// </summary>
        Level6 = 6
    }

    /// <summary>
    /// Adds a heading segment. Empty or whitespace text is ignored.
    /// </summary>
    /// <summary>
    /// Adds a heading segment at the specified <paramref name="level"/> with the provided <paramref name="text"/>.
    /// Whitespace-only input is ignored. The text is trimmed before storage.
    /// </summary>
    /// <param name="level">Heading level (1-6).</param>
    /// <param name="text">Raw heading text.</param>
    public void AddHeading(HeadingLevel level, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        _segments.Add(new HeadingSegment(level, text.Trim()));
    }

    /// <summary>
    /// Adds a paragraph of free-form text. Empty paragraphs are ignored.
    /// </summary>
    /// <summary>
    /// Adds a paragraph segment. Paragraphs are emitted verbatim (after trimming) followed by a blank line.
    /// Empty or whitespace-only values are ignored.
    /// </summary>
    /// <param name="text">Paragraph content.</param>
    public void AddParagraph(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        _segments.Add(new ParagraphSegment(text.Trim()));
    }

    /// <summary>
    /// Adds a bullet list. Null or empty items are skipped; empty resulting list is ignored.
    /// </summary>
    /// <summary>
    /// Adds a bullet list. Each non-empty trimmed entry becomes a <c>- item</c> line.
    /// If all supplied <paramref name="items"/> are null/empty, no segment is added.
    /// </summary>
    /// <param name="items">Collection of bullet item strings.</param>
    public void AddBullets(IEnumerable<string> items)
    {
        if (items == null)
        {
            return;
        }

        var materialized = items.Where(i => !string.IsNullOrWhiteSpace(i)).Select(i => i.Trim()).ToList();
        if (materialized.Count == 0)
        {
            return;
        }

        _segments.Add(new BulletListSegment(materialized));
    }

    /// <summary>
    /// Adds a markdown table. Rows must have same column count as headers. No table emitted if headers empty.
    /// </summary>
    /// <summary>
    /// Adds a markdown table with the given <paramref name="headers"/> and <paramref name="rows"/>.
    /// Headers are trimmed; rows with differing column counts are still rendered (excess/missing cells are not padded).
    /// Null rows are skipped. If no valid headers remain, the table is ignored.
    /// </summary>
    /// <param name="headers">Table header values.</param>
    /// <param name="rows">Matrix of row cells.</param>
    public void AddTable(IEnumerable<string> headers, IEnumerable<IEnumerable<string>> rows)
    {
        if (headers == null || rows == null)
        {
            return;
        }

        var headerList = headers.Where(h => h != null).Select(h => h.Trim()).ToList();
        if (headerList.Count == 0)
        {
            return;
        }

        var rowList = new List<IReadOnlyList<string>>();
        foreach (var row in rows)
        {
            if (row == null)
            {
                continue;
            }

            var cells = row.Select(c => c == null ? string.Empty : c.Trim()).ToList();
            rowList.Add(cells);
        }

        _segments.Add(new TableSegment(headerList, rowList));
    }

    /// <summary>
    /// Renders all accumulated segments to the provided <paramref name="writer"/> in insertion order.
    /// </summary>
    /// <summary>
    /// Flushes all accumulated segments to <paramref name="writer"/> in original insertion order.
    /// Each segment is separated by a trailing blank line to ensure clean markdown boundaries.
    /// </summary>
    /// <param name="writer">Destination writer (must not be null).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> is null.</exception>
    public void Build(TextWriter writer)
    {
        if (writer == null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        foreach (var segment in _segments)
        {
            segment.Render(writer);
            writer.WriteLine();
        }
    }

    private interface IMarkdownSegment
    {
        void Render(TextWriter writer);
    }

    private sealed class HeadingSegment : IMarkdownSegment
    {
        private readonly HeadingLevel _level;
        private readonly string _text;

        public HeadingSegment(HeadingLevel level, string text)
        {
            _level = level;
            _text = text;
        }

        public void Render(TextWriter writer)
        {
            writer.Write(new string('#', (int)_level));
            writer.Write(' ');
            writer.WriteLine(_text);
        }
    }

    private sealed class ParagraphSegment : IMarkdownSegment
    {
        private readonly string _text;

        public ParagraphSegment(string text)
        {
            _text = text;
        }

        public void Render(TextWriter writer)
        {
            writer.WriteLine(_text);
        }
    }

    private sealed class BulletListSegment : IMarkdownSegment
    {
        private readonly IReadOnlyList<string> _items;

        public BulletListSegment(IReadOnlyList<string> items)
        {
            _items = items;
        }

        public void Render(TextWriter writer)
        {
            foreach (var item in _items)
            {
                writer.Write("- ");
                writer.WriteLine(item);
            }
        }
    }

    private sealed class TableSegment : IMarkdownSegment
    {
        private readonly IReadOnlyList<string> _headers;
        private readonly IReadOnlyList<IReadOnlyList<string>> _rows;

        public TableSegment(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
        {
            _headers = headers;
            _rows = rows;
        }

        public void Render(TextWriter writer)
        {
            writer.Write("| ");
            writer.Write(string.Join(" | ", _headers));
            writer.WriteLine(" |");

            writer.Write("| ");
            writer.Write(string.Join(" | ", _headers.Select(h => new string('-', Math.Max(3, h.Length)))));
            writer.WriteLine(" |");

            foreach (var row in _rows)
            {
                writer.Write("| ");
                writer.Write(string.Join(" | ", row));
                writer.WriteLine(" |");
            }
        }
    }
}
