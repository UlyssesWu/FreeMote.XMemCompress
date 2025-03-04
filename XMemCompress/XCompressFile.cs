﻿using System;
using System.IO;
using System.Text;
using Be.IO;
// ReSharper disable MustUseReturnValue

namespace XMemCompress
{
    public class XCompressFile
    {
        public const uint BigMagic = 0x0FF512EE;
        public const uint LittleMagic = 0xEE12F50F;
        public bool BigEndian { get; set; } = true;
        public uint Version { get; set; } = 0x01030000;
        public uint Reserved { get; set; }

        public uint ContextFlags { get; set; } = 0;

        //16
        public uint WindowSize { get; set; } = 0x80000;
        public uint ChunkSize { get; set; } = 0x80000;
        public long UncompressedSize { get; set; }
        public long CompressedSize { get; set; }
        public int LargestUncompressedChunkSize { get; set; }
        public int LargestCompressedChunkSize { get; set; }

        public XMEMCODEC_PARAMETERS_LZX GetParameters()
        {
            return new XMEMCODEC_PARAMETERS_LZX
            {
                Flags = ContextFlags,
                WindowSize = WindowSize,
                CompressionPartitionSize = ChunkSize
            };
        }

        public static bool? IsBigEndian(Stream stream)
        {
            var pos = stream.Position;
            var bts = new byte[4];
            stream.Read(bts, 0, 4);
            stream.Position = pos;
            var m = BitConverter.ToUInt32(bts, 0); //it's little endian
            return m == BigMagic ? false : m == LittleMagic ? true : (bool?) null;
        }

        public static bool? IsBigEndian(byte[] stream)
        {
            var m = BitConverter.ToUInt32(stream, 0); //it's little endian
            return m == BigMagic ? false : m == LittleMagic ? true : (bool?) null;
        }

        public XCompressFile()
        {
        }

        public XCompressFile(string path)
        {
            using var fs = File.OpenRead(path);
            var e = IsBigEndian(fs);
            if (e == null)
                throw new FormatException("Invalid XCompress file");
            Init(fs, e.Value, false);
        }

        public XCompressFile(Stream input, bool bigEndian = true, bool leaveOpen = true)
        {
            Init(input, bigEndian, leaveOpen);
        }

        private void Init(Stream input, bool bigEndian = true, bool leaveOpen = true)
        {
            BigEndian = bigEndian;
            var br = bigEndian
                ? new BeBinaryReader(input, Encoding.Default, leaveOpen)
                : new BinaryReader(input, Encoding.Default, leaveOpen);
            br.ReadUInt32(); //Magic
            Version = br.ReadUInt32(); //Version
            Reserved = br.ReadUInt32(); //Reserved
            ContextFlags = br.ReadUInt32();
            WindowSize = br.ReadUInt32();
            ChunkSize = br.ReadUInt32();
            UncompressedSize = br.ReadInt64();
            CompressedSize = br.ReadInt64();
            LargestUncompressedChunkSize = br.ReadInt32();
            LargestCompressedChunkSize = br.ReadInt32();
        }

        public static XCompressFile DecompressStream(Stream stream, Stream output)
        {
            var bigEndian = IsBigEndian(stream);
            if (bigEndian == null)
                return null;
            var x = new XCompressFile(stream, bigEndian.Value);
            x.Decompress(stream, output);
            return x;
        }

        /// <summary>
        /// Compress to stream using default configuration
        /// <para>If you need detailed control, use <see cref="Compress"/> instead</para>
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static MemoryStream CompressStream(Stream input)
        {
            var x = new XCompressFile();
            var ms = new MemoryStream();
            x.Compress(input, ms);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public void Compress(Stream input, Stream output)
        {
            var uncompressedSize = input.Length;
            var bw = BigEndian ? new BeBinaryWriter(output, Encoding.Default, true) : new BinaryWriter(output, Encoding.Default, true);
            bw.Write(BigMagic);
            bw.Write(Version);
            bw.Write(Reserved);
            bw.Write(ContextFlags);
            bw.Write(WindowSize);
            bw.Write(ChunkSize);
            var pos = bw.BaseStream.Position;
            bw.Write(UncompressedSize);
            bw.Write(CompressedSize);
            bw.Write(LargestUncompressedChunkSize);
            bw.Write(LargestCompressedChunkSize);
            using var context = new CompressionContext(GetParameters());
            var remaining = input.Length;
            long compressedSize = 0;
            int largestUncompressed = 0;
            int largestCompressed = 0;
            while (remaining > 0)
            {
                var chunkSize = (int) Math.Min(remaining, ChunkSize);
                if (chunkSize > largestUncompressed)
                {
                    largestUncompressed = chunkSize;
                }

                var chunk = new byte[chunkSize];
                input.Read(chunk, 0, chunkSize);
                remaining -= chunkSize;
                var zip = context.Compress(chunk, chunkSize);
                if (zip.Length > largestCompressed)
                {
                    largestCompressed = zip.Length;
                }

                compressedSize += zip.Length;
                bw.Write(zip.Length);
                bw.Write(zip);
                //context.Reset();
            }

            UncompressedSize = uncompressedSize;
            CompressedSize = compressedSize;
            LargestUncompressedChunkSize = largestUncompressed;
            LargestCompressedChunkSize = largestCompressed;

            bw.BaseStream.Position = pos;
            bw.Write(uncompressedSize);
            bw.Write(compressedSize);
            bw.Write(largestUncompressed);
            bw.Write(largestCompressed);

            bw.Flush();
        }

        public void Decompress(Stream input, Stream output)
        {
            var br = BigEndian ? new BeBinaryReader(input, Encoding.Default, true) : new BinaryReader(input, Encoding.Default, true);
            var remaining = CompressedSize;
            if (LargestUncompressedChunkSize <= 0)
            {
                LargestUncompressedChunkSize = (int)ChunkSize;
            }

            using var context = new DecompressionContext(GetParameters());
            while (remaining > 0)
            {
                var chunkSize = br.ReadInt32();
                remaining -= 4;
                var zipContent = br.ReadBytes(chunkSize);
                remaining -= chunkSize;
                var unzip = context.Decompress(zipContent, LargestUncompressedChunkSize);
                output.Write(unzip, 0, unzip.Length);
                //context.Reset();
            }

            output.Flush();
        }
    }
}