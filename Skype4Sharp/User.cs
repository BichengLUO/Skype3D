using System;

namespace Skype4Sharp
{
    public class User
    {
        //add to this yourself. its very basic.
        public Enums.UserType Type;
        public string DisplayName { get; set; }
        public string Username { get; set; }
        public string Status { get; set; }
        public Uri AvatarUri { get; set; }
        public Skype4Sharp parentSkype;
        public User(Skype4Sharp skypeToUse)
        {
            parentSkype = skypeToUse;
        }
    }
}
