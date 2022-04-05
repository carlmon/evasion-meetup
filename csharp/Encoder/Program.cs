using System;

namespace Encoder
{
    class Program
    {
        static void Main(string[] args)
        {
            Encode("amsi.dll");
            Encode("AmsiScanBuffer");
        }

        private static void Encode(string decoded)
        {
            Console.WriteLine("{0}:\t{1}", decoded, Decoder.Encode(decoded));
        }
    }
}
