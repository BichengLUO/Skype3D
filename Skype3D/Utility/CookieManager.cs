using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Skype4Sharp.Auth;
using System.Collections;
using System.Reflection;

namespace Skype3D.Utility
{
    public class CookieManager
    {
        public static async void WriteTokenToDisk(string file, Tokens tokens)
        {
            StorageFolder temporaryFolder = ApplicationData.Current.TemporaryFolder;
            StorageFile tokensFile = await temporaryFolder.CreateFileAsync(file, CreationCollisionOption.ReplaceExisting);
            using (var tokensRandomAccess = await tokensFile.OpenAsync(FileAccessMode.ReadWrite))
            using (var tokensOutputStream = tokensRandomAccess.GetOutputStreamAt(0))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Tokens));
                serializer.Serialize(tokensOutputStream.AsStreamForWrite(), tokens);
            }
        }
        public static async Task<Tokens> ReadTokenFromDisk(string file)
        {
            StorageFolder temporaryFolder = ApplicationData.Current.TemporaryFolder;
            StorageFile tokensFile = await temporaryFolder.CreateFileAsync(file, CreationCollisionOption.OpenIfExists);
            using (var tokenInputStream = await tokensFile.OpenReadAsync())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Tokens));
                return (Tokens)serializer.Deserialize(tokenInputStream.AsStreamForRead());
            }   
        }
        public static async Task<bool> FileExists(string fileName)
        {
            StorageFolder temporaryFolder = ApplicationData.Current.TemporaryFolder;
            try { StorageFile file = await temporaryFolder.GetFileAsync(fileName); }
            catch { return false; }
            return true;
        }

        public static async Task<ulong> FileSize(string fileName)
        {
            StorageFolder temporaryFolder = ApplicationData.Current.TemporaryFolder;
            StorageFile file = await temporaryFolder.GetFileAsync(fileName);
            var basicProperties = await file.GetBasicPropertiesAsync();
            return basicProperties.Size;
        }

        public static async void RemoveFile(string fileName)
        {
            StorageFolder temporaryFolder = ApplicationData.Current.TemporaryFolder;
            try {
                StorageFile file = await temporaryFolder.GetFileAsync(fileName);
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch { }
        }
    }
}
