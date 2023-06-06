using System.Diagnostics;
using System.IO;
using System.Reflection;
using System;
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
    internal static class XCompress
    {
        static XCompress()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetWinDllDirectory();
            }
        }

        private static void SetWinDllDirectory()
        {
            string path;

            var location = Assembly.GetExecutingAssembly().Location;
            if (string.IsNullOrEmpty(location) || (path = Path.GetDirectoryName(location)) == null)
            {
                Trace.TraceWarning($"{nameof(XCompress)}: Failed to get executing assembly location");
                return;
            }
            
            var platform = Environment.Is64BitProcess ? "x64" : "x86";
            var dir = Path.Combine(path, platform);
            if (File.Exists(Path.Combine(dir, DLL)) || File.Exists(Path.Combine(dir, DLL + ".dll")))
            {
                if (!SetDllDirectory(dir))
                    Trace.TraceWarning($"{nameof(XCompress)}: Failed to set DLL directory to '{path}'");
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetDllDirectory(string path);

        private const string DLL = "xcompress";

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