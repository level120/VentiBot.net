using System;
using System.Threading;
using System.Threading.Tasks;

namespace VentiBot.net
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var cts = new CancellationTokenSource();

                VentiBot.RunAsync(cts);

                Console.ReadLine();
                cts.Cancel();
            }
            catch (Exception e)
            {
                await Console.Error.WriteLineAsync(e.ToString());
            }
        }
    }
}