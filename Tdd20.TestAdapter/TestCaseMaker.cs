using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System;
using System.IO;

namespace Tdd20.TestAdapter
{
    internal class TestCaseMaker
    {
        public static TestCase Make(string testName, string source)
        {   // testName is of the form "test name?filename with extension?line#"
            string[] pieces = testName.Split(new[] { "?" }, StringSplitOptions.RemoveEmptyEntries);
            string name     = pieces[0];
            string filename = Path.GetFileName(pieces[1]);
                   filename = filename.Replace('.', '\u2024'); // replace real period with lookalike, because real one causes tree nodes
            int line        = int.Parse(pieces[2]);

            var testCase     = new TestCase("TDD20." + filename + '.' + name, new Uri("executor://Tdd20.TestAdapter"), source)
            {
                DisplayName  = name,
                CodeFilePath = pieces[1],
                LineNumber   = line
            };
            return testCase;
        }
    }
}
