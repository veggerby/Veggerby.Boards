using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

// Utility to normalize test method bodies so each [Fact]/[Theory] has
//   // arrange
//   // act
//   // assert
// in that order (even if logically unused). Idempotent: running again causes no changes.
// Heuristic (regex + simple brace scanning) – avoids full Roslyn dependency to keep core repo clean.

var repoRoot = args.Length > 0 ? Path.GetFullPath(args[0]) : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../.."));
var testRoot = Path.Combine(repoRoot, "test");

if (!Directory.Exists(testRoot))
{
    Environment.ExitCode = 1;
    return 1;
}

int modifiedFiles = 0;
int updatedMethods = 0;

var factRegex = new Regex(@"\[(Fact|Theory)\]", RegexOptions.Compiled);
var methodHeaderRegex = new Regex(@"^(\s*)(public|private|internal|protected).*\)\s*(\{)?\s*$", RegexOptions.Compiled);

foreach (var file in Directory.EnumerateFiles(testRoot, "*.cs", SearchOption.AllDirectories))
{
    var original = File.ReadAllText(file);
    var lines = original.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();
    bool fileChanged = false;

    for (int i = 0; i < lines.Count; i++)
    {
        if (!factRegex.IsMatch(lines[i]))
        {
            continue;
        }

        // Find next method signature line after [Fact]/[Theory]
        int j = i + 1;
        while (j < lines.Count && string.IsNullOrWhiteSpace(lines[j]))
        {
            j++;
        }
        if (j >= lines.Count)
        {
            break;
        }

        if (!methodHeaderRegex.IsMatch(lines[j]))
        {
            continue; // not a method header – skip (nested class etc.)
        }

        // Ensure we have opening brace line; if header line lacked '{', scan forward until first '{'
        int braceLine = j;
        if (!lines[braceLine].Contains("{"))
        {
            int k = braceLine + 1;
            while (k < lines.Count && !lines[k].Contains("{"))
            {
                k++;
            }
            if (k >= lines.Count)
            {
                continue;
            }
            braceLine = k;
        }

        // Determine insertion point (line after '{')
        int bodyStart = braceLine;
        var braceIndex = lines[braceLine].IndexOf('{');
        string indent = new string(lines[braceLine].TakeWhile(char.IsWhiteSpace).ToArray()) + "    "; // indent + 4 spaces

        // Collect existing leading comment block inside method (skip blank/comment lines only)
        int scan = braceLine;
        bool sameLineBrace = lines[braceLine].Trim().EndsWith("{");
        int contentStart = braceLine + 1;
        if (!sameLineBrace)
        {
            // opening brace might be elsewhere; but we treat following line as start
            contentStart = braceLine + 1;
        }

        int cursor = contentStart;
        var existingTags = new List<string>();
        while (cursor < lines.Count)
        {
            var trimmed = lines[cursor].Trim();
            if (trimmed.StartsWith("//"))
            {
                if (trimmed.Contains("arrange", StringComparison.OrdinalIgnoreCase)) existingTags.Add("arrange");
                if (trimmed.Contains("act", StringComparison.OrdinalIgnoreCase)) existingTags.Add("act");
                if (trimmed.Contains("assert", StringComparison.OrdinalIgnoreCase)) existingTags.Add("assert");
                cursor++;
                continue;
            }
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                cursor++;
                continue;
            }
            break; // first non-comment, non-blank line
        }

        // Normalize: if already all three present (in any order) skip
        if (existingTags.Contains("arrange") && existingTags.Contains("act") && existingTags.Contains("assert"))
        {
            continue;
        }

        // Replace the scanned region with normalized block
        int replaceEndExclusive = cursor; // region [contentStart, replaceEndExclusive)
        var newBlock = new List<string>
        {
            indent + "// arrange",
            string.Empty,
            indent + "// act",
            string.Empty,
            indent + "// assert",
            string.Empty
        };

        lines.RemoveRange(contentStart, replaceEndExclusive - contentStart);
        lines.InsertRange(contentStart, newBlock);
        fileChanged = true;
        updatedMethods++;
        i = contentStart + newBlock.Count; // advance
    }

    if (fileChanged)
    {
        var newContent = string.Join("\r\n", lines);
        if (!newContent.EndsWith("\r\n"))
        {
            newContent += "\r\n";
        }
        File.WriteAllText(file, newContent, new UTF8Encoding(false));
        modifiedFiles++;
    // file updated
    }
}

Environment.SetEnvironmentVariable("TEST_COMMENT_NORMALIZER_FILES", modifiedFiles.ToString(CultureInfo.InvariantCulture));
Environment.SetEnvironmentVariable("TEST_COMMENT_NORMALIZER_METHODS", updatedMethods.ToString(CultureInfo.InvariantCulture));
return 0;
