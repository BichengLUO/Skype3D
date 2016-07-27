using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using Skype4Sharp.Auth;
using Skype4Sharp.Enums;
using Skype4Sharp.Events;
using Skype4Sharp.Exceptions;
using Skype4Sharp.Helpers;
using Skype4Sharp.Skype4SharpCore;

namespace Skype4Sharp
{
    public class Skype4Sharp
    {
        public event MessageReceived messageReceived;
        public event ContactReceived contactReceived;
        public event MessageEdited messageEdited;
        public event ChatMembersChanged chatMembersChanged;
        public event TopicChange topicChange;
        public event ContactRequestReceived contactRequestReceived;
        public event CallStarted callStarted;
        public event FileReceived fileReceived;
        public event ChatPictureChanged chatPictureChanged;
        public event UserRoleChanged userRoleChanged;

        public SkypeCredentials authInfo;
        public Tokens authTokens = new Tokens();
        public User selfProfile;
        public CookieContainer mainCookies;
        public HttpRequestFactory mainFactory;
        public string userAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.95 Safari/537.36";
        public LoginState authState = LoginState.Unknown;
        public SkypeTokenType tokenType = SkypeTokenType.Standard;
        public bool ignoreOwnEvents = true;
        public bool isPolling = false;

        private Poller mainPoll;
        private UserModule mainUserModule;
        private AuthModule mainAuthModule;
        private MessageModule mainMessageModule;
        private ContactModule mainContactModule;
        private string clientGatewayMessengerDomain = "https://client-s.gateway.messenger.live.com";
        public Skype4Sharp(SkypeCredentials loginData)
        {
            authInfo = loginData;
            mainCookies = new CookieContainer();
            mainFactory = new HttpRequestFactory();
            mainPoll = new Poller(this);
            selfProfile = new User(this);
            mainUserModule = new UserModule(this);
            mainAuthModule = new AuthModule(this);
            mainMessageModule = new MessageModule(this);
            mainContactModule = new ContactModule(this);
        }
        public Skype4Sharp(Tokens tokens)
        {
            authTokens = tokens;
            mainCookies = new CookieContainer();
            mainFactory = new HttpRequestFactory();
            mainPoll = new Poller(this);
            selfProfile = new User(this);
            mainUserModule = new UserModule(this);
            mainAuthModule = new AuthModule(this);
            mainMessageModule = new MessageModule(this);
            mainContactModule = new ContactModule(this);
        }
        public void StartPoll()
        {
            blockUnauthorized();
            isPolling = true;
            mainPoll.StartPoll();
        }
        public void StopPoll()
        {
            blockUnauthorized();
            isPolling = false;
            mainPoll.StopPoll();
        }
        public async Task<bool> Login(bool bypassLogin = false)
        {
            if (!bypassLogin)
            {
                if (authState == LoginState.Success)
                {
                    throw new InvalidSkypeActionException("You are already signed in");
                }
                return await mainAuthModule.Login();
            }
            else
            {
                mainPoll.StopPoll();
                bool loginSuccess = await mainAuthModule.Login();
                mainPoll.StartPoll();
                return loginSuccess;
            }
        }
        public async Task Logout()
        {
            mainPoll.StopPoll();
            await mainAuthModule.Logout();
        }
        public async Task<ChatMessage> SendMessage(Chat targetChat, string newMessage, MessageType messageType = MessageType.Text)
        {
            blockUnauthorized();
            return await mainMessageModule.createMessage(targetChat, newMessage, messageType);
        }
        public async Task<ChatMessage> SendMessage(User targetUser, string newMessage, MessageType messageType = MessageType.Text)
        {
            blockUnauthorized();
            Chat targetChat = new Chat(this);
            if (targetUser.Username.StartsWith("8:"))
                targetChat.ID = targetUser.Username.ToLower();
            else
                targetChat.ID = "8:" + targetUser.Username.ToLower();
            targetChat.ChatLink = clientGatewayMessengerDomain + "/v1/users/ME/conversations/" + targetChat.ID;
            targetChat.Type = Enums.ChatType.Private;
            return await mainMessageModule.createMessage(targetChat, newMessage, messageType);
        }
        public async Task<ChatMessage> SendMessage(string targetUser, string newMessage, MessageType messageType = MessageType.Text)
        {
            blockUnauthorized();
            Chat targetChat = new Chat(this);
            targetChat.ID = "8:" + targetUser.ToLower();
            targetChat.ChatLink = clientGatewayMessengerDomain + "/v1/users/ME/conversations/" + targetChat.ID;
            targetChat.Type = Enums.ChatType.Private;
            return await mainMessageModule.createMessage(targetChat, newMessage, messageType);
        }
        public async Task<User> GetUser(string inputName)
        {
            blockUnauthorized();
            return await mainUserModule.getUser(inputName);
        }
        public async Task AddUser(string targetUser, string requestMessage)
        {
            blockUnauthorized();
            await mainContactModule.addUser(targetUser, requestMessage);
        }
        public async Task RemoveUser(string targetUser)
        {
            blockUnauthorized();
            await mainContactModule.deleteUser(targetUser);
        }
        public async Task<List<User>> GetContacts()
        {
            blockUnauthorized();
            return await mainContactModule.getContacts();
        }
        public async Task<List<Chat>> GetRecent()
        {
            blockUnauthorized();
            return await mainMessageModule.getRecent();
        }

        public async Task editMessage(ChatMessage originalMessage, string newMessage)
        {
            await mainMessageModule.editMessage(originalMessage, newMessage);
        }
        public async Task<User[]> getUsers(string[] inputNames)
        {
            blockUnauthorized();
            return await mainUserModule.getUsers(inputNames);
        }
        public void invokeMessageReceived(ChatMessage pMessage)
        {
            if (messageReceived == null) { return; }
            if (ignoreOwnEvents)
            {
                if (pMessage.Sender.Username == selfProfile.Username)
                {
                    return;
                }
            }
            messageReceived.Invoke(pMessage);
        }
        public void invokeContactReceived(User receivedUser, Chat originChat, User originUser)
        {
            if (contactReceived == null) { return; }
            if (ignoreOwnEvents)
            {
                if (originUser.Username == selfProfile.Username)
                {
                    return;
                }
            }
            contactReceived.Invoke(receivedUser, originChat, originUser);
        }
        public void invokeMessageEdited(ChatMessage pMessage)
        {
            if (messageEdited == null) { return; }
            if (ignoreOwnEvents)
            {
                if (pMessage.Sender.Username == selfProfile.Username)
                {
                    return;
                }
            }
            messageEdited.Invoke(pMessage);
        }
        public void invokeChatMembersChanged(Chat newChat, User eventInitiator, User eventTarget, ChatMemberChangedType changeType)
        {
            if (chatMembersChanged == null) { return; }
            chatMembersChanged.Invoke(newChat, eventInitiator, eventTarget, changeType);
        }
        public void invokeTopicChange(Chat targetChat, User eventInitiator, string newTopic)
        {
            if (topicChange == null) { return; }
            topicChange.Invoke(targetChat, eventInitiator, newTopic);
        }
        public void invokeContactRequestReceived(ContactRequest sentRequest)
        {
            if (contactRequestReceived == null) { return; }
            contactRequestReceived.Invoke(sentRequest);
        }
        public void invokeCallStarted(Chat originChat, User eventInitiator)
        {
            if (callStarted == null) { return; }
            callStarted.Invoke(originChat, eventInitiator);
        }
        public void invokeFileReceived(Events.SkypeFile sentFile)
        {
            if (fileReceived == null) { return; }
            fileReceived.Invoke(sentFile);
        }
        public void invokeChatPictureChanged(Chat targetChat, User eventInitiator, string newPicture)
        {
            if (chatPictureChanged == null) { return; }
            chatPictureChanged.Invoke(targetChat, eventInitiator, newPicture);
        }
        public void invokeUserRoleChanged(Chat newChat, User eventInitiator, User eventTarget, Enums.ChatRole newRole)
        {
            if (userRoleChanged == null) { return; }
            userRoleChanged.Invoke(newChat, eventInitiator, eventTarget, newRole);
        }
        private void blockUnauthorized()
        {
            if (authState != LoginState.Success)
            {
                throw new InvalidSkypeActionException("Not signed in");
            }
        }
    }
}