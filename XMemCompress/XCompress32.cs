using System.Runtime.InteropServices;

namespace XMemCompress
{
    public enum XMEMCODEC_TYPE : int
    {
        XMEMCODEC_DEFAULT = 0,
        XMEMCODEC_LZX = 1
    }

    // TODO: What are those parameters?
    // TODO: What are those flags?
    internal class XCompress32
    {
        private const string DLL = "xcompress32.dll";

        [DllImport(DLL, CallingConvention = CallingConvention.StdCall)]
        public static extern int XMemCreateDecompressionContext(XMEMCODEC_TYPE codec, int param, int flags, ref int context);

        [DllImport(DLL, CallingConvention = CallingConvention.StdCall)]
        public static extern int XMemResetDecompressionContext(int context);

        [DllImport(DLL, CallingConvention = CallingConvention.StdCall)]
        public static extern int XMemDecompress(int context, byte[] dest, ref int destLen, byte[] src, int srcLen);

        [DllImport(DLL, CallingConvention = CallingConvention.StdCall)]
        public static extern void XMemDestroyDecompressionContext(int context);

        [DllImport(DLL, CallingConvention = CallingConvention.StdCall)]
        public static extern int XMemCreateCompressionContext(XMEMCODEC_TYPE codec, int param, int flags, ref int context);

        [DllImport(DLL, CallingConvention = CallingConvention.StdCall)]
        public static extern int XMemResetCompressionContext(int context);

        [DllImport(DLL, CallingConvention = CallingConvention.StdCall)]
        public static extern int XMemCompress(int context, byte[] dest, ref int destLen, byte[] src, int srcSize);

        [DllImport(DLL, CallingConvention = CallingConvention.StdCall)]
        public static extern void XMemDestroyCompressionContext(int context);
    }
}