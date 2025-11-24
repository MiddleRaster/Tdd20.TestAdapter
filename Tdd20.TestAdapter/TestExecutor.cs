using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;

namespace Tdd20.TestAdapter
{    
    [FileExtension(".exe")]
    [ExtensionUri("executor://Tdd20.TestAdapter")]
    public class TestExecutor : ITestExecutor
    {
        private bool cancelRequested = false;

        public void RunTests(IEnumerable<TestCase> subsetOfTests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            if (subsetOfTests == null || frameworkHandle == null) return;

            // tests may be from different sources (.exes, in my case). Make a map of source to list of tests in that source.
            var mapOfSourceToTests = new Dictionary<string, List<TestCase>>();
            foreach (var test in subsetOfTests)
            {
                if (!mapOfSourceToTests.TryGetValue(test.Source, out var list))
                {
                    list = new List<TestCase>();
                    mapOfSourceToTests[test.Source] = list;
                }
                list.Add(test);
            }

            foreach (var kvp in mapOfSourceToTests)
            {
                if (cancelRequested)
                    break;

                string source = kvp.Key;
                var testCases = kvp.Value;

                int exitCode  = 0;
                string output = "";
                string args   = string.Join(" ", testCases.Select(t => "\"" + t.GetPropertyValue(PropertyRegistrar.MyStringProperty) + "\""));
                   
                if (runContext.IsBeingDebugged)
                {
                    string pipeName = "TDD20Pipe_" + Guid.NewGuid().ToString("N");
                    using (var server = new NamedPipeServerStream(pipeName, PipeDirection.In))
                    {   // Let VS own the debug session and pump events and launch the process under its debugger.
                        frameworkHandle.LaunchProcessWithDebuggerAttached(filePath: source,
                                                                          arguments: "/pipe:" + pipeName + " " + args,
                                                                          workingDirectory: Path.GetDirectoryName(source),
                                                                          environmentVariables: null);
                        server.WaitForConnection();
                        output = new StreamReader(server).ReadToEnd(); // blocks until .exe writes its one big block.
                        exitCode = 1; // anything non-zero
                    }
                }
                else
                {
                    using (var proc = Process.Start(new ProcessStartInfo(source, args) { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true }))
                    {
                        output = proc.StandardOutput.ReadToEnd();
                        proc.WaitForExit();
                        exitCode = proc.ExitCode;
                    }
                }

                if (exitCode == 0) // sum of passed + failed == 0 => no tests were run
                {                  // this should never happen, but just in case, report all tests as skipped
                    foreach(var testCase in testCases)
                    {
                        var result = new TestResult(testCase) { Outcome = TestOutcome.Skipped };
                        frameworkHandle.RecordStart (testCase);
                        frameworkHandle.RecordResult(result);
                        frameworkHandle.RecordEnd   (testCase, result.Outcome);
                    }
                } else
                    ReportResults(output, testCases, frameworkHandle);
            }
        }

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            if (sources == null || frameworkHandle == null) return;

            if (TestCache.IsEmpty())
            {   // it's possible to launch vstest.console.exe so that it doesn't call ITestDiscoverer first. Fill the TestCache ourselves
                TestDiscoverer discoverer = new TestDiscoverer();
                discoverer.DiscoverTests(sources, runContext, frameworkHandle, new NullObjectTestCaseDiscoverySink());
            }

            foreach (var source in sources)
            {
                using (var proc = Process.Start(new ProcessStartInfo(source, "") { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true }))
                {
                    string output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit();
                    ReportResults(output, TestCache.GetAll(source), frameworkHandle);
                }
            }
        }

        public void Cancel() { cancelRequested = true; }

        private void ReportResults(string output, IEnumerable<TestCase> subsetOfTests, IFrameworkHandle frameworkHandle)
        {
            var failedTestCases = new List<string>(output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries));

            foreach (var testCase in subsetOfTests)
            {
                string native = testCase.GetPropertyValue(PropertyRegistrar.MyStringProperty) as string;

                // Find the first failed test string that contains the native substring
                string matchedFailure = failedTestCases.FirstOrDefault(f => f?.IndexOf(native, StringComparison.OrdinalIgnoreCase) >= 0);
                bool isFailed = matchedFailure != null;

                var result = new TestResult(testCase);
                result.Outcome = isFailed ? TestOutcome.Failed : TestOutcome.Passed;
                if (isFailed)
                {
                    string message = matchedFailure; // message is stuff past filename(##)
                    message = message.Replace(native, "");
                    message = message.Replace(testCase.CodeFilePath, ""); // message is stuff past filename(##)
                    message = message.Substring(message.IndexOf(')') + 1); // get past ( + linenumber + ) part
                    message = message.Replace(" : warning unit-test: ", "");
                    message = message.Replace("\"\"", "");
                    message = message.Replace(" failed with: ", "");
                    result.ErrorMessage = message;

                    string filename = matchedFailure.Substring(0, matchedFailure.IndexOf('('));
                    int start       = matchedFailure.IndexOf('(') + 1;
                    int end         = matchedFailure.IndexOf(')', start);
                    int line        = int.Parse(matchedFailure.Substring(start, end - start));
                    string[] pieces = testCase.FullyQualifiedName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    result.ErrorStackTrace = "   at " + pieces[0] + '.' + pieces[1] + '.' + pieces[2] + "() in " + filename + ":line " + line;
                }
                frameworkHandle.RecordStart(testCase);
                frameworkHandle.RecordResult(result);
                frameworkHandle.RecordEnd(testCase, result.Outcome);
            }
        }
        private class NullObjectTestCaseDiscoverySink : ITestCaseDiscoverySink
        {
            public void SendTestCase(TestCase discoveredTest) { }
        };
    }
}