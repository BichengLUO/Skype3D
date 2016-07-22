using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace Skype3D.CharacterUtil
{
    public class CharacterManager
    {
        public static string leancloudAPIDomain = "https://api.leancloud.cn/1.1/classes/";
        public static string tableName = "Characters";
        public static int NotFound = -1;
        public static int MaxCount = 9;

        public static async Task<bool> SetCharacter(int charID)
        {
            string charObjId = await FindCharacter(App.mainSkype.selfProfile);
            if (charObjId == null)
                return await CreateCharacter(charID);
            else
                return await UpdateCharacter(charObjId, charID);
        }

        public static async Task<bool> CreateCharacter(int charID)
        {
            string post_data = string.Format("{{\"skypename\":\"{0}\",\"charID\":{1}}}", App.mainSkype.selfProfile.Username, charID);
            HttpRequestMessage request = App.mainSkype.mainFactory.createWebRequest_POST(leancloudAPIDomain + tableName, new string[][] { new string[] { "X-LC-Id", LeanCloudKey.appID }, new string[] { "X-LC-Key", LeanCloudKey.appKey } }, Encoding.ASCII.GetBytes(post_data), "application/json");
            string rawJSON = "";
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", App.mainSkype.userAgent);
                var resuslt = await client.SendAsync(request);
                rawJSON = await resuslt.Content.ReadAsStringAsync();
            }
            dynamic decodedJSON = JsonConvert.DeserializeObject(rawJSON);
            return decodedJSON.createdAt != null;
        }

        public static async Task<bool> UpdateCharacter(string objId, int charID)
        {
            string put_data = string.Format("{{\"charID\":{0}}}", charID);
            string url = leancloudAPIDomain + tableName + "/" + objId;
            HttpRequestMessage request = App.mainSkype.mainFactory.createWebRequest_PUT(url, new string[][] { new string[] { "X-LC-Id", LeanCloudKey.appID }, new string[] { "X-LC-Key", LeanCloudKey.appKey } }, Encoding.ASCII.GetBytes(put_data), "application/json");
            string rawJSON = "";
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", App.mainSkype.userAgent);
                var resuslt = await client.SendAsync(request);
                rawJSON = await resuslt.Content.ReadAsStringAsync();
            }
            dynamic decodedJSON = JsonConvert.DeserializeObject(rawJSON);
            return decodedJSON.updatedAt != null;
        }
        public static async Task<string> FindCharacter(Skype4Sharp.User user)
        {
            string get_data = string.Format("{{\"skypename\":\"{0}\"}}", user.Username);
            string url = leancloudAPIDomain + tableName + "?where=" + WebUtility.UrlEncode(get_data);
            HttpRequestMessage request = App.mainSkype.mainFactory.createWebRequest_GET(url, new string[][] { new string[] { "X-LC-Id", LeanCloudKey.appID }, new string[] { "X-LC-Key", LeanCloudKey.appKey } });
            string rawJSON = "";
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", App.mainSkype.userAgent);
                var result = await client.SendAsync(request);
                rawJSON = await result.Content.ReadAsStringAsync();
            }
            dynamic decodedJSON = JsonConvert.DeserializeObject(rawJSON);
            dynamic results = decodedJSON.results;
            if (results.Count > 0)
                return results[0].objectId;
            else
                return null;
        }

        public static async Task<int> GetCharacter(Skype4Sharp.User user)
        {
            string get_data = string.Format("{{\"skypename\":\"{0}\"}}", user.Username);
            string url = leancloudAPIDomain + tableName + "?where=" + WebUtility.UrlEncode(get_data);
            HttpRequestMessage request = App.mainSkype.mainFactory.createWebRequest_GET(url, new string[][] { new string[] { "X-LC-Id", LeanCloudKey.appID }, new string[] { "X-LC-Key", LeanCloudKey.appKey } });
            string rawJSON = "";
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", App.mainSkype.userAgent);
                var result = await client.SendAsync(request);
                rawJSON = await result.Content.ReadAsStringAsync();
            }
            dynamic decodedJSON = JsonConvert.DeserializeObject(rawJSON);
            dynamic results = decodedJSON.results;
            if (results.Count > 0)
                return results[0].charID;
            else
                return NotFound;
        }

        public static int GetHashCharID(Skype4Sharp.User user)
        {
            int hash = 0;
            for (int i = 0; i < user.Username.Length; i++)
            {
                char ch = user.Username[i];
                hash += ch;
            }
            return hash % MaxCount;
        }

        public static async Task<Uri> GetCharAvatarForUser(Skype4Sharp.User user)
        {
            return new Uri("ms-appx:///Assets/CharacterAvatars/" + await GetCharIDForUser(user) + ".png");
        }

        public static async Task GetCharAvatarUrlsForUsers(List<Skype4Sharp.User> users)
        {
            for (int i = 0; i < users.Count; i++)
            {
                Skype4Sharp.User user = users[i];
                user.CharAvatarUri = await GetCharAvatarForUser(user);
            }
        }

        public static async Task GetCharAvatarUrlsForChats(List<Skype4Sharp.Chat> chats)
        {
            for (int i = 0; i < chats.Count; i++)
            {
                Skype4Sharp.Chat chat = chats[i];
                if (chat.Type == Skype4Sharp.Enums.ChatType.Group)
                    chat.CharAvatarUri = new Uri("ms-appx:///Assets/default-group-avatar.png");
                else
                    chat.CharAvatarUri = await GetCharAvatarForUser(chat.mainParticipant);
            }
        }

        public static async Task<int> GetCharIDForUser(Skype4Sharp.User user)
        {
            int charID = await GetCharacter(user);
            if (charID == NotFound)
                charID = GetHashCharID(user);
            return charID;
        }

        public static async Task<int[]> GetCharIDsForUsers(List<Skype4Sharp.User> users)
        {
            int[] result = new int[users.Count];
            for (int i = 0; i < users.Count; i++)
            {
                result[i] = await GetCharIDForUser(users[i]);
            }
            return result;
        }
    }
}
