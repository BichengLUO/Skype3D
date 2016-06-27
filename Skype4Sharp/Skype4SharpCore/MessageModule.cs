using System.Net;
using System.Text;
using System.Net.Http;

namespace Skype4Sharp.Skype4SharpCore
{
    class MessageModule
    {
        private Skype4Sharp parentSkype;
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
    }
}
