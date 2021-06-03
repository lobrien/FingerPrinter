using Markdig.Syntax;
using System;
using System.IO;
using System.Linq;

namespace FingerPrinter
{
    class Program
    {
        static void Main(string[] args)
        {

            var docsRepoLocalRoot = @"c:\src\AzureDocs\azure-docs-pr\articles\machine-learning";
            var filename = "tutorial-deploy-managed-endpoints-using-system-managed-identity.md";
            var fullPath = Path.Combine(docsRepoLocalRoot, filename);
            var content = File.ReadAllText(fullPath);

            var els = Fingerprinter.ExtractCodeElements(content);
            var el = els.Where(el => el.Contains("azureml-examples-main")).First();

            var fingerprint = Fingerprinter.Fingerprint(el);
            var newSnippet = Fingerprinter.AddFingerprint(el, fingerprint);
            Console.WriteLine(newSnippet);

        }
    }
}
