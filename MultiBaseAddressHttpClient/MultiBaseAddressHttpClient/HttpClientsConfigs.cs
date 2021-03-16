using System.Collections.Generic;

namespace MultiBaseAddressHttpClient
{
    public class HttpClientsConfigs
    {
        public List<HttpClientInfo> HttpClientInfo { get; set; }
    }

    public class HttpClientInfo
    {
        public string Name { get; set; }
        public string BaseAddress { get; set; }
    }
}