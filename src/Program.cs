using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PortfolioTool.Models;
using PortfolioTool.Services;

namespace PortfolioTool
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                if (args.Length < 1) {
                    throw new ArgumentException("Must specify path to account information.");
                }

                var portfolioDirectory = Path.GetFullPath(args[0]);

                if (!Directory.Exists(portfolioDirectory))
                {
                    throw new ArgumentException($"The directory '{portfolioDirectory}' wasn't found.");
                }

                await SynchronizedConsole.WriteLineAsync($"Beginning to read directory '{portfolioDirectory}'.");

                List<Task> tasks = new List<Task>();
                foreach (string path in Directory.GetFiles(portfolioDirectory, "*.json")) 
                {
                    tasks.Add(Task.Run(async () => {
                        var portfolioManager = new PortfolioManager();
                        await portfolioManager.LoadPortfolioAsync(path);
                        await portfolioManager.Rebalance();   
                    }));
                }
                await Task.WhenAll(tasks);
            }
            catch (Exception ex) 
            {
                await SynchronizedConsole.WriteLineAsync(ex.Message, ConsoleColor.DarkRed);
            }
        }
    }
}
