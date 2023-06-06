using System;

namespace XMemTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var ctx = new XMemCompress.DecompressionContext();
            var content = ctx.Decompress(new byte[1], 1);
        }
    }
}