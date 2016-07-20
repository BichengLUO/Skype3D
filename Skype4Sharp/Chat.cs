using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
using System;

namespace Skype4Sharp
{
    public class Chat
    {
        public string ID;
        public string ChatLink { get; set; }
        public ChatMessage LastMessage { get; set; }
        public string Topic { get; set; }
        public User mainParticipant { get; set; }
        public bool Unread { get; set; }
        public Uri AvatarUri { get; set; }
        public Uri CharAvatarUri { get; set; }
        public Enums.ChatType Type;
        public Skype4Sharp parentSkype;
        private string clientGatewayMessengerDomain = "https://client-s.gateway.messenger.live.com";
        public Chat(Skype4Sharp skypeToUse)
        {
            parentSkype = skypeToUse;
        }
        public async Task<List<ChatMessage>> getMessageHistory()
        {
            User[] participants = await getParticipants();
            HttpRequestMessage userListRequest = parentSkype.mainFactory.createWebRequest_GET(clientGatewayMessengerDomain + "/v1/users/ME/conversations/" + ID + "/messages?startTime=0&pageSize=51&view=msnp24Equivalent", new string[][] { new string[] { "RegistrationToken", parentSkype.authTokens.RegistrationToken } });
            string rawJSON = "";
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                var result = await client.SendAsync(userListRequest);
                rawJSON = await result.Content.ReadAsStringAsync();
            }
            List<ChatMessage> messages = new List<ChatMessage>();
            dynamic decodedJSON = JsonConvert.DeserializeObject(rawJSON);
            foreach (dynamic message in decodedJSON.messages)
            {
                if (message.messagetype == "Text" || message.messagetype == "RichText")
                    messages.Add(messageFromJson(message, participants));
            }
            messages.Reverse();
            return messages;
        }
        public async Task<User[]> getParticipants()
        {
            if (Type == Enums.ChatType.Private)
            {
                return new User[] { parentSkype.selfProfile, await parentSkype.GetUser(ID.Remove(0, 2)) };
            }
            HttpRequestMessage userListRequest = parentSkype.mainFactory.createWebRequest_GET(clientGatewayMessengerDomain + "/v1/threads/" + ID + "?view=msnp24Equivalent", new string[][] { new string[] { "RegistrationToken", parentSkype.authTokens.RegistrationToken } });
            string rawJSON = "";
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                var result = await client.SendAsync(userListRequest);
                rawJSON = await result.Content.ReadAsStringAsync();
            }
            dynamic decodedJSON = JsonConvert.DeserializeObject(rawJSON);
            List<string> allUsernames = new List<string>();
            foreach (dynamic singleUser in decodedJSON.members)
            {
                allUsernames.Add(((string)singleUser.id).Remove(0, 2));
            }
            return await parentSkype.getUsers(allUsernames.ToArray());
        }
        public async Task<Enums.ChatRole> getRole()
        {
            return await getRole(parentSkype.selfProfile.Username);
        }
        public async Task Kick(string usernameToKick)
        {
            checkChatType();
            HttpRequestMessage kickUserRequest = parentSkype.mainFactory.createWebRequest_DELETE(clientGatewayMessengerDomain + "/v1/threads/" + ID + "/members/8:" + usernameToKick.ToLower(), new string[][] { new string[] { "RegistrationToken", parentSkype.authTokens.RegistrationToken } });
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                await client.SendAsync(kickUserRequest);
            }
        }
        public async Task Add(string usernameToAdd)
        {
            checkChatType();
            HttpRequestMessage addUserRequest = parentSkype.mainFactory.createWebRequest_PUT(clientGatewayMessengerDomain + "/v1/threads/" + ID + "/members/8:" + usernameToAdd.ToLower(), new string[][] { new string[] { "RegistrationToken", parentSkype.authTokens.RegistrationToken } }, Encoding.ASCII.GetBytes("{\"role\":\"User\"}"), "application/json");
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                await client.SendAsync(addUserRequest);
            }
        }
        public async Task Leave()
        {
            await Kick(parentSkype.selfProfile.Username);
        }
        public async Task SetAdmin(string usernameToPromote)
        {
            checkChatType();
            HttpRequestMessage addUserRequest = parentSkype.mainFactory.createWebRequest_PUT(clientGatewayMessengerDomain + "/v1/threads/" + ID + "/members/8:" + usernameToPromote.ToLower(), new string[][] { new string[] { "RegistrationToken", parentSkype.authTokens.RegistrationToken } }, Encoding.ASCII.GetBytes("{\"role\":\"Admin\"}"), "application/json");
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                await client.SendAsync(addUserRequest);
            }
        }
        public async Task<Enums.ChatRole> UserRole(string userToCheck)
        {
            return await getRole(userToCheck);
        }
        private async Task<Enums.ChatRole> getRole(string userToCheck)
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
                var result = await client.SendAsync(chatPropertyRequest);
                rawJSON = await result.Content.ReadAsStringAsync();
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

        public ChatMessage messageFromJson(dynamic jsonObject, User[] participants)
        {
            ChatMessage message = new ChatMessage(parentSkype);
            message.Chat = this;
            message.ID = jsonObject.clientmessageid;
            if (jsonObject.messagetype == "Text")
            {
                message.Type = Enums.MessageType.Text;
                message.Body = jsonObject.content;
            } 
            else if (jsonObject.messagetype == "RichText")
            {
                message.Type = Enums.MessageType.RichText;
                message.Body = jsonObject.content;
            }
            foreach (User user in participants)
            {
                if (((string)jsonObject.from).Contains(user.Username))
                {
                    message.Sender = user;
                    break;
                }
            }
            return message;
        }
    }
}
