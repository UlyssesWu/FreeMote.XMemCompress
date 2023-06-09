using System;
using System.IO;
using System.Linq;
using XMemCompress;

namespace XMemTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var zip = File.ReadAllBytes(@"xmem-zip.bytes");
            //zip = zip.Skip(52).ToArray();
            //var ctx = new XMemCompress.DecompressionContext();
            //var content = ctx.Decompress(zip, 16345); //16345
            //File.WriteAllBytes("test.bytes", content);

            using var zip = File.OpenRead(@"xmem-zip.bytes");
            using var fs = File.OpenWrite("test2.bytes");
            XCompressFile.DecompressStream(zip, fs);
        }
    }
}