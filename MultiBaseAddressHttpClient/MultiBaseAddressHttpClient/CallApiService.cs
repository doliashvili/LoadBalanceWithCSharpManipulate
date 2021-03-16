using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace MultiBaseAddressHttpClient
{
    public class CallApiService
    {

        private readonly HttpRandomClient _httpRandomClient;

        public CallApiService(IHttpClientFactory httpClient, IOptions<HttpClientsConfigs> faOptions)
        {
            _httpRandomClient = new HttpRandomClient(httpClient, faOptions.Value);
        }

        public async Task<string> GetInfo()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var response = await _httpRandomClient.GetAsync("WeatherForecast", default);
            stopwatch.Stop();

            Console.WriteLine(stopwatch.Elapsed);

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<HttpResponseMessage> Send(string body)
        {
            var response = await _httpRandomClient.SendAsync(HttpMethod.Post, body, "WeatherForecast/Post", default);
            return response;
        }

    }
}
