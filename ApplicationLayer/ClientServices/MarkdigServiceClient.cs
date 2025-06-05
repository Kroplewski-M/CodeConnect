using System.Net;
using System.Text;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace ApplicationLayer.ClientServices;

public class MarkdigServiceClient
{
    public string ConvertToHtmlOnlyCode(string markdown)
    {
        var pipeline = new MarkdownPipelineBuilder().DisableHtml().Build();
        var document = Markdown.Parse(markdown, pipeline);

        var html = new StringBuilder();

        foreach (var node in document)
        {
            if (node is FencedCodeBlock codeBlock)
            {
                var lang = codeBlock.Info?.Trim(); // e.g. "csharp"
                var code = codeBlock.Lines.ToString();
                html.AppendLine(
                    $"<pre class='mb-[10px] mt-[10px]'><code class=\"language-{WebUtility.HtmlEncode(lang)}\">{WebUtility.HtmlEncode(code)}</code></pre>");
            }
            else
            {
                var content = new StringBuilder();

                if (node is LeafBlock leaf)
                {
                    if (leaf.Inline != null)
                    {
                        foreach (var inline in leaf.Inline)
                        {
                            if (inline is LiteralInline literal)
                            {
                                content.Append(literal.Content.Text.Substring(literal.Content.Start, literal.Content.Length));
                            }
                        }
                    }
                    else
                    {
                        content.Append(leaf);
                    }
                }
                else
                {
                    content.Append(node);
                }

                var paragraphText = content.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(paragraphText))
                {
                    html.AppendLine($"<p class='text-light-primaryColor'>{WebUtility.HtmlEncode(paragraphText)}</p>");
                }
            }
        }
        return html.ToString();
    }
}