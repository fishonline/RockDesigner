using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace Rock.Orm.Common
{
    /// <summary>
    /// Compress Manager
    /// </summary>
    public sealed class CompressionManager
    {
        private CompressionManager()
        {
        }

        private static CN.Rock.Compression.CompressionManager singleton = new CN.Rock.Compression.CompressionManager();

        /// <summary>
        /// Compresses the specified STR.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns></returns>
        public static string Compress(string str)
        {
            return singleton.CompressGZip(str);
        }

        /// <summary>
        /// Decompress the specified STR.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns></returns>
        public static string Decompress(string str)
        {
            return singleton.DecompressGZip(str);
        }

        /// <summary>
        /// 7Zip Compress the str.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns></returns>
        public static string Compress7Zip(string str)
        {
            return singleton.Compress7Zip(str);
        }

        /// <summary>
        /// 7Zip Decompress the str.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns></returns>
        public static string Decompress7Zip(string str)
        {
            return singleton.Decompress7Zip(str);
        }
    }
}
