using System;

namespace XMemCompress
{
    public class DecompressionContext : IDisposable
    {
        private IntPtr _context;

        public DecompressionContext(XMEMCODEC_PARAMETERS_LZX codecParams, XMEMCODEC_TYPE codec = XMEMCODEC_TYPE.XMEMCODEC_LZX)
        {
            //XMEMCODEC_PARAMETERS_LZX codecParams;
            //codecParams.Flags = 0;
            //codecParams.WindowSize = 524288;//64 * 1024;
            //codecParams.CompressionPartitionSize = 524288;//256 * 1024;
            int ret;
            if ((ret = XCompress.XMemCreateDecompressionContext(codec, codecParams, 0, ref _context)) != 0)
                throw new XCompressException($"XMemCreateDecompressionContext returned non-zero value {ret}.");
        }

        public void Reset()
        {
            int ret;
            if ((ret = XCompress.XMemResetDecompressionContext(_context)) != 0)
                throw new XCompressException($"XMemResetDecompressionContext returned non-zero value {ret}.");
        }

        /// <summary>
        /// Decompresses compressed data.
        /// </summary>
        /// <param name="data">The data to decompress.</param>
        /// <param name="output">Where the decompressed data will put.</param>
        /// <returns>The total size of the compressed data.</returns>
        public void Decompress(byte[] data, ref byte[] output)
        {
            var targetLen = output.Length;
            var srcLen = data.Length;
            int ret;
            
            if ((ret = XCompress.XMemDecompress(_context, output, ref targetLen, data, srcLen)) != 0)
                throw new XCompressException($"XMemDecompress returned non-zero value {ret}.");
            Array.Resize(ref output, targetLen);
        }

        /// <summary>
        /// Decompresses compressed data.
        /// </summary>
        /// <param name="data">The data to decompress.</param>
        /// <param name="uncompressedSize">Length of the uncompressed data.</param>
        /// <returns>The decompressed data.</returns>
        public byte[] Decompress(byte[] data, int uncompressedSize)
        {
            var output = new byte[uncompressedSize];
            Decompress(data, ref output);
            return output;
        }

        public void Dispose()
        {
            XCompress.XMemDestroyDecompressionContext(_context);
        }
    }
}