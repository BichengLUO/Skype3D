using System.Net.Http;
using System.Text;

namespace Skype4Sharp.Events
{
    public delegate void ContactRequestReceived(ContactRequest sentRequest);
    public class ContactRequest
    {
        private Skype4Sharp parentSkype;
        public User Sender;
        public string Body;
        public ContactRequest(Skype4Sharp skypeToUse)
        {
            parentSkype = skypeToUse;
        }
        public void Decline()
        {
            HttpRequestMessage declineRequest = parentSkype.mainFactory.createWebRequest_PUT("https://api.skype.com/users/self/contacts/auth-request/" + Sender.Username + "/decline", new string[][] { new string[] { "X-Skypetoken", parentSkype.authTokens.SkypeToken } }, Encoding.ASCII.GetBytes(""), "application/x-www-form-urlencoded");
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                client.SendAsync(declineRequest).Wait();
            }
        }
        public void Accept()
        {
            HttpRequestMessage acceptRequest = parentSkype.mainFactory.createWebRequest_PUT("https://api.skype.com/users/self/contacts/auth-request/" + Sender.Username + "/accept", new string[][] { new string[] { "X-Skypetoken", parentSkype.authTokens.SkypeToken } }, Encoding.ASCII.GetBytes(""), "application/x-www-form-urlencoded");
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                client.SendAsync(acceptRequest).Wait();
            }
        }
    }
}
