using Markdig;
using Markdig.Syntax;
using Microsoft.DocAsCode.MarkdigEngine.Extensions;
using System;
using System.IO;
using System.Text;

namespace FingerPrinter
{
    class Program
    {
        static void Main(string[] args)
        {
            var src = ":::code language=\"csharp\" source=\"~/TripleColonTest.cs\":::";
            var buildPipeline = BuildPipeline(ReadFile);
            var html = RenderCodeSnippetToHtml(buildPipeline, src);
            var fingerprint = Md5Hash(html);
            Console.WriteLine(fingerprint);
        }

        private static string RenderCodeSnippetToHtml(MarkdownPipeline buildPipeline, string snippet)
        {
            using (InclusionContext.PushFile("lobrien_junk.md"))
            {
                var actualHtml = Markdown.ToHtml(snippet, buildPipeline);
                return actualHtml;
            }
        }

        private static string Md5Hash(string s)
        {
            var bytes = new UTF8Encoding().GetBytes(s);
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                var hashbytes = md5.ComputeHash(bytes);
                var hash = BitConverter.ToString(hashbytes).Replace("-", "");
                return hash;
            }
        }

        private static MarkdownPipeline BuildPipeline(MarkdownContext.ReadFileDelegate readFile)
        {

            var markdownContext = new MarkdownContext(readFile: readFile);
            var pipelineBuilder = new MarkdownPipelineBuilder()
                .UseDocfxExtensions(markdownContext)
                .UseYamlFrontMatter();

            var pipeline = pipelineBuilder.Build();
            return pipeline;
        }

        static (string content, object file) ReadFile(string path, MarkdownObject origin)
        {
            var key = Path.Combine(Path.GetDirectoryName(InclusionContext.File.ToString()), path).Replace('\\', '/');

            return (key, key);
        }

    }
}
