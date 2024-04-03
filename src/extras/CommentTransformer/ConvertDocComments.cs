using NUnit.Framework;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace CommentTransformer
{
    //[Ignore("TDD")]
    public class ConvertDocComments
    {
        private string _srcDir = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "core"
        );
        private Encoding _encoding = Encoding.GetEncoding("latin1") ?? throw new Exception("latin1");

        [Test]
        //[Ignore("TDD")]
        public void ExtractMemberNameList()
        {
            var nameList = new List<string>();

            string ConsiderJava(string csName)
            {
                if (csName == "bool")
                {
                    return "boolean";
                }
                if (csName == "java.lang.Object")
                {
                    return "object";
                }

                return csName;
            }

            foreach (var csFile in Directory.GetFiles(_srcDir, "*.cs", SearchOption.AllDirectories)
                .Select(csFile => Path.GetFullPath(csFile))
            )
            {
                Console.WriteLine(csFile);
                var body = _encoding.GetString(File.ReadAllBytes(csFile));
                var tree = CSharpSyntaxTree.ParseText(body);

                tree.GetRoot().DescendantNodes()
                    .OfType<TypeDeclarationSyntax>()
                    .ToList()
                    .ForEach(
                        typeDef =>
                        {
                            var memberNamespace = typeDef
                                .Ancestors()
                                .OfType<NamespaceDeclarationSyntax>()
                                .Select(it => it.Name + "")
                                .SelectMany(it => it.Split('.'))
                                .ToArray();

                            var className = typeDef.Identifier.Text;

                            var fullClassNames = new List<string>();
                            var realName = string.Join(".", memberNamespace.Append(className));
                            fullClassNames.Add(realName);

                            nameList.Add($"{className}={realName}");

                            if (typeDef.Kind() == SyntaxKind.InterfaceDeclaration && className.StartsWith("I"))
                            {
                                fullClassNames.Add(string.Join(".", memberNamespace.Append(className.Substring(1))));

                                nameList.Add($"{className}={string.Join(".", memberNamespace.Append(className.Substring(1)))}");
                            }

                            foreach (var fullClassName in fullClassNames)
                            {
                                nameList.Add(fullClassName);

                                typeDef.DescendantNodes()
                                    .OfType<MethodDeclarationSyntax>()
                                    .ToList()
                                    .ForEach(
                                        methodDef =>
                                        {
                                            var methodName = methodDef.Identifier.Text;
                                            {
                                                // fullTypeName
                                                var paramTypeList = string.Join(", ", methodDef.ParameterList.Parameters
                                                    .Select(one => ConsiderJava(FixClassName(one.Type + "")))
                                                );
                                                nameList.Add($"{fullClassName}.{methodName}({paramTypeList})");
                                            }
                                        }
                                    );

                                typeDef.DescendantNodes()
                                    .OfType<PropertyDeclarationSyntax>()
                                    .ToList()
                                    .ForEach(
                                        propDef =>
                                        {
                                            var propName = propDef.Identifier.Text;
                                            var keywords = propDef.AccessorList?.Accessors.Select(it => it.Keyword.Text)?.ToArray() ?? Array.Empty<string>();

                                            nameList.Add($"{fullClassName}.{propName}");

                                            foreach (var keyword in keywords)
                                            {
                                                nameList.Add($"{fullClassName}.{keyword}{propName}");
                                            }
                                        }
                                    );
                            }
                        }
                    );
            }

            nameList.Sort();

            File.WriteAllText("MemberNameList.txt", string.Join("\n", nameList));
        }

        private string FixClassName(string name)
        {
            return name.Replace("com.lowagie.", "iTextSharp.");
        }

        [Test]
        public void Apply()
        {
            var memberNameDict = new SortedDictionary<string, string>();
            var shortNameDict = new SortedDictionary<string, string>();
            foreach (var one in File.ReadAllLines("MemberNameList.txt"))
            {
                var pair = one.Split('=');
                if (pair.Length == 2)
                {
                    shortNameDict[pair[0].ToLowerInvariant()] = pair[1];
                }
                else if (pair.Length == 1)
                {
                    memberNameDict[one.ToLowerInvariant()] = one;
                }
            }

            string? GetCRefOf(string keyword)
            {
                keyword = keyword.Replace("#", ".");

                {
                    if (shortNameDict.TryGetValue(keyword.ToLowerInvariant(), out string? longName))
                    {
                        return longName;
                    }
                }

                {
                    // Gif → GifImage
                    if (!keyword.Contains(".") && shortNameDict.TryGetValue($"{keyword}Image".ToLowerInvariant(), out string? longName))
                    {
                        return longName;
                    }

                }

                var list = new List<string>();
                {
                    var className = FixClassName(keyword);
                    list.Add(className);
                }

                foreach (var target in list.Select(it => it.ToLowerInvariant()))
                {
                    if (memberNameDict.TryGetValue(target, out string? hit))
                    {
                        return hit;
                    }
                }

                Console.Error.WriteLine(keyword);
                return keyword;
            }

            foreach (var csFile in Directory.GetFiles(_srcDir, "*.cs", SearchOption.AllDirectories)
                .Select(csFile => Path.GetFullPath(csFile))
                .Where(csFile => false
                    //|| csFile.Contains(@"text\Annotation.cs")
                    //|| csFile.Contains(@"text\Anchor.cs")
                    //|| csFile.Contains(@"text\Cell.cs")
                    //|| csFile.Contains(@"pdf\ColumnText.cs")
                    //|| csFile.Contains(@"\RtfProtection.cs")
                    || true
                )
            )
            {
                Console.WriteLine(csFile);
                var body1 = _encoding.GetString(File.ReadAllBytes(csFile));

                var body2 = Regex.Replace(
                    body1,
                    "(?<leading>[ \\t]*)/\\*\\*[ \\t]?(?<content>.+?)\\*/",
                    match =>
                    {
                        var leading = match.Groups["leading"].Value;

                        if (leading.Length == 0)
                        {
                            return match.Value;
                        }

                        var content1 = match.Groups["content"].Value
                            .Replace("\r\n", "\n")
                            .Replace("\r", "\n");

                        //if (0 <= content1.IndexOf("(non-Javadoc)", StringComparison.InvariantCultureIgnoreCase))
                        //{
                        //    return match.Value;
                        //}

                        var content2 = Regex.Replace(
                            content1,
                            "^([ \\t]*\\*[ \\t]?|[ \\t]+$)",
                            "",
                            RegexOptions.Multiline
                        );

                        var lines = content2.Split('\n');

                        var state = 0;
                        var block = "";
                        var tags = new List<Tag>();
                        var tagName = "";
                        var commentBody = "";
                        for (int y = 0; y < lines.Length; y++)
                        {
                            var matchTag = Regex.Match(lines[y], "^\\s*@(?<tag>[a-z]+)\\s*(?<content>.*)$", RegexOptions.Singleline);
                            if (matchTag.Success || (y + 1) == lines.Length)
                            {
                                if ((y + 1) == lines.Length)
                                {
                                    block += lines[y] + "\n";
                                }

                                if (state == 0)
                                {
                                    commentBody = block;
                                }
                                else
                                {
                                    tags.Add(new Tag(Name: tagName, Content: block));
                                }

                                state = 1;
                                tagName = matchTag.Groups["tag"].Value;
                                block = matchTag.Groups["content"].Value + "\n";
                            }
                            else
                            {
                                block += lines[y] + "\n";
                            }
                        }

                        return FormatCsComment(leading, commentBody, tags, GetCRefOf);
                        //return match.Value;
                    },
                    RegexOptions.Singleline
                );

                //File.WriteAllBytes(csFile, _encoding.GetBytes(body2));
            }
        }

        /// <param name="Name">param, return, throws, since, see, seealso</param>
        private record Tag(string Name, string Content)
        {
            public override string ToString() => $"@{Name} {Content}";
        }

        private string FormatCsComment(string leading, string commentBody, IEnumerable<Tag> tags, Func<string, string?> getCRefOf)
        {
            var writer = new StringWriter();

            void WriteCsTag(string start, string end, string body)
            {
                body = body.Trim();

                var isSingleLine = body.IndexOfAny(new char[] { '\n', '\r' }) < 0;

                if (isSingleLine)
                {
                    writer.WriteLine($"{leading}/// {start}{body.Trim()}{end}");
                }
                else
                {
                    writer.WriteLine($"{leading}/// {start}");
                    foreach (var line in body.Split('\n'))
                    {
                        writer.WriteLine($"{leading}/// {line}");
                    }
                    writer.WriteLine($"{leading}/// {end}");
                }
            }

            if (commentBody.Trim().Any())
            {
                WriteCsTag("<summary>", "</summary>", ReformatCodeAndPreTags(FormatToHtmlParagraphs(commentBody)));
            }

            foreach (var tag in tags)
            {
                if (false) { }
                else if (false
                    || tag.Name == "return"
                    || tag.Name == "returns"
                )
                {
                    WriteCsTag("<returns>", "</returns>", ReformatCodeAndPreTags(tag.Content));
                }
                else if (false
                    || tag.Name == "param"
                    || tag.Name == "para" // typo
                )
                {
                    var pair = ParsePair(tag.Content);
                    WriteCsTag($"<param name=\"{pair.Name}\">", "</param>", ReformatCodeAndPreTags(pair.Content));
                }
                else if (false
                    || tag.Name == "throws"
                    || tag.Name == "exception"
                )
                {
                    var pair = ParsePair(tag.Content);
                    WriteCsTag($"<exception cref=\"{pair.Name}\">", "</exception>", ReformatCodeAndPreTags(pair.Content));
                }
                else if (false
                    || tag.Name == "see"
                    || tag.Name == "seealso"
                )
                {
                    var content = tag.Content.Trim();
                    var cref = getCRefOf(content);
                    if (cref != null)
                    {
                        writer.WriteLine($"<{tag.Name} cref=\"{cref}\" />");
                    }
                    else
                    {
                        WriteCsTag($"<{tag.Name}>", $"</{tag.Name}>", ReformatCodeAndPreTags(tag.Content));
                    }
                }
                else if (false
                    || tag.Name == "since"
                    || tag.Name == "author"
                    || tag.Name == "version"
                    || tag.Name == "deprecated"
                )
                {
                    WriteCsTag($"<{tag.Name}>", $"</{tag.Name}>", ReformatCodeAndPreTags(tag.Content));
                }
            }

            return writer.ToString().TrimEnd();
        }

        private string ReformatCodeAndPreTags(string body)
        {
            return Regex.Replace(
                body,
                "<CODE>(?<content>.*?)</CODE>",
                match => $"<c>{match.Groups["content"].Value}</c>",
                RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
            )
                .Replace("<pre>", "<code>")
                .Replace("</pre>", "</code>")
                ;
        }

        private string FormatToHtmlParagraphs(string body)
        {
            var parts = Regex.Split(body, "^[ \\t]*<P>[ \\t]*$", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (2 <= parts.Length)
            {
                return string.Join(
                    "\n",
                    parts.Select(
                        part => $"<para>\n{part.Trim()}\n</para>"
                    )
                );
            }
            else
            {
                return body;
            }
        }

        private record Pair(string Name, string Content);

        private Pair ParsePair(string body)
        {
            var match = Regex.Match(body, "^\\s*(?<name>\\S+)\\s+(?<content>.+)$", RegexOptions.Singleline);
            if (match.Success)
            {
                return new Pair(
                    Name: match.Groups["name"].Value,
                    Content: match.Groups["content"].Value
                );
            }
            else
            {
                return new Pair(
                    Name: "",
                    Content: body
                );
            }
        }
    }
}
