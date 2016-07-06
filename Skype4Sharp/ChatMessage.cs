namespace Skype4Sharp
{
    public class ChatMessage
    {
        public User Sender;
        public Chat Chat;
        private string _Body;
        public string ID;
        public Enums.MessageType Type;
        private Skype4Sharp parentSkype;
        public ChatMessage(Skype4Sharp skypeToUse)
        {
            parentSkype = skypeToUse;
            _Body = null;
        }
        public string getBody()
        {
           return _Body;
        }
        public async void setBody(string value)
        {
            if (_Body == null)
            {
                _Body = value;
            }
            else
            {
                _Body = value;
                await parentSkype.editMessage(this, _Body);
            }
        }
    }
}
