using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiBaseAddressHttpClient
{
    public class HttpRandomClient
    {
        private readonly HttpClient _client;
        private readonly IHttpClientFactory _clientFactory;
        private static readonly Dictionary<int, DateTime> BadClients = new Dictionary<int, DateTime>();
        private readonly HttpClientsConfigs _httpClientsConfig;

        public HttpRandomClient(IHttpClientFactory clientFactory, HttpClientsConfigs httpClientsConfig)
        {
            _clientFactory = clientFactory;
            _httpClientsConfig = httpClientsConfig;
            _client = GetRandomClient();
        }

        public async Task<HttpResponseMessage> GetAsync(string urlWithOutBaseAddress, CancellationToken cancellationToken)
        {
            var requestUrl = $"{_client.BaseAddress}{urlWithOutBaseAddress}";
            var response = await _client.GetAsync(requestUrl, cancellationToken);

            EnsureSuccessStatus(response);

            return response;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpMethod httpMethod, string body, string urlWithOutBaseAddress, CancellationToken cancellationToken)
        {
            var requestUrl = $"{_client.BaseAddress}{urlWithOutBaseAddress}?body={body}";

            var httpRequestMessage = new HttpRequestMessage(httpMethod, requestUrl);

            var response = await _client.SendAsync(httpRequestMessage, cancellationToken);

            EnsureSuccessStatus(response);

            return response;
        }

        #region Private methods

        private void EnsureSuccessStatus(HttpResponseMessage response)
        {
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound) //Todo ავირჩიოთ რა სტატუს კოდებზე ვიჭერთ
                {
                    var index = _httpClientsConfig.HttpClientInfo
                        .FindIndex(x => _client.BaseAddress == new Uri(x.BaseAddress));
                    BadClients.Add(index, DateTime.Now.AddMinutes(1)); // Todo ავირჩიოთ რამდენი ხანში გასიფთავდეს
                    throw;
                }
            }
            finally
            {
                ClearBadClientsIfTimeExpire();
            }
        }

        private HttpClient GetRandomClient()
        {
            int randomIndex;
            do
            {
                randomIndex = new Random().Next(0, _httpClientsConfig.HttpClientInfo.Count);
            } while (CheckBadClients(randomIndex));

            var clientName = _httpClientsConfig.HttpClientInfo[randomIndex].Name;

            return _clientFactory.CreateClient(clientName);
        }

        private bool CheckBadClients(int randomIndex)
        {
            var isBadClient = BadClients.Keys.Contains(randomIndex);

            return isBadClient;
        }

        private static void ClearBadClientsIfTimeExpire()
        {
            var expireBadClientsKey = BadClients
                .Where(x => x.Value <= DateTime.Now)
                .Select(x => x.Key);

            foreach (var key in expireBadClientsKey)
            {
                BadClients.Remove(key);
            }
        }

        #endregion
    }
}
