using Newtonsoft.Json;
using System;
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
        public bool Login()
        {
            parentSkype.authState = Enums.LoginState.Processing;
            try
            {
                string skypeToken = getSkypeToken();
                if (skypeToken == "")
                    return false;
                else
                    parentSkype.authTokens.SkypeToken = skypeToken;
                setRegTokenAndEndpoint();
                startSubscription();
                setProfile();
                parentSkype.authState = Enums.LoginState.Success;
                return true;
            }
            catch
            {
                parentSkype.authState = Enums.LoginState.Failed;
                return false;
            }
        }
        private string getSkypeToken()
        {
            switch (parentSkype.tokenType)
            {
                case Enums.SkypeTokenType.Standard:
                    HttpRequestMessage standardTokenRequest = parentSkype.mainFactory.createWebRequest_GET("https://login.skype.com/login?client_id=578134&redirect_uri=https%3A%2F%2Fweb.skype.com", new string[][] { });
                    string uploadData = "";
                    using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
                    using (var client = new HttpClient(handler))
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                        var result = client.SendAsync(standardTokenRequest).Result;
                        string rawDownload = result.Content.ReadAsStringAsync().Result;
                        uploadData = string.Format("username={0}&password={1}&timezone_field={2}&js_time={3}&pie={4}&etm={5}&client_id=578134&redirect_uri={6}", parentSkype.authInfo.Username.UrlEncode(), parentSkype.authInfo.Password.UrlEncode(), DateTime.Now.ToString("zzz").Replace(":", "|").UrlEncode(), (Helpers.Misc.getTime() / 1000).ToString(), new Regex("<input type=\"hidden\" name=\"pie\" id=\"pie\" value=\"(.*?)\"/>").Match(rawDownload).Groups[1].ToString().UrlEncode(), new Regex("<input type=\"hidden\" name=\"etm\" id=\"etm\" value=\"(.*?)\"/>").Match(rawDownload).Groups[1].ToString().UrlEncode(), "https://web.skype.com".UrlEncode());
                    }
                    HttpRequestMessage actualLogin = parentSkype.mainFactory.createWebRequest_POST("https://login.skype.com/login?client_id=578134&redirect_uri=https%3A%2F%2Fweb.skype.com", new string[][] { }, Encoding.ASCII.GetBytes(uploadData), "");
                    using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
                    using (var client = new HttpClient(handler))
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                        var result = client.SendAsync(actualLogin).Result;
                        string resp = result.Content.ReadAsStringAsync().Result;
                        return new Regex("type=\"hidden\" name=\"skypetoken\" value=\"(.*?)\"").Match(resp).Groups[1].ToString();
                    }
                case Enums.SkypeTokenType.MSNP24:
                    HttpRequestMessage MSNP24TokenRequest = parentSkype.mainFactory.createWebRequest_POST("https://api.skype.com/login/skypetoken", new string[][] { }, Encoding.ASCII.GetBytes(string.Format("scopes=client&clientVersion=0/7.17.0.105//&username={0}&passwordHash={1}", parentSkype.authInfo.Username, Convert.ToBase64String(Helpers.Misc.hashMD5_Byte(string.Format("{0}\nskyper\n{1}", parentSkype.authInfo.Username, parentSkype.authInfo.Password))))), "");
                    using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
                    using (var client = new HttpClient(handler))
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                        var result = client.SendAsync(MSNP24TokenRequest).Result;
                        string resp = result.Content.ReadAsStringAsync().Result;
                        return new Regex("{\"skypetoken\":\"(.*?)\",\"expiresIn\":86400}").Match(resp).Groups[1].ToString();
                    }
                default:
                    return null;
            }
        }
        private void setRegTokenAndEndpoint()
        {
            HttpRequestMessage webRequest = parentSkype.mainFactory.createWebRequest_POST(clientGatewayMessengerDomain + "/v1/users/ME/endpoints", new string[][] { new string[] { "Authentication", "skypetoken=" + parentSkype.authTokens.SkypeToken } }, Encoding.ASCII.GetBytes("{}"), "application/x-www-form-urlencoded");
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                var webResponse = client.SendAsync(webRequest).Result;
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
                    parentSkype.authTokens.EndpointID = e.Current.Split(';')[2].Split('=')[1];
                }
            }
        }
        private void startSubscription()
        {
            HttpRequestMessage propertiesRequest = parentSkype.mainFactory.createWebRequest_PUT(clientGatewayMessengerDomain + "/v1/users/ME/endpoints/SELF/properties?name=supportsMessageProperties", new string[][] { new string[] { "RegistrationToken", parentSkype.authTokens.RegistrationToken } }, Encoding.ASCII.GetBytes("{\"supportsMessageProperties\":true}"), "application/json");
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                client.SendAsync(propertiesRequest).Wait();
            }
            HttpRequestMessage subscriptionRequest = parentSkype.mainFactory.createWebRequest_POST(clientGatewayMessengerDomain + "/v1/users/ME/endpoints/SELF/subscriptions", new string[][] { new string[] { "RegistrationToken", parentSkype.authTokens.RegistrationToken } }, Encoding.ASCII.GetBytes("{\"channelType\":\"httpLongPoll\",\"template\":\"raw\",\"interestedResources\":[\"/v1/users/ME/conversations/ALL/properties\",\"/v1/users/ME/conversations/ALL/messages\",\"/v1/users/ME/contacts/ALL\",\"/v1/threads/ALL\"]}"), "application/json");
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                client.SendAsync(subscriptionRequest).Wait();
            }
        }
        private void setProfile()
        {
            HttpRequestMessage selfProfileRequest = parentSkype.mainFactory.createWebRequest_GET("https://api.skype.com/users/self/profile", new string[][] { new string[] { "X-Skypetoken", parentSkype.authTokens.SkypeToken } });
            string rawJSON = "";
            using (var handler = new HttpClientHandler() { CookieContainer = parentSkype.mainCookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", parentSkype.userAgent);
                var result = client.SendAsync(selfProfileRequest).Result;
                rawJSON = result.Content.ReadAsStringAsync().Result;
            }
            dynamic decodedJSON = JsonConvert.DeserializeObject(rawJSON);
            string firstName = decodedJSON.firstname;
            string lastName = decodedJSON.lastname;
            string userName = decodedJSON.username;
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
            parentSkype.selfProfile.Type = Enums.UserType.Normal;
        }
    }
}
