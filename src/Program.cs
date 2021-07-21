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

                var portfolioPath = Path.GetFullPath(args[0]);

                if (File.Exists(portfolioPath))
                {
                    await SynchronizedConsole.WriteLineAsync($"Beginning to read file '{portfolioPath}'.");
                    var portfolioManager = new PortfolioManager();
                    await portfolioManager.LoadPortfolioAsync(portfolioPath);
                    await portfolioManager.Rebalance();
                    return;
                }

                if (Directory.Exists(portfolioPath))
                {
                    await SynchronizedConsole.WriteLineAsync($"Beginning to read directory '{portfolioPath}'.");
                    List<Task> tasks = new List<Task>();

                    foreach (string path in Directory.GetFiles(portfolioPath, "*.json")) 
                    {
                        tasks.Add(Task.Run(async () => {
                            var portfolioManager = new PortfolioManager();
                            await portfolioManager.LoadPortfolioAsync(path);
                            await portfolioManager.Rebalance();
                        }));
                    }

                    await Task.WhenAll(tasks);
                    return;
                }

                throw new ArgumentException($"The path '{portfolioPath}' appears to be invalid.");
            }
            catch (Exception ex) 
            {
                await SynchronizedConsole.WriteLineAsync(ex.Message, ConsoleColor.DarkRed);
                await SynchronizedConsole.WriteLineAsync(ex.StackTrace, ConsoleColor.DarkRed);
            }
        }
    }
}
