using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace Common.Hashing
{
    public class CryptoHelper
    {
        // approach using MD5 and GUIDs
        public static Guid ComputeHash(string val)
        {
            var md5 = HashAlgorithmProvider.OpenAlgorithm("MD5");
            IBuffer buff = CryptographicBuffer.ConvertStringToBinary(val, BinaryStringEncoding.Utf8);
            var hashed = md5.HashData(buff);
            var hash = hashed.ToArray();

            Array.Resize(ref hash, 16);
            var guid = new Guid(hash);
            return guid;
        }
    }
}
