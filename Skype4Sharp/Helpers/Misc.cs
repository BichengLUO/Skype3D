using System;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using System.Text;

namespace Skype4Sharp.Helpers
{
    public class Misc
    {
        public static Int64 getTime()
        {
            Int64 returnValue = 0;
            var startTime = new DateTime(1970, 1, 1);
            TimeSpan timeSpan = (DateTime.Now.ToUniversalTime() - startTime);
            returnValue = (Int64)(timeSpan.TotalMilliseconds + 0.5);
            return returnValue;
        }

        public static byte[] hashMD5_Byte(string strToHash)
        {
            IBuffer buffUtf8Msg = CryptographicBuffer.ConvertStringToBinary(strToHash, BinaryStringEncoding.Utf8);
            HashAlgorithmProvider objAlgProv = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            IBuffer buffHash = objAlgProv.HashData(buffUtf8Msg);
            byte[] hashBytes;
            CryptographicBuffer.CopyToByteArray(buffHash, out hashBytes);
            return hashBytes;
        }
    }
}
