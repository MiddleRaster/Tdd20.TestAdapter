using System;
using System.Runtime.InteropServices;

namespace Tdd20.TestAdapter
{
    public sealed class NativeDll : IDisposable
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        private readonly IntPtr _hModule;

        public NativeDll(string path)
        {
            _hModule = LoadLibrary(path);
        }

        public T GetFunction<T>(string name) where T : Delegate
        {
            if (_hModule != IntPtr.Zero) {
                IntPtr proc = GetProcAddress(_hModule, name);
                if (proc != IntPtr.Zero)
                    return (T)Marshal.GetDelegateForFunctionPointer(proc, typeof(T));
            }
            return null;

            //if (_hModule == IntPtr.Zero)
            //    return null; // the TestPlatform expect us to fail silently if there are any errors

            //IntPtr proc = GetProcAddress(_hModule, name);
            //if (proc == IntPtr.Zero)
            //    return null;

            //return (T)Marshal.GetDelegateForFunctionPointer(proc, typeof(T));
        }

        public void Dispose()
        {
            if (_hModule != IntPtr.Zero)
                FreeLibrary(_hModule);
        }
    }
}
