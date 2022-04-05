using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace LoaderDynamic
{
    internal class Dynamic
    {
        private static readonly Type _nativeMethods;

        static Dynamic()
        {
            // Microsoft.Win32.UnsafeNativeMethods
            _nativeMethods = AppDomain.CurrentDomain.GetAssemblies()
                .First(a => a.Location.EndsWith("System.dll"))
                .GetTypes()
                .First(a => a.Name.EndsWith("eMethods"));
        }

        private static IntPtr GetFunctionPointer(string moduleName, string procName)
        {
            var module = (IntPtr)_nativeMethods.GetMethod("GetModuleHandle").Invoke(null, new[] { moduleName });
            return GetFunctionPointer(module, procName);
        }

        internal static IntPtr GetFunctionPointer(IntPtr module, string procName)
        {
            var getAddr = _nativeMethods.GetMethod("GetProcAddress", new Type[] { typeof(IntPtr), typeof(string) });
            var res = getAddr.Invoke(null, new object[] { module, procName });
            return (IntPtr)res;
        }

        internal static U GetFunction<U>(string procName)
        {
            var addr = GetFunctionPointer("kernel32.dll", procName);
            return Marshal.GetDelegateForFunctionPointer<U>(addr);
        }
    }
}
