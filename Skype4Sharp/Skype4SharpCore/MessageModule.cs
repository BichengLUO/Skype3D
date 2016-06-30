﻿using System;
using System.Text;
using System.Net.Http;
using System.Collections.Generic;
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
        public void editMessage(ChatMessage messageInfo, string newMessage)
        {
            HttpRequestMessage webRequest = parentSkype.mainFactory.createWebRequest_POST(messageInfo.Chat.ChatLink + "/messages", new string[][] { new string[] { "RegistrationToken", parentSkype.authTokens.RegistrationToken }, new string[] { "X-Skypetoken", parentSkype.authTokens.SkypeToken } }, Encoding.ASCII.GetBytes("{\"content\":\"" + newMessage.JsonEscape() + "\",\"messagetype\":\"" + ((messageInfo.Type == Enums.MessageType.RichText) ? "RichText" : "Text") + "\",\"contenttype\":\"text\",\"skypeeditedid\":\"" + messageInfo.ID + "\"}"), "application/json");
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                client.SendAsync(webRequest).Wait();
            }
        }
        public ChatMessage createMessage(Chat targetChat, string chatMessage, Enums.MessageType messageType)
        {
            ChatMessage toReturn = new ChatMessage(parentSkype);
            toReturn.Body = chatMessage;
            toReturn.Chat = targetChat;
            toReturn.Type = messageType;
            toReturn.ID = Helpers.Misc.getTime().ToString();
            toReturn.Sender = parentSkype.selfProfile;
            sendChatmessage(toReturn);
            return toReturn;
        }
        private void sendChatmessage(ChatMessage messageToSend)
        {
            HttpRequestMessage webRequest = parentSkype.mainFactory.createWebRequest_POST(messageToSend.Chat.ChatLink + "/messages", new string[][] { new string[] { "RegistrationToken", parentSkype.authTokens.RegistrationToken }, new string[] { "X-Skypetoken", parentSkype.authTokens.SkypeToken } }, Encoding.ASCII.GetBytes("{\"content\":\"" + messageToSend.Body.JsonEscape() + "\",\"messagetype\":\"" + ((messageToSend.Type == Enums.MessageType.RichText) ? "RichText" : "Text") + "\",\"contenttype\":\"text\",\"clientmessageid\":\"" + messageToSend.ID + "\"}"), "application/json");
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                client.SendAsync(webRequest).Wait();
            }
        }

        public List<Chat> getRecent()
        {
            List<Chat> toReturn = new List<Chat>();
            string startTime = Convert.ToString(Helpers.Misc.getLastWeekTime());
            HttpRequestMessage webRequest = parentSkype.mainFactory.createWebRequest_GET(clientGatewayMessengerDomain + "/v1/users/ME/conversations?startTime=" + startTime + "&pageSize=100&view=msnp24Equivalent&targetType=Passport|Skype|Lync|Thread|PSTN", new string[][] { new string[] { "RegistrationToken", parentSkype.authTokens.RegistrationToken } });
            string rawInfo = "";
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                var result = client.SendAsync(webRequest).Result;
                rawInfo = result.Content.ReadAsStringAsync().Result;
            }
            dynamic jsonObject = JsonConvert.DeserializeObject(rawInfo);
            foreach (dynamic chat in jsonObject.conversations)
            {
                toReturn.Add(chatFromRecent(chat));
            }
            return toReturn;
        }

        public Chat chatFromRecent(dynamic jsonObject)
        {
            Chat toReturn = new Chat(parentSkype);
            toReturn.ChatLink = jsonObject.targetLink;
            toReturn.ID = jsonObject.id;
            if (jsonObject.threadProperties != null)
            {
                toReturn.Topic = jsonObject.threadProperties.topic;
                toReturn.AvatarUri = new Uri("ms-appx:///Assets/default-group-avatar.png");
            }
            else
            {
                User user = parentSkype.GetUser(toReturn.ID.Remove(0, 2));
                if (user.DisplayName != null && user.DisplayName != "")
                    toReturn.Topic = user.DisplayName;
                else
                    toReturn.Topic = user.Username;
                if (user.AvatarUri != null)
                    toReturn.AvatarUri = user.AvatarUri;
                else
                    toReturn.AvatarUri = new Uri("ms-appx:///Assets/default-avatar.png");
            }
            toReturn.LastMessage = jsonObject.lastMessage.content;
            return toReturn;
        }
    }
}
