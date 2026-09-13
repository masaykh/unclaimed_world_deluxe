using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UW.Tools.SourceCompare;

/// <summary>
/// Compares two C# source trees by what they DECLARE, not by what they look like.
///
/// The question it answers: the port's <c>decomp/</c> tree is ILSpy output from the shipped
/// 1.0.4.8 binaries, and <c>original_src/</c> is the source Refactored Games released - which is
/// a DIFFERENT, earlier build. To take the released source as the base and patch it forward, we
/// need to know exactly which types and members 1.0.4.8 has that it does not, and vice versa.
///
/// A textual diff cannot answer that. Every line differs for reasons that carry no information:
/// ILSpy writes <c>this.</c> qualifiers, expands collection initialisers, splits nested types
/// into their own files, spells <c>int</c> as <c>int</c> only sometimes, and orders members by
/// metadata token rather than by how anyone would write them. Comparing declarations skips all
/// of it.
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("usage: sourcecompare <old-tree> <new-tree> [--label-a NAME] [--label-b NAME]");
            Console.Error.WriteLine("                    [--out FILE] [--types-only] [--members-for TYPE]");
            Console.Error.WriteLine();
            Console.Error.WriteLine("  Tree A is the RELEASED SOURCE, tree B the DECOMPILED SHIPPED BUILD.");
            Console.Error.WriteLine("  \"only in B\" therefore means: present in 1.0.4.8 and missing from the source.");
            return 2;
        }

        string dirA = args[0], dirB = args[1];
        string labelA = Opt(args, "--label-a") ?? Path.GetFileName(dirA.TrimEnd('/', '\\'));
        string labelB = Opt(args, "--label-b") ?? Path.GetFileName(dirB.TrimEnd('/', '\\'));
        string? outFile = Opt(args, "--out");
        bool typesOnly = args.Contains("--types-only");
        string? membersFor = Opt(args, "--members-for");

        if (!Directory.Exists(dirA) || !Directory.Exists(dirB))
        {
            Console.Error.WriteLine("both arguments must be existing directories");
            return 2;
        }

        Dictionary<string, TypeInfo> a = Scan(dirA), b = Scan(dirB);

        var report = new StringBuilder();
        void W(string line = "") => report.AppendLine(line);

        W($"{labelA}  vs  {labelB}");
        W(new string('=', 78));
        W();
        W($"  {labelA,-28} {a.Count,6} type(s), {a.Values.Sum(t => t.Members.Count),7} member(s)");
        W($"  {labelB,-28} {b.Count,6} type(s), {b.Values.Sum(t => t.Members.Count),7} member(s)");
        W();

        var onlyA = a.Keys.Except(b.Keys).OrderBy(s => s, StringComparer.Ordinal).ToList();
        var onlyB = b.Keys.Except(a.Keys).OrderBy(s => s, StringComparer.Ordinal).ToList();
        var common = a.Keys.Intersect(b.Keys).OrderBy(s => s, StringComparer.Ordinal).ToList();

        W($"TYPES ONLY IN {labelA}: {onlyA.Count}");
        foreach (string t in onlyA) W("    - " + t + "   [" + a[t].Kind + ", " + a[t].File + "]");
        W();
        W($"TYPES ONLY IN {labelB}: {onlyB.Count}");
        foreach (string t in onlyB) W("    + " + t + "   [" + b[t].Kind + ", " + b[t].File + "]");
        W();

        if (typesOnly)
        {
            Emit(report.ToString(), outFile);
            return 0;
        }

        W($"MEMBER DIFFERENCES ACROSS {common.Count} SHARED TYPE(S)");
        W(new string('-', 78));

        int changedTypes = 0, missingMembers = 0, extraMembers = 0;
        var detail = new StringBuilder();

        foreach (string type in common)
        {
            if (membersFor != null && !type.Contains(membersFor, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var ma = a[type].Members;
            var mb = b[type].Members;
            var lost = ma.Except(mb, StringComparer.Ordinal).OrderBy(s => s, StringComparer.Ordinal).ToList();
            var gained = mb.Except(ma, StringComparer.Ordinal).OrderBy(s => s, StringComparer.Ordinal).ToList();
            if (lost.Count == 0 && gained.Count == 0)
            {
                continue;
            }

            changedTypes++;
            missingMembers += gained.Count;
            extraMembers += lost.Count;

            detail.AppendLine();
            detail.AppendLine($"  {type}");
            foreach (string m in lost) detail.AppendLine($"      - {m}");
            foreach (string m in gained) detail.AppendLine($"      + {m}");
        }

        W($"  {changedTypes} shared type(s) differ");
        W($"  {extraMembers} member(s) only in {labelA}   (marked -)");
        W($"  {missingMembers} member(s) only in {labelB}   (marked +)");
        W(detail.ToString());

        // ---- body-level signal ------------------------------------------------------------
        // Types that agree on every declaration can still differ in what they DO. String
        // literals are the cheapest evidence of that which survives compilation intact.
        var literalChanged = new List<string>();
        foreach (string type in common)
        {
            var la = a[type].Literals.OrderBy(s => s, StringComparer.Ordinal).ToList();
            var lb = b[type].Literals.OrderBy(s => s, StringComparer.Ordinal).ToList();
            if (!la.SequenceEqual(lb, StringComparer.Ordinal))
            {
                literalChanged.Add(type);
            }
        }

        W();
        W("TYPES WHOSE STRING LITERALS DIFFER: " + literalChanged.Count + " of " + common.Count);
        W("  (a body-level signal - declarations can match while the code inside does not)");
        foreach (string type in literalChanged)
        {
            var la = a[type].Literals.OrderBy(s => s, StringComparer.Ordinal).ToList();
            var lb = b[type].Literals.OrderBy(s => s, StringComparer.Ordinal).ToList();
            W();
            W("  " + type);
            foreach (string s in la.Except(lb, StringComparer.Ordinal).Take(12))
                W("      - " + Show(s));
            foreach (string s in lb.Except(la, StringComparer.Ordinal).Take(12))
                W("      + " + Show(s));
        }

        Emit(report.ToString(), outFile);
        return 0;
    }

    /// <summary>One literal on one line, with newlines and tabs made visible.</summary>
    private static string Show(string s)
    {
        // Verbatim replacements, so one line of report is one literal however many
        // line breaks the string itself contains.
        string flat = s.Replace("\r", @"\r").Replace("\n", @"\n").Replace("\t", @"\t");
        if (flat.Length > 110) flat = flat[..110] + "...";
        return "\"" + flat + "\"";
    }

    private static void Emit(string text, string? outFile)
    {
        if (outFile != null)
        {
            File.WriteAllText(outFile, text);
            // Only the header, so a large report does not have to be read to be used.
            foreach (string line in text.Split('\n').TakeWhile(l => !l.StartsWith("    ")).Take(40))
            {
                Console.WriteLine(line.TrimEnd());
            }
            Console.WriteLine($"[full report: {outFile}, {text.Length / 1024} KB]");
        }
        else
        {
            Console.WriteLine(text);
        }
    }

    private static string? Opt(string[] args, string name)
    {
        int i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }

    private sealed class TypeInfo
    {
        public string Kind = "";
        public string File = "";
        public readonly HashSet<string> Members = new(StringComparer.Ordinal);

        /// <summary>
        /// Every string literal in the type, as a multiset.
        ///
        /// A BODY-LEVEL signal, which the member comparison cannot give. Two builds can agree on
        /// every type and every signature and still differ in what the methods do - and that is
        /// exactly the situation between 1.0.4.7 and 1.0.4.8. String literals survive compilation
        /// and decompilation byte for byte, so a type whose literals changed almost certainly had
        /// its code changed, and one whose literals match is weak evidence that it did not.
        /// </summary>
        public readonly List<string> Literals = new();
    }

    private static Dictionary<string, TypeInfo> Scan(string dir)
    {
        var types = new Dictionary<string, TypeInfo>(StringComparer.Ordinal);

        foreach (string path in Directory.EnumerateFiles(dir, "*.cs", SearchOption.AllDirectories)
                                         .OrderBy(p => p, StringComparer.Ordinal))
        {
            // Generated files are not source in any sense that matters here.
            string name = Path.GetFileName(path);
            if (name.EndsWith(".g.cs", StringComparison.Ordinal) ||
                name.EndsWith(".Designer.cs", StringComparison.Ordinal) ||
                path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
                path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            {
                continue;
            }

            SyntaxNode root;
            try
            {
                root = CSharpSyntaxTree.ParseText(File.ReadAllText(path)).GetRoot();
            }
            catch
            {
                continue;
            }

            string relative = Path.GetRelativePath(dir, path);
            foreach (BaseTypeDeclarationSyntax decl in root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
            {
                string full = FullName(decl);
                if (IsCompilerGenerated(full))
                {
                    continue;
                }

                if (!types.TryGetValue(full, out TypeInfo? info))
                {
                    info = new TypeInfo { Kind = Kind(decl), File = relative };
                    types[full] = info;
                }

                foreach (MemberDeclarationSyntax member in decl.ChildNodes().OfType<MemberDeclarationSyntax>())
                {
                    // Fields and events declare one signature PER VARIABLE, because the two
                    // trees group them differently and the grouping carries no meaning.
                    switch (member)
                    {
                        case FieldDeclarationSyntax f:
                            foreach (var v in f.Declaration.Variables)
                                info.Members.Add(v.Identifier.ValueText);
                            continue;
                        case EventFieldDeclarationSyntax ef:
                            foreach (var v in ef.Declaration.Variables)
                                info.Members.Add("event " + v.Identifier.ValueText);
                            continue;
                    }

                    string? signature = Signature(member);
                    if (signature != null && !IsCompilerGenerated(signature))
                    {
                        info.Members.Add(signature);
                    }
                }

                // NUMERIC literals as well as string ones. Strings alone would miss the change
                // most likely to be made in a point release of a simulation game: a tuned
                // constant. A balance tweak from 0.5f to 0.6f alters no declaration and no
                // message, and would otherwise read as "the two builds are identical".
                foreach (SyntaxToken tok in decl.DescendantTokens()
                             .Where(t => t.IsKind(SyntaxKind.StringLiteralToken) ||
                                         t.IsKind(SyntaxKind.NumericLiteralToken) ||
                                         t.IsKind(SyntaxKind.CharacterLiteralToken)))
                {
                    // Attribute each literal to the INNERMOST enclosing type, so a nested class
                    // does not donate its strings to its parent.
                    if (tok.Parent?.Ancestors().OfType<BaseTypeDeclarationSyntax>().FirstOrDefault() == decl)
                        info.Literals.Add(tok.Kind() + ":" + tok.ValueText);
                }

                if (decl is EnumDeclarationSyntax e)
                {
                    foreach (EnumMemberDeclarationSyntax m in e.Members)
                    {
                        info.Members.Add(m.Identifier.ValueText);
                    }
                }
            }
        }

        return types;
    }

    /// <summary>
    /// ILSpy names closure and iterator classes with angle brackets, and they are an artefact of
    /// compilation rather than anything a person wrote. Counting them would report hundreds of
    /// differences that mean only "these two builds used different compilers".
    /// </summary>
    private static bool IsCompilerGenerated(string name) =>
        name.Contains('<') && (name.Contains(">c__", StringComparison.Ordinal) ||
                               name.Contains(">d__", StringComparison.Ordinal) ||
                               name.Contains("<>", StringComparison.Ordinal) ||
                               name.Contains(">e__", StringComparison.Ordinal));

    private static string FullName(BaseTypeDeclarationSyntax decl)
    {
        var parts = new List<string> { decl.Identifier.ValueText + Arity(decl) };
        for (SyntaxNode? node = decl.Parent; node != null; node = node.Parent)
        {
            switch (node)
            {
                case BaseTypeDeclarationSyntax outer:
                    parts.Insert(0, outer.Identifier.ValueText + Arity(outer));
                    break;
                case BaseNamespaceDeclarationSyntax ns:
                    parts.Insert(0, ns.Name.ToString());
                    break;
            }
        }
        return string.Join(".", parts);
    }

    private static string Kind(BaseTypeDeclarationSyntax decl) => decl switch
    {
        ClassDeclarationSyntax => "class",
        StructDeclarationSyntax => "struct",
        InterfaceDeclarationSyntax => "interface",
        RecordDeclarationSyntax => "record",
        EnumDeclarationSyntax => "enum",
        _ => "type",
    };

    private static string Arity(BaseTypeDeclarationSyntax decl) =>
        decl is TypeDeclarationSyntax { TypeParameterList: { } tp } && tp.Parameters.Count > 0
            ? "`" + tp.Parameters.Count
            : "";

    /// <summary>
    /// A member signature normalised until only real differences survive: no return type (ILSpy
    /// and the source spell the same type differently often enough to matter), parameter TYPES
    /// but not names, and language aliases folded together.
    /// </summary>
    private static string? Signature(MemberDeclarationSyntax member) => member switch
    {
        MethodDeclarationSyntax m =>
            m.Identifier.ValueText + Generic(m.TypeParameterList) + Parameters(m.ParameterList),
        // A parameterless constructor that does nothing IN A RELEASE BUILD compiles to exactly
        // the IL the compiler generates when there is no constructor at all, so ILSpy cannot
        // tell them apart and omits it. "Does nothing" has to include Debug.Assert, which is
        // [Conditional("DEBUG")] and therefore absent from the shipped assembly: this game marks
        // ~150 snapshot classes with
        //     public Person() { Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor."); }
        // and every one of them was reported as a difference until this rule accounted for it.
        ConstructorDeclarationSyntax { ParameterList.Parameters.Count: 0 } ctor when IsNoOpInRelease(ctor) => null,
        ConstructorDeclarationSyntax c => ".ctor" + Parameters(c.ParameterList),
        DestructorDeclarationSyntax => ".dtor()",
        PropertyDeclarationSyntax p => p.Identifier.ValueText + " {}",
        IndexerDeclarationSyntax i => "this[]" + Parameters(i.ParameterList),
        EventDeclarationSyntax ev => "event " + ev.Identifier.ValueText,
        // NOT joined. `private int left, top;` in the source is two separate declarations in
        // ILSpy output, and joining them made one signature that could never match either -
        // reporting both as different when the two builds agree exactly.
        EventFieldDeclarationSyntax => null,
        FieldDeclarationSyntax => null,
        OperatorDeclarationSyntax o => "operator " + o.OperatorToken.ValueText + Parameters(o.ParameterList),
        ConversionOperatorDeclarationSyntax co => "operator " + Normalise(co.Type.ToString()),
        _ => null,
    };

    /// <summary>
    /// Whether a constructor body would be empty once the compiler has stripped everything
    /// conditional on DEBUG. Only Debug.* calls qualify - anything else is real work.
    /// </summary>
    private static bool IsNoOpInRelease(ConstructorDeclarationSyntax ctor)
    {
        if (ctor.Initializer != null) return false;
        foreach (StatementSyntax s in ctor.Body?.Statements ?? default)
        {
            if (s is not ExpressionStatementSyntax { Expression: InvocationExpressionSyntax inv })
                return false;
            string target = inv.Expression.ToString();
            if (!target.Contains("Debug.", StringComparison.Ordinal)) return false;
        }
        return true;
    }

    private static string Generic(TypeParameterListSyntax? list) =>
        list is { Parameters.Count: > 0 } ? "`" + list.Parameters.Count : "";

    private static string Parameters(BaseParameterListSyntax? list) =>
        "(" + string.Join(",", (list?.Parameters ?? default).Select(p =>
            (p.Modifiers.Any(m => m.ValueText is "ref" or "out" or "in") ? p.Modifiers.First().ValueText + " " : "") +
            Normalise(p.Type?.ToString() ?? "?"))) + ")";

    /// <summary>
    /// Folds the spellings that differ between generated and written code but mean the same type:
    /// BCL aliases, namespace qualification, and whitespace inside generic arguments.
    /// </summary>
    private static string Normalise(string type)
    {
        // ALL whitespace, not just spaces: a long generic argument list is wrapped across lines
        // in the released source and on one line in ILSpy output.
        string t = new string(type.Where(c => !char.IsWhiteSpace(c)).ToArray());

        // Drop namespace qualification - one tree writes Microsoft.Xna.Framework.Vector2 and the
        // other writes Vector2, and they are the same parameter.
        var sb = new StringBuilder();
        var token = new StringBuilder();
        foreach (char c in t)
        {
            if (char.IsLetterOrDigit(c) || c == '_' || c == '.')
            {
                token.Append(c);
                continue;
            }
            sb.Append(Simplify(token.ToString()));
            token.Clear();
            sb.Append(c);
        }
        sb.Append(Simplify(token.ToString()));
        return sb.ToString();
    }

    private static string Simplify(string token)
    {
        if (token.Length == 0) return token;
        int dot = token.LastIndexOf('.');
        string bare = dot >= 0 ? token[(dot + 1)..] : token;
        return bare switch
        {
            "Boolean" => "bool",
            "Byte" => "byte",
            "SByte" => "sbyte",
            "Char" => "char",
            "Int16" => "short",
            "UInt16" => "ushort",
            "Int32" => "int",
            "UInt32" => "uint",
            "Int64" => "long",
            "UInt64" => "ulong",
            "Single" => "float",
            "Double" => "double",
            "Decimal" => "decimal",
            "String" => "string",
            "Object" => "object",
            _ => bare,
        };
    }
}
