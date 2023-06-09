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

    [StructLayout(LayoutKind.Explicit)]
    public struct XMEMCODEC_PARAMETERS_LZX
    {
        [FieldOffset(0)] public uint Flags;
        [FieldOffset(4)] public uint WindowSize;
        [FieldOffset(8)] public uint CompressionPartitionSize;
    }

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
        public static extern int XMemCreateDecompressionContext(XMEMCODEC_TYPE codec, XMEMCODEC_PARAMETERS_LZX param, int flags, ref IntPtr context);

        [DllImport(DLL, CallingConvention = CallingConvention.StdCall)]
        public static extern int XMemResetDecompressionContext(IntPtr context);

        [DllImport(DLL, CallingConvention = CallingConvention.StdCall)]
        public static extern int XMemDecompress(IntPtr context, byte[] dest, ref int destLen, byte[] src, int srcLen);

        [DllImport(DLL, CallingConvention = CallingConvention.StdCall)]
        public static extern void XMemDestroyDecompressionContext(IntPtr context);

        [DllImport(DLL, CallingConvention = CallingConvention.StdCall)]
        public static extern int XMemCreateCompressionContext(XMEMCODEC_TYPE codec, XMEMCODEC_PARAMETERS_LZX param, int flags, ref IntPtr context);

        [DllImport(DLL, CallingConvention = CallingConvention.StdCall)]
        public static extern int XMemResetCompressionContext(IntPtr context);

        [DllImport(DLL, CallingConvention = CallingConvention.StdCall)]
        public static extern int XMemCompress(IntPtr context, byte[] dest, ref int destLen, byte[] src, int srcSize);

        [DllImport(DLL, CallingConvention = CallingConvention.StdCall)]
        public static extern void XMemDestroyCompressionContext(IntPtr context);
    }
}