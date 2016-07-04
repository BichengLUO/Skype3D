using System.Threading.Tasks;

namespace Skype4Sharp.Helpers
{
    public static class Extensions
    {
        public static async Task<ChatMessage> SendMessage(this Chat targetChat, string newMessage, Enums.MessageType messageType = Enums.MessageType.Text)
        {
            return await targetChat.parentSkype.SendMessage(targetChat, newMessage, messageType);
        }
        public static async Task<ChatMessage> SendMessage(this User targetUser, string newMessage, Enums.MessageType messageType = Enums.MessageType.Text)
        {
            return await targetUser.parentSkype.SendMessage(targetUser.Username, newMessage, messageType);
        }
        public static async Task Add(this Chat targetChat, User targetUser)
        {
            await targetChat.Add(targetUser.Username);
        }
        public static async Task Kick(this Chat targetChat, User targetUser)
        {
            await targetChat.Kick(targetUser.Username);
        }
        public static async Task Promote(this Chat targetChat, User targetUser)
        {
            await targetChat.SetAdmin(targetUser.Username);
        }
        public static async Task Add(this User targetUser, string requestMessage)
        {
            await targetUser.parentSkype.AddUser(targetUser.Username, requestMessage);
        }
        public static async Task Remove(this User targetUser)
        {
            await targetUser.parentSkype.RemoveUser(targetUser.Username);
        }
    }
}
