using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PortfolioTool.Models;

namespace PortfolioTool.Services
{
    class StockBroker
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public async Task<IDictionary<string, double>> GetQuotes(params string[] symbols)
        {
            string url = $"https://api.iextrading.com/1.0/stock/market/batch?symbols={string.Join(',', symbols)}&types=quote&range=1d";
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode) 
            {
                throw new HttpRequestException($"Received response code {response.StatusCode} from '{url}'.");
            }

            Dictionary<string, double> quotes = new Dictionary<string, double>();
            JObject content = JObject.Parse(await response.Content.ReadAsStringAsync());

            for (var i = 0; i < symbols.Length; i++)
            {
                try {
                    quotes[symbols[i]] = (double)content[symbols[i].ToUpper()]["quote"]["latestPrice"];
                }
                catch (Exception) 
                {
                    throw new HttpRequestException($"Unable to parse '{symbols[i].ToUpper()}.quote.latestPrice'.");
                }
            }
            
            return quotes;
        }
    }
}