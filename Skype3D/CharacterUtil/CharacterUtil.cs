using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace Skype3D.CharacterUtil
{
    public class CharacterUtil
    {
        public static string leancloudAPIDomain = "https://api.leancloud.cn/1.1/classes/";
        public static string tableName = "Characters";
        public static int NotFound = -1;

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
            if (results[0] != null)
                return results[0].charID;
            else
                return NotFound;
        }
    }
}
