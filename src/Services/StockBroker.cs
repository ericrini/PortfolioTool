using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PortfolioTool.Models;

namespace PortfolioTool.Services
{
    class StockBroker
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public async Task<IDictionary<string, double>> GetQuotes(params string[] symbols)
        {
            string url = $"https://api.iextrading.com/1.0/tops/last?symbols={string.Join(',', symbols)}";
            await SynchronizedConsole.WriteLineAsync(url);
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode) 
            {
                throw new HttpRequestException($"Received response code {response.StatusCode} from '{url}'.");
            }

            Dictionary<string, double> transformed = new Dictionary<string, double>();
            List<Quote> quotes = JsonConvert.DeserializeObject<List<Quote>>(await response.Content.ReadAsStringAsync());

            for (var i = 0; i < quotes.Count; i++)
            {
                transformed.Add(quotes[i].Symbol, quotes[i].Price);
            }
            
            return transformed;
        }
    }
}