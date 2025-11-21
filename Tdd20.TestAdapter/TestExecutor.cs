using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Tdd20.TestAdapter
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    internal delegate int RunSingleTestDelegate([MarshalAs(UnmanagedType.BStr)] string testName,
                                                [MarshalAs(UnmanagedType.BStr)] out string filename,
                                                out int line,
                                                [MarshalAs(UnmanagedType.BStr)] out string message);

    [FileExtension(".dll")]
    [FileExtension("dll")]
    [ExtensionUri("executor://Tdd20.TestAdapter")]
    public class TestExecutor : ITestExecutor
    {
        private static readonly string[] separator = new[] { "\r\n", "\n" };
        private bool _cancelRequested = false;

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            if (tests == null || frameworkHandle == null) return;

            TestCase first = tests.FirstOrDefault();
            if (first == null)
                return;

            var source = first.Source;
            var dll = new NativeDll(source);
            var runSingleTest = dll.GetFunction<RunSingleTestDelegate>("RunSingleTest");

            try
            {
                foreach (var test in tests)
                {
                    if (_cancelRequested) return;

                    if (test.Source != source)
                    {
                        dll.Dispose();
                        source = first.Source;
                        dll = new NativeDll(source);
                        runSingleTest = dll.GetFunction<RunSingleTestDelegate>("RunSingleTest");
                    }

                    if (runSingleTest != null)
                    {
                        frameworkHandle.RecordStart(test);

                        // piece the VsTest name back together again
                        string vsTestName = test.DisplayName + '?' + test.CodeFilePath + '?' + test.LineNumber.ToString();
                        int hr = runSingleTest(vsTestName, out string filename, out int line, out string message);
                        if (hr != 0)
                        {
                            frameworkHandle.RecordEnd(test, TestOutcome.NotFound);
                        }
                        else if (filename == "")
                        {
                            TestResult result = new TestResult(test)
                            {
                                Outcome = TestOutcome.Passed,
                                //Duration = TimeSpan.FromMilliseconds(5)
                            };
                            frameworkHandle.RecordResult(result);
                            frameworkHandle.RecordEnd(test, TestOutcome.Passed);
                        }
                        else
                        {
                            string[] pieces = test.FullyQualifiedName.Split(new char[] {'.'}, StringSplitOptions.RemoveEmptyEntries);

                            // trim message
                            message = message.Replace(" : warning unit-test: ", "");
                            message = message.Replace(test.GetPropertyValue(PropertyRegistrar.MyStringProperty) as string, "");
                            message = message.Replace("\"\"", "");
                            message = message.Replace(" failed with: ", "");

                            TestResult result = new TestResult(test)
                            {
                                Outcome = TestOutcome.Failed,
                                //Duration = TimeSpan.FromMilliseconds(5),
                                ErrorMessage = message,
                                ErrorStackTrace = "   at " + pieces[0] + '.' + pieces[1] + '.' + pieces[2] + "() in " + filename + ":line " + line,
                            };
                            frameworkHandle.RecordResult(result);
                            frameworkHandle.RecordEnd(test, TestOutcome.Failed);
                        }
                    }
                }
            }
            finally
            {
                dll?.Dispose();
            }
        }

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            if (sources == null || frameworkHandle == null) return;

            var tests = new List<TestCase>();
            foreach (var source in sources)
            {
                using (var dll = new NativeDll(source))
                {
                    var listAllTests = dll.GetFunction<ListAllTestsDelegate>("ListAllTests");
                    if (listAllTests != null)
                    {
                        int hr = listAllTests(out string output);
                        if (hr == 0)
                        {
                            string[] testNames = output.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var testname in testNames)
                            {
                                var testCase = TestCaseMaker.Make(testname, source);
                                testCase.SetPropertyValue(PropertyRegistrar.MyStringProperty, testname); // hang onto string from native code
                                tests.Add(testCase);
                            }
                        }
                    }
                }
            }
            RunTests(tests, runContext, frameworkHandle);
        }

        public void Cancel()
        {
            _cancelRequested = true;
        }
    }
}