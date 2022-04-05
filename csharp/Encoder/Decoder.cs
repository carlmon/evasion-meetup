using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Encoder
{
    internal class Decoder
    {
        private static void CopyTo(Stream src, Stream dest)
        {
            byte[] buffer = new byte[4096];
            int cnt;

            while ((cnt = src.Read(buffer, 0, buffer.Length)) != 0)
            {
                dest.Write(buffer, 0, cnt);
            }
        }

        public static string Decode(string input)
        {
            var bytes = Convert.FromBase64String(input);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

        internal static string Encode(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    gs.Write(bytes, 0, bytes.Length);
                }

                return Convert.ToBase64String(mso.ToArray());
            }
        }
    }
}
