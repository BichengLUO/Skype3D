using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.IO;

namespace Skype4Sharp
{
    public class Chat
    {
        public string ID;
        public string ChatLink;
        public Enums.ChatType Type;
        public Skype4Sharp parentSkype;
        private string clientGatewayMessengerDomain = "https://bn2-client-s.gateway.messenger.live.com";
        public Chat(Skype4Sharp skypeToUse)
        {
            parentSkype = skypeToUse;
        }
        public User[] Participants
        {
            get
            {
                if (Type == Enums.ChatType.Private)
                {
                    return new User[] { parentSkype.selfProfile, parentSkype.GetUser(ID.Remove(0, 2)) };
                }
                HttpRequestMessage userListRequest = parentSkype.mainFactory.createWebRequest_GET(clientGatewayMessengerDomain + "/v1/threads/" + ID + "?view=msnp24Equivalent", new string[][] { new string[] { "RegistrationToken", parentSkype.authTokens.RegistrationToken } });
                string rawJSON = "";
                using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                    var result = client.SendAsync(userListRequest).Result;
                    rawJSON = result.Content.ReadAsStringAsync().Result;
                }
                dynamic decodedJSON = JsonConvert.DeserializeObject(rawJSON);
                List<string> allUsernames = new List<string>();
                foreach (dynamic singleUser in decodedJSON.members)
                {
                    allUsernames.Add(((string)singleUser.id).Remove(0, 2));
                }
                return parentSkype.getUsers(allUsernames.ToArray());
            }
        }
        public string Topic
        {
            get
            {
                HttpRequestMessage chatPropertyRequest = parentSkype.mainFactory.createWebRequest_GET(clientGatewayMessengerDomain + "/v1/threads/" + ID + "?view=msnp24Equivalent", new string[][] { new string[] { "RegistrationToken", parentSkype.authTokens.RegistrationToken } });
                string rawJSON = "";
                using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                    var result = client.SendAsync(chatPropertyRequest).Result;
                    rawJSON = result.Content.ReadAsStringAsync().Result;
                }
                dynamic decodedJSON = JsonConvert.DeserializeObject(rawJSON);
                return decodedJSON.properties.topic;
            }
            set
            {
                HttpRequestMessage topicChangeRequest = parentSkype.mainFactory.createWebRequest_PUT(clientGatewayMessengerDomain + "/v1/threads/" + ID + "/properties?name=topic", new string[][] { new string[] { "RegistrationToken", parentSkype.authTokens.RegistrationToken } }, Encoding.ASCII.GetBytes("{\"topic\":\"" + value.JsonEscape() + "\"}"), "application/json");
                using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                    client.SendAsync(topicChangeRequest).Wait();
                }
            }
        }
        public Enums.ChatRole Role
        {
            get
            {
                return getRole(parentSkype.selfProfile.Username);
            }
        }
        public void Kick(string usernameToKick)
        {
            checkChatType();
            HttpRequestMessage kickUserRequest = parentSkype.mainFactory.createWebRequest_DELETE(clientGatewayMessengerDomain + "/v1/threads/" + ID + "/members/8:" + usernameToKick.ToLower(), new string[][] { new string[] { "RegistrationToken", parentSkype.authTokens.RegistrationToken } });
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                client.SendAsync(kickUserRequest).Wait();
            }
        }
        public void Add(string usernameToAdd)
        {
            checkChatType();
            HttpRequestMessage addUserRequest = parentSkype.mainFactory.createWebRequest_PUT(clientGatewayMessengerDomain + "/v1/threads/" + ID + "/members/8:" + usernameToAdd.ToLower(), new string[][] { new string[] { "RegistrationToken", parentSkype.authTokens.RegistrationToken } }, Encoding.ASCII.GetBytes("{\"role\":\"User\"}"), "application/json");
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                client.SendAsync(addUserRequest).Wait();
            }
        }
        public void Leave()
        {
            Kick(parentSkype.selfProfile.Username);
        }
        public void SetAdmin(string usernameToPromote)
        {
            checkChatType();
            HttpRequestMessage addUserRequest = parentSkype.mainFactory.createWebRequest_PUT(clientGatewayMessengerDomain + "/v1/threads/" + ID + "/members/8:" + usernameToPromote.ToLower(), new string[][] { new string[] { "RegistrationToken", parentSkype.authTokens.RegistrationToken } }, Encoding.ASCII.GetBytes("{\"role\":\"Admin\"}"), "application/json");
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                client.SendAsync(addUserRequest).Wait();
            }
        }
        public Enums.ChatRole UserRole(string userToCheck)
        {
            return getRole(userToCheck);
        }
        private Enums.ChatRole getRole(string userToCheck)
        {
            if (Type == Enums.ChatType.Private)
            {
                return Enums.ChatRole.User;
            }
            HttpRequestMessage chatPropertyRequest = parentSkype.mainFactory.createWebRequest_GET(clientGatewayMessengerDomain + "/v1/threads/" + ID + "?view=msnp24Equivalent", new string[][] { new string[] { "RegistrationToken", parentSkype.authTokens.RegistrationToken } });
            string rawJSON = "";
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                var result = client.SendAsync(chatPropertyRequest).Result;
                rawJSON = result.Content.ReadAsStringAsync().Result;
            }
            dynamic decodedJSON = JsonConvert.DeserializeObject(rawJSON);
            foreach (dynamic singleUser in decodedJSON.members)
            {
                if (((string)singleUser.id) == "8:" + userToCheck.ToLower())
                {
                    string userRole = singleUser.role;
                    return (userRole == "User") ? Enums.ChatRole.User : Enums.ChatRole.Admin;
                }
            }
            return Enums.ChatRole.User;
        }
        private void checkChatType()
        {
            if (Type == Enums.ChatType.Private)
            {
                throw new Exceptions.InvalidSkypeActionException("This is not available in private messages");
            }
        }
    }
}
