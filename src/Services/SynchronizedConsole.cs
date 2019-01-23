using System;
using System.Threading;
using System.Threading.Tasks;

namespace PortfolioTool
{
    static class SynchronizedConsole
    {
        private static int _locked = 0;

        public async static Task WriteLineAsync(string output = "", ConsoleColor foreground = ConsoleColor.White, ConsoleColor background = ConsoleColor.Black)
        {
            using (ConsoleContext context = await ObtainContextAsync()) 
            {
                context.SetForeground(foreground).SetBackground(background).Write(output).EndLine();
            }
        }

        public static async Task<ConsoleContext> ObtainContextAsync() 
        {
            while (Interlocked.CompareExchange(ref _locked, 1, 0) == 1) 
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1));
            }

            return new ConsoleContext();
        }

        public class ConsoleContext : IDisposable
        {
            private readonly ConsoleColor _foreground;
            private readonly ConsoleColor _background;

            public ConsoleContext() 
            {
                _foreground = Console.ForegroundColor;
                _background = Console.BackgroundColor;
            }

            public ConsoleContext SetForeground(ConsoleColor color)
            {
                Console.ForegroundColor = color;
                return this;
            }

            public ConsoleContext SetBackground(ConsoleColor color)
            {
                Console.BackgroundColor = color;
                return this;
            }
            
            public ConsoleContext Write(object output)
            {
                Console.Write(output);
                return this;
            }

            public ConsoleContext PadRight(object output, int padding)
            {
                Write(output.ToString().PadRight(padding));
                return this;
            }

            public ConsoleContext PadLeft(object output, int padding)
            {
                Write(output.ToString().PadRight(padding));
                return this;
            }

            public void EndLine()
            {
                Write(Environment.NewLine);
            }

            public void Dispose()
            {
                Console.ForegroundColor = _foreground;
                Console.BackgroundColor = _background;
                _locked = 0;
            }
        }
    }
}
