namespace Skype4Sharp
{
    public class ChatMessage
    {
        public User Sender { get; set; }
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
        public string Body
        {
            get { return _Body;}
            set { _Body = value; }
        }
    }
}
