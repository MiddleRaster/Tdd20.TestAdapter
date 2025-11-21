using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Tdd20.TestAdapter
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    internal delegate int ListAllTestsDelegate([MarshalAs(UnmanagedType.BStr)] out string output);

    [DefaultExecutorUri("executor://Tdd20.TestAdapter")]
    [FileExtension(".dll")]
    [Category("native")]
    public class TestDiscoverer : ITestDiscoverer
    {
        private static readonly string[] separator = new[] { "\r\n", "\n" };

        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            if (sources == null) return;

            foreach (var source in sources)
            {
                using (var dll = new NativeDll(source))
                {
                    var listAllTests  = dll.GetFunction<ListAllTestsDelegate>("ListAllTests");
                    if (listAllTests != null)
                    {
                        int hr = listAllTests(out string output); 
                        if (hr == 0)
                        {
                            string[] testNames = output.Split(separator, StringSplitOptions.RemoveEmptyEntries); 
                            var tests = new List<TestCase>();
                            foreach (var testname in testNames) 
                            {
                                var testCase = TestCaseMaker.Make(testname, source);
                                testCase.SetPropertyValue(PropertyRegistrar.MyStringProperty, testname); // hang onto string from native code
                                discoverySink.SendTestCase(testCase);
                            }
                        }
                    }
                }
            }
        }
    }
}
