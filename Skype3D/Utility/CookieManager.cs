using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using System.Collections;
using System.Reflection;

namespace Skype3D.Utility
{
    public class CookieManager
    {
        public static async void WriteCookiesToDisk(string file, CookieContainer cookieJar)
        {
            StorageFolder temporaryFolder = ApplicationData.Current.TemporaryFolder;
            StorageFile cookieFile = await temporaryFolder.CreateFileAsync(file, CreationCollisionOption.ReplaceExisting);
            var cookieRandomAccess = await cookieFile.OpenAsync(FileAccessMode.ReadWrite);
            var cookieOutputStream = cookieRandomAccess.GetOutputStreamAt(0);
            XmlSerializer serializer = new XmlSerializer(typeof(CookieContainer));
            serializer.Serialize(cookieOutputStream.AsStreamForWrite(), cookieJar);
        }

        public static async Task<CookieContainer> ReadCookiesFromDisk(string file)
        {
            StorageFolder temporaryFolder = ApplicationData.Current.TemporaryFolder;
            StorageFile cookieFile = await temporaryFolder.CreateFileAsync(file, CreationCollisionOption.OpenIfExists);
            var cookieInputStream = await cookieFile.OpenReadAsync();
            XmlSerializer serializer = new XmlSerializer(typeof(CookieContainer));
            return (CookieContainer)serializer.Deserialize(cookieInputStream.AsStreamForRead());
        }

        public static async Task<bool> FileExists(string fileName)
        {
            StorageFolder temporaryFolder = ApplicationData.Current.TemporaryFolder;
            try { StorageFile file = await temporaryFolder.GetFileAsync(fileName); }
            catch { return false; }
            return true;
        }
    }
}
