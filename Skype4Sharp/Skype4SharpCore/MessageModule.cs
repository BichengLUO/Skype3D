using System;
using System.Text;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Skype4Sharp.Skype4SharpCore
{
    class MessageModule
    {
        private Skype4Sharp parentSkype;
        private string clientGatewayMessengerDomain = "https://client-s.gateway.messenger.live.com";
        public MessageModule(Skype4Sharp skypeToUse)
        {
            parentSkype = skypeToUse;
        }
        public async Task editMessage(ChatMessage messageInfo, string newMessage)
        {
            HttpRequestMessage webRequest = parentSkype.mainFactory.createWebRequest_POST(messageInfo.Chat.ChatLink + "/messages", new string[][] { new string[] { "RegistrationToken", parentSkype.authTokens.RegistrationToken }, new string[] { "X-Skypetoken", parentSkype.authTokens.SkypeToken } }, Encoding.ASCII.GetBytes("{\"content\":\"" + newMessage.JsonEscape() + "\",\"messagetype\":\"" + ((messageInfo.Type == Enums.MessageType.RichText) ? "RichText" : "Text") + "\",\"contenttype\":\"text\",\"skypeeditedid\":\"" + messageInfo.ID + "\"}"), "application/json");
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                await client.SendAsync(webRequest);
            }
        }
        public async Task<ChatMessage> createMessage(Chat targetChat, string chatMessage, Enums.MessageType messageType)
        {
            ChatMessage toReturn = new ChatMessage(parentSkype);
            toReturn.Body = chatMessage;
            toReturn.Chat = targetChat;
            toReturn.Type = messageType;
            toReturn.ID = Helpers.Misc.getTime().ToString();
            toReturn.Sender = parentSkype.selfProfile;
            await sendChatmessage(toReturn);
            return toReturn;
        }
        private async Task sendChatmessage(ChatMessage messageToSend)
        {
            HttpRequestMessage webRequest = parentSkype.mainFactory.createWebRequest_POST(messageToSend.Chat.ChatLink + "/messages", new string[][] { new string[] { "RegistrationToken", parentSkype.authTokens.RegistrationToken }, new string[] { "X-Skypetoken", parentSkype.authTokens.SkypeToken } }, Encoding.ASCII.GetBytes("{\"content\":\"" + messageToSend.Body.JsonEscape() + "\",\"messagetype\":\"" + ((messageToSend.Type == Enums.MessageType.RichText) ? "RichText" : "Text") + "\",\"contenttype\":\"text\",\"clientmessageid\":\"" + messageToSend.ID + "\"}"), "application/json");
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                var result = await client.SendAsync(webRequest);
            }
        }

        public async Task<List<Chat>> getRecent()
        {
            List<Chat> toReturn = new List<Chat>();
            string startTime = Convert.ToString(Helpers.Misc.getLastWeekTime());
            HttpRequestMessage webRequest = parentSkype.mainFactory.createWebRequest_GET(clientGatewayMessengerDomain + "/v1/users/ME/conversations?startTime=" + startTime + "&pageSize=100&view=msnp24Equivalent&targetType=Passport|Skype|Lync|Thread|PSTN", new string[][] { new string[] { "RegistrationToken", parentSkype.authTokens.RegistrationToken } });
            string rawInfo = "";
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                var result = await client.SendAsync(webRequest);
                rawInfo = await result.Content.ReadAsStringAsync();
            }
            dynamic jsonObject = JsonConvert.DeserializeObject(rawInfo);
            foreach (dynamic chat in jsonObject.conversations)
            {
                if (chat.lastMessage.conversationLink != null)
                    toReturn.Add(await chatFromRecent(chat));
            }
            return toReturn;
        }

        public async Task<Chat> chatFromRecent(dynamic jsonObject)
        {
            Chat toReturn = new Chat(parentSkype);
            toReturn.ChatLink = jsonObject.lastMessage.conversationLink;
            toReturn.ID = jsonObject.id;
            if (jsonObject.threadProperties != null)
            {
                toReturn.Topic = jsonObject.threadProperties.topic;
                toReturn.AvatarUri = new Uri("ms-appx:///Assets/default-group-avatar.png");
                toReturn.Type = Enums.ChatType.Group;
            }
            else
            {
                User user = await parentSkype.GetUser(toReturn.ID.Remove(0, 2));
                toReturn.mainParticipant = user;
                if (user.DisplayName != null && user.DisplayName != "")
                    toReturn.Topic = user.DisplayName;
                else
                    toReturn.Topic = user.Username;
                if (user.AvatarUri != null)
                    toReturn.AvatarUri = user.AvatarUri;
                else
                    toReturn.AvatarUri = new Uri("ms-appx:///Assets/default-avatar.png");
                toReturn.Type = Enums.ChatType.Private;
            }
            if (jsonObject.lastMessage != null)
                toReturn.LastMessage = toReturn.messageFromJson(jsonObject.lastMessage, await toReturn.getParticipants());
            return toReturn;
        }
    }
}
