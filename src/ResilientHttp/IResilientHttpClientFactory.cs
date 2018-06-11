using System;

namespace ResilientHttp
{
    public interface IResilientHttpClientFactory
    {
        IHttpClient CreateResilientHttpClient();
    }
}