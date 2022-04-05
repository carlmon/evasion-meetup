using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Loader
{
    internal class Decoder
    {
        private static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];
            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        internal static string Decode(string input)
        {
            var bytes = Convert.FromBase64String(input);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            using (var gs = new GZipStream(msi, CompressionMode.Decompress))
            {
                CopyTo(gs, mso);
                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }
    }
}
