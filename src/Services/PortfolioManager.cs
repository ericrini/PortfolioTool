using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PortfolioTool.Models;
using static PortfolioTool.SynchronizedConsole;

namespace PortfolioTool.Services
{
    class PortfolioManager
    {
        private static StockBroker _stockBroker = new StockBroker();
        private Portfolio _portfolio;
        private IDictionary<string, double> _quotes;

        public async Task LoadPortfolioAsync(string path)
        {
            await SynchronizedConsole.WriteLineAsync($"Beginning to parse file '{path}'.");
            string json = await File.ReadAllTextAsync(path);
            _portfolio = JsonConvert.DeserializeObject<Portfolio>(json);
            _quotes = await _stockBroker.GetQuotes(GetUniqueSymbols().ToArray());
        }

        public async Task Rebalance()
        {
            if (_portfolio == null)
            {
                throw new NullReferenceException("Must load a portfolio before attempting to rebalance.");
            }

            ValidateTotalAllocations();

            using (ConsoleContext context = await ObtainContextAsync())
            {
                // The total value of the portfolio is the sum of all holdings at the current market price plus liquid cash.
                double totalValue = _portfolio.Cash;
                foreach (KeyValuePair<string, double> holding in _portfolio.Holdings)
                {
                    totalValue += holding.Value * _quotes[holding.Key];
                }

                context.EndLine();

                context.SetForeground(ConsoleColor.Yellow).Write(_portfolio.Name)
                    .SetForeground(ConsoleColor.DarkGray).Write(" (")
                    .SetForeground(ConsoleColor.Yellow).Write(totalValue.ToString("c"))
                    .SetForeground(ConsoleColor.DarkGray).Write(")")
                    .EndLine();

                // Calculate the order to place for each desired allocation.
                foreach (string symbol in GetUniqueSymbols())
                {
                    _portfolio.Allocations.TryGetValue(symbol, out double allocationPercent);
                    _portfolio.Holdings.TryGetValue(symbol, out double currentShares);
                    double marketValue = _quotes[symbol];
                    double holdingPercent = (currentShares * marketValue) / totalValue;
                    double deltaPercent = allocationPercent - holdingPercent;
                    double desiredValue = totalValue * allocationPercent;
                    double desiredShares = Math.Floor(desiredValue / marketValue);
                    double deltaShares = desiredShares - currentShares;
                    string tradeText = "BUY";
                    ConsoleColor tradeColor = ConsoleColor.DarkGreen;
                    double totalTrade = marketValue * deltaShares;

                    if (deltaShares < 0)
                    {
                        tradeText = "SELL";
                        tradeColor = ConsoleColor.DarkRed;
                        deltaShares = Math.Abs(deltaShares);
                    }

                    context.SetForeground(ConsoleColor.DarkGray).Write("  ")
                        .SetForeground(ConsoleColor.Yellow).Write(symbol.ToUpper())
                        .EndLine();

                    context.SetForeground(ConsoleColor.DarkGray).Write("    Current holding is ")
                        .SetForeground(ConsoleColor.DarkBlue).Write(holdingPercent.ToString("p"))
                        .SetForeground(ConsoleColor.DarkGray).Write(", the target allocation is ")
                        .SetForeground(ConsoleColor.DarkBlue).Write(allocationPercent.ToString("p"))
                        .SetForeground(ConsoleColor.DarkGray).Write(".")
                        .SetForeground(ConsoleColor.DarkGray)
                        .EndLine();

                    context.SetForeground(ConsoleColor.DarkGray).Write("    To align the holding, ")
                        .SetForeground(tradeColor).Write(tradeText)
                        .SetForeground(ConsoleColor.DarkGray).Write(" ")
                        .SetForeground(ConsoleColor.DarkBlue).Write(deltaShares)
                        .SetForeground(ConsoleColor.DarkGray).Write(" shares ")
                        .SetForeground(ConsoleColor.DarkMagenta).Write("LIMIT")
                        .SetForeground(ConsoleColor.DarkGray).Write(" at ")
                        .SetForeground(ConsoleColor.DarkBlue).Write(marketValue.ToString("c"))
                        .SetForeground(ConsoleColor.DarkGray).Write(" each for a total of ")
                        .SetForeground(ConsoleColor.DarkBlue).Write(totalTrade.ToString("c"))
                        .SetForeground(ConsoleColor.DarkGray).Write(".")
                        .EndLine();

                    context.SetForeground(ConsoleColor.DarkGray).Write("    To limit loss, ")
                        .SetForeground(ConsoleColor.DarkMagenta).Write("STOP")
                        .SetForeground(ConsoleColor.DarkGray).Write(" at ")
                        .SetForeground(ConsoleColor.DarkBlue).Write((marketValue * 0.90).ToString("c"))
                        .SetForeground(ConsoleColor.DarkGray).Write(".")
                        .EndLine();
                }
            }
        }

        private void ValidateTotalAllocations()
        {
            double total = 0;

            foreach (KeyValuePair<string, double> current in _portfolio.Allocations)
            {
                total += current.Value;
            }

            if (total != 1)
            {
                throw new Exception($"Portfolio allocations in portfolio '{_portfolio.Name}' don't add up to 100%.");
            }
        }

        private string[] GetUniqueSymbols()
        {
            HashSet<string> symbols = new HashSet<string>();
            foreach (string symbol in _portfolio.Allocations.Keys.Union(_portfolio.Holdings.Keys))
            {
                symbols.Add(symbol);
            }
            string[] symbolsArray = symbols.ToArray();
            Array.Sort(symbolsArray);
            return symbolsArray;
        }
    }
}
