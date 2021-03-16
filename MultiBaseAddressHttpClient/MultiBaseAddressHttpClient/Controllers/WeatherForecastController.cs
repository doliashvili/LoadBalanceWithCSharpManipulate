using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MultiBaseAddressHttpClient.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly CallApiService _callApiService;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(CallApiService callApiService, ILogger<WeatherForecastController> logger)
        {
            _callApiService = callApiService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<string> Get()
        {
            var result = await _callApiService.GetInfo();
            return result;
        }

        [HttpPost]
        public async Task<string> Post(string body)
        {
            var result = await _callApiService.Send(body);
            return await result.Content.ReadAsStringAsync();
        }
    }
}
