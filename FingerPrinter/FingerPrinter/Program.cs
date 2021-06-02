using Markdig;
using Markdig.Syntax;
using Microsoft.DocAsCode.MarkdigEngine.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FingerPrinter
{
    class Program
    {
        static void Main(string[] args)
        {
            var readFileFn = ReadFilesFromJson();

            var src = ":::code language=\"shell\" source=\"~/azureml-examples-main/cli/how-to-batch-score.sh\" id=\"create_batch_endpoint\" :::";
            var buildPipeline = BuildPipeline(readFileFn);
            var html = RenderCodeSnippetToHtml(buildPipeline, src);
            var fingerprint = Md5Hash(html);
            Console.WriteLine(fingerprint);
        }

        private static MarkdownContext.ReadFileDelegate ReadFilesFromJson()
        {
            // urlForAlias maps config JSON key to (probably Github URL)
            var urlForAlias = new Dictionary<string, string>();
            // PathByRepository maps source URL to local path;
            var pathForURL = new Dictionary<string, string>();

            //Hardcoded for hack
            //TODO: You know, you could just as easily dictionary alias -> path and skip the URL altogether
            var jsonPath = "C:\\src\\AzureDocs\\azure-docs-pr\\.openpublishing.publish.config.json";
            pathForURL["https://github.com/azure/azureml-examples"] = "c:\\src\\AzureDocs\\azureml-examples";

            using (var file = File.OpenText(jsonPath))
            using (var reader = new JsonTextReader(file))
            {
                var root = (JObject)JToken.ReadFrom(reader);
                var dependentRepositories = root["dependent_repositories"];
                foreach(var dependentRepository in dependentRepositories)
                {
                    var path_to_root = dependentRepository["path_to_root"].ToString();
                    var url_for_alias = dependentRepository["url"].ToString();
                    //TODO: Incorporate dependentRepository["branch"];
                    urlForAlias[path_to_root] = url_for_alias;
                }

                return (path, origin) =>
                {
                    var re = @"~\/(?<repo_alias>[^\/]*)\/(?<path>\S*)";
                    var match = Regex.Match(path, re, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        var repoAlias = match.Groups["repo_alias"].Value;
                        var filePath = match.Groups["path"].Value;

                        try
                        {
                            var repo = urlForAlias[repoAlias];
                            var repoLocalRoot = pathForURL[repo];
                            var localFilePath = Path.Combine(repoLocalRoot, filePath);
                            if (!File.Exists(localFilePath))
                            {
                                throw new Exception($"Could not find expected file {localFilePath} based on snippet path {path}");
                            }
                            var content = File.ReadAllText(localFilePath);
                            return (content, localFilePath);
                        }
                        catch (KeyNotFoundException _)
                        {
                            throw new Exception($"Could not resolve local path for repository `{repoAlias}`. Either value not found in .openpublishing.config.json or mapping to local path not found in `pathByRepository`");
                        }
                    }
                    else
                    {
                        throw new Exception($"Didn't recognize code snippet alias {path}");
                    }
                };
            }
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
