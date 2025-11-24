using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Tdd20.TestAdapter
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    internal delegate int ListAllTestsDelegate([MarshalAs(UnmanagedType.BStr)] out string output);

    [DefaultExecutorUri("executor://Tdd20.TestAdapter")]
    [FileExtension(".exe")]
    [Category("native")]
    public class TestDiscoverer : ITestDiscoverer
    {        
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            if (sources == null) return;

            foreach (var source in sources)
            {
                using (var proc = Process.Start(new ProcessStartInfo(source, "-dump") { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true }))
                {
                    string line;
                    while ((line = proc.StandardOutput.ReadLine()) != null)
                    {
                        var testCase = TestCaseMaker.Make(line, source);
                        testCase.SetPropertyValue(PropertyRegistrar.MyStringProperty, line); // hang onto string from native code
                        discoverySink.SendTestCase(testCase);
                        TestCache.Add(source, testCase); // our cache, too.
                    }
                    proc.WaitForExit();
                }
            }
        }
    }
}
