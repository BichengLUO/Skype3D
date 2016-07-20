using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Skype4Sharp.Skype4SharpCore
{
    class AuthModule
    {
        private Skype4Sharp parentSkype;
        private string clientGatewayMessengerDomain = "https://client-s.gateway.messenger.live.com";
        public AuthModule(Skype4Sharp skypeToUse)
        {
            parentSkype = skypeToUse;
        }
        public async Task<bool> Login()
        {
            parentSkype.authState = Enums.LoginState.Processing;
            try
            {
                string skypeToken = "";
                if (parentSkype.authInfo != null)
                    skypeToken = await getSkypeToken();
                else
                    skypeToken = await getSkypeTokenBySession();
                if (skypeToken == "")
                    return false;
                else
                    parentSkype.authTokens.SkypeToken = skypeToken;
                await setRegTokenAndEndpoint();
                await startSubscription();
                await setProfile();
                parentSkype.authState = Enums.LoginState.Success;
                return true;
            }
            catch
            {
                parentSkype.authState = Enums.LoginState.Failed;
                return false;
            }
        }
        public async Task Logout()
        {
            HttpRequestMessage logoutRequest = parentSkype.mainFactory.createWebRequest_GET("https://login.skype.com/logout?client_id=578134&redirect_uri=https%3A%2F%2Fweb.skype.com", new string[][] { });
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                await client.SendAsync(logoutRequest);
            }
            parentSkype.authState = Enums.LoginState.Unknown;
        }
        private async Task<string> getSkypeTokenBySession()
        {
            string skypeToken = "";
            HttpRequestMessage standardTokenRequest = parentSkype.mainFactory.createWebRequest_GET("https://login.skype.com/login?client_id=578134&redirect_uri=https%3A%2F%2Fweb.skype.com", new string[][] { });
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                var result = await client.SendAsync(standardTokenRequest);
                string resp = await result.Content.ReadAsStringAsync();
                skypeToken = new Regex("type=\"hidden\" name=\"skypetoken\" value=\"(.*?)\"").Match(resp).Groups[1].ToString();
            }
            return skypeToken;
        }
        private async Task<string> getSkypeToken()
        {
            switch (parentSkype.tokenType)
            {
                case Enums.SkypeTokenType.Standard:
                    HttpRequestMessage standardTokenRequest = parentSkype.mainFactory.createWebRequest_GET("https://login.skype.com/login?client_id=578134&redirect_uri=https%3A%2F%2Fweb.skype.com", new string[][] { });
                    string rawDownload = "";
                    using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
                    using (var client = new HttpClient(handler))
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                        var result = await client.SendAsync(standardTokenRequest);
                        rawDownload = await result.Content.ReadAsStringAsync();
                    }
                    string skypeToken = new Regex("type=\"hidden\" name=\"skypetoken\" value=\"(.*?)\"").Match(rawDownload).Groups[1].ToString();
                    if (skypeToken != null && skypeToken != "")
                        return skypeToken;
                    string uploadData = string.Format("username={0}&password={1}&timezone_field={2}&js_time={3}&pie={4}&etm={5}&client_id=578134&redirect_uri={6}&persistent=1", parentSkype.authInfo.Username.UrlEncode(), parentSkype.authInfo.Password.UrlEncode(), DateTime.Now.ToString("zzz").Replace(":", "|").UrlEncode(), (Helpers.Misc.getTime() / 1000).ToString(), new Regex("<input type=\"hidden\" name=\"pie\" id=\"pie\" value=\"(.*?)\"/>").Match(rawDownload).Groups[1].ToString().UrlEncode(), new Regex("<input type=\"hidden\" name=\"etm\" id=\"etm\" value=\"(.*?)\"/>").Match(rawDownload).Groups[1].ToString().UrlEncode(), "https://web.skype.com".UrlEncode());
                    HttpRequestMessage actualLogin = parentSkype.mainFactory.createWebRequest_POST("https://login.skype.com/login?client_id=578134&redirect_uri=https%3A%2F%2Fweb.skype.com", new string[][] { }, Encoding.ASCII.GetBytes(uploadData), "");
                    using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
                    using (var client = new HttpClient(handler))
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                        var result = await client.SendAsync(actualLogin);
                        string resp = await result.Content.ReadAsStringAsync();
                        return new Regex("type=\"hidden\" name=\"skypetoken\" value=\"(.*?)\"").Match(resp).Groups[1].ToString();
                    }
                case Enums.SkypeTokenType.MSNP24:
                    HttpRequestMessage MSNP24TokenRequest = parentSkype.mainFactory.createWebRequest_POST("https://api.skype.com/login/skypetoken", new string[][] { }, Encoding.ASCII.GetBytes(string.Format("scopes=client&clientVersion=0/7.17.0.105//&username={0}&passwordHash={1}", parentSkype.authInfo.Username, Convert.ToBase64String(Helpers.Misc.hashMD5_Byte(string.Format("{0}\nskyper\n{1}", parentSkype.authInfo.Username, parentSkype.authInfo.Password))))), "");
                    using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
                    using (var client = new HttpClient(handler))
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                        var result = await client.SendAsync(MSNP24TokenRequest);
                        string resp = await result.Content.ReadAsStringAsync();
                        return new Regex("{\"skypetoken\":\"(.*?)\",\"expiresIn\":86400}").Match(resp).Groups[1].ToString();
                    }
                default:
                    return null;
            }
        }
        private async Task setRegTokenAndEndpoint()
        {
            HttpRequestMessage webRequest = parentSkype.mainFactory.createWebRequest_POST(clientGatewayMessengerDomain + "/v1/users/ME/endpoints", new string[][] { new string[] { "Authentication", "skypetoken=" + parentSkype.authTokens.SkypeToken } }, Encoding.ASCII.GetBytes("{}"), "application/x-www-form-urlencoded");
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                var webResponse = await client.SendAsync(webRequest);
                IEnumerable<string> values;
                if (webResponse.Headers.TryGetValues("Set-RegistrationToken", out values))
                {
                    var e = values.GetEnumerator();
                    e.MoveNext();
                    parentSkype.authTokens.RegistrationToken = e.Current.Split(';')[0];
                }
                if (webResponse.Headers.TryGetValues("Location", out values))
                {
                    var e = values.GetEnumerator();
                    e.MoveNext();
                    parentSkype.authTokens.Endpoint = e.Current;
                }
                if (webResponse.Headers.TryGetValues("Set-RegistrationToken", out values))
                {
                    var e = values.GetEnumerator();
                    e.MoveNext();
                    if (e.Current.Split(';').Length >= 3)
                        parentSkype.authTokens.EndpointID = e.Current.Split(';')[2].Split('=')[1];
                }
            }
        }
        private async Task startSubscription()
        {
            HttpRequestMessage propertiesRequest = parentSkype.mainFactory.createWebRequest_PUT(clientGatewayMessengerDomain + "/v1/users/ME/endpoints/SELF/properties?name=supportsMessageProperties", new string[][] { new string[] { "RegistrationToken", parentSkype.authTokens.RegistrationToken } }, Encoding.ASCII.GetBytes("{\"supportsMessageProperties\":true}"), "application/json");
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                await client.SendAsync(propertiesRequest);
            }
            HttpRequestMessage subscriptionRequest = parentSkype.mainFactory.createWebRequest_POST(clientGatewayMessengerDomain + "/v1/users/ME/endpoints/SELF/subscriptions", new string[][] { new string[] { "RegistrationToken", parentSkype.authTokens.RegistrationToken } }, Encoding.ASCII.GetBytes("{\"channelType\":\"httpLongPoll\",\"template\":\"raw\",\"interestedResources\":[\"/v1/users/ME/conversations/ALL/properties\",\"/v1/users/ME/conversations/ALL/messages\",\"/v1/users/ME/contacts/ALL\",\"/v1/threads/ALL\"]}"), "application/json");
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                await client.SendAsync(subscriptionRequest);
            }
        }
        private async Task setProfile()
        {
            HttpRequestMessage selfProfileRequest = parentSkype.mainFactory.createWebRequest_GET("https://api.skype.com/users/self/profile", new string[][] { new string[] { "X-Skypetoken", parentSkype.authTokens.SkypeToken } });
            string rawJSON = "";
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                var result = await client.SendAsync(selfProfileRequest);
                rawJSON = await result.Content.ReadAsStringAsync();
            }
            dynamic decodedJSON = JsonConvert.DeserializeObject(rawJSON);
            string firstName = decodedJSON.firstname;
            string lastName = decodedJSON.lastname;
            string userName = decodedJSON.username;
            string avatarUrl = decodedJSON.avatarUrl;
            string finalName = "";
            if (firstName == null)
            {
                if (lastName != null)
                {
                    finalName = lastName;
                }
            }
            else
            {
                if (lastName == null)
                {
                    finalName = firstName;
                }
                else
                {
                    finalName = firstName + " " + lastName;
                }
            }
            parentSkype.selfProfile.DisplayName = finalName;
            parentSkype.selfProfile.Username = userName;
            if (avatarUrl != null)
                parentSkype.selfProfile.AvatarUri = new Uri(avatarUrl, UriKind.Absolute);
            else
                parentSkype.selfProfile.AvatarUri = new Uri("ms-appx:///Assets/default-avatar.png");
            parentSkype.selfProfile.Type = Enums.UserType.Normal;
        }
    }
}
