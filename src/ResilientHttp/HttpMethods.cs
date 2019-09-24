using System;
using System.Net.Http;

namespace ResilientHttp
{
    public class HttpMethods
    {
        public static HttpMethod DELETE = new HttpMethod("DELETE");
        public static HttpMethod GET = new HttpMethod("GET");
        public static HttpMethod PATCH = new HttpMethod("PATCH");
        public static HttpMethod POST = new HttpMethod("POST");
        public static HttpMethod PUT = new HttpMethod("PUT");
    }
}