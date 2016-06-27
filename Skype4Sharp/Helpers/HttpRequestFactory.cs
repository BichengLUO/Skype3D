using System.Net.Http;
using System.Net;

namespace Skype4Sharp.Helpers
{
    public class HttpRequestFactory
    {
        public HttpRequestMessage createWebRequest_GET(string targetURL, string[][] requestHeaders)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, targetURL);
            foreach (string[] headerPair in requestHeaders)
            {
                request.Headers.Add(headerPair[0], headerPair[1]);
            }
            return request;
        }
        public HttpRequestMessage createWebRequest_PUT(string targetURL, string[][] requestHeaders, byte[] postData, string contentType)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, targetURL);
            request.Content = new ByteArrayContent(postData);
            request.Content.Headers.ContentLength = postData.Length;
            foreach (string[] headerPair in requestHeaders)
            {
                request.Headers.Add(headerPair[0], headerPair[1]);
            }
            return request;
        }
        public HttpRequestMessage createWebRequest_POST(string targetURL, string[][] requestHeaders, byte[] postData, string contentType)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, targetURL);
            request.Content = new ByteArrayContent(postData);
            request.Content.Headers.ContentLength = postData.Length;
            foreach (string[] headerPair in requestHeaders)
            {
                request.Headers.Add(headerPair[0], headerPair[1]);
            }
            return request;
        }
        public HttpRequestMessage createWebRequest_DELETE(string targetURL, string[][] requestHeaders)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, targetURL);
            foreach (string[] headerPair in requestHeaders)
            {
                request.Headers.Add(headerPair[0], headerPair[1]);
            }
            return request;
        }
    }
}
