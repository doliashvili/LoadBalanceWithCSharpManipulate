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
    public class HttpRandomHandler : DelegatingHandler
    {
        private HttpClient _client;
        private static readonly Dictionary<int, DateTime> BadClients = new Dictionary<int, DateTime>();
        private readonly HttpClientsConfigs _httpClientsConfig;

        public HttpRandomHandler(HttpClientsConfigs httpClientsConfig)
        {
            _httpClientsConfig = httpClientsConfig;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _client = GetRandomClient();
            var absolutePath = request.RequestUri?.AbsolutePath.Remove(0, 1);
            var requestUri = new Uri($"{_client.BaseAddress.AbsoluteUri}{absolutePath}");

            request.RequestUri = requestUri;

            var response = await base.SendAsync(request, cancellationToken);
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

            var client = _httpClientsConfig.HttpClientInfo[randomIndex];

            return new HttpClient() { BaseAddress = new Uri(client.BaseAddress) };
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
