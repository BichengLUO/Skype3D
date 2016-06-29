using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace Skype4Sharp.Skype4SharpCore
{
    class ContactModule
    {
        private Skype4Sharp parentSkype;
        private UserModule userModule;
        public ContactModule(Skype4Sharp skypeToUse)
        {
            parentSkype = skypeToUse;
            userModule = new UserModule(parentSkype);
        }
        public List<User> getContacts()
        {
            List<User> toReturn = new List<User>();
            HttpRequestMessage webRequest = parentSkype.mainFactory.createWebRequest_GET("https://contacts.skype.com/contacts/v1/users/" + parentSkype.selfProfile.Username + "/contacts?$filter=type%20eq%20%27skype%27%20or%20type%20eq%20%27msn%27%20or%20type%20eq%20%27pstn%27%20or%20type%20eq%20%27agent%27&reason=default", new string[][] { new string[] { "X-Skypetoken", parentSkype.authTokens.SkypeToken } });
            string rawInfo = "";
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                var result = client.SendAsync(webRequest).Result;
                rawInfo = result.Content.ReadAsStringAsync().Result;
            }
            dynamic jsonObject = JsonConvert.DeserializeObject(rawInfo);
            foreach (dynamic singleUser in jsonObject.contacts)
            {
                toReturn.Add(userModule.userFromContacts(singleUser));
            }
            return toReturn;
        }
        public void addUser(string targetUsername, string requestMessage)
        {
            HttpRequestMessage webRequest = parentSkype.mainFactory.createWebRequest_PUT("https://api.skype.com/users/self/contacts/auth-request/" + targetUsername.ToLower(), new string[][] { new string[] { "X-Skypetoken", parentSkype.authTokens.SkypeToken } }, Encoding.ASCII.GetBytes("greeting=" + requestMessage.UrlEncode()), "application/x-www-form-urlencoded");
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                client.SendAsync(webRequest).Wait();
            }
        }
        public void deleteUser(string targetUsername)
        {
            HttpRequestMessage webRequest = parentSkype.mainFactory.createWebRequest_DELETE("https://contacts.skype.com/contacts/v1/users/" + parentSkype.selfProfile.Username + "/contacts/skype/" + targetUsername.ToLower(), new string[][] { new string[] { "X-Skypetoken", parentSkype.authTokens.SkypeToken } });
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                client.SendAsync(webRequest).Wait();
            }
        }
    }
}
