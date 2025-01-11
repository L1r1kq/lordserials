using MyLordSerialsLibrary;
using MyLordSerialsLibrary.Configuration;

namespace MyHttpServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var prefixes = new[] { $"http://{AppConfig.Domain}:{AppConfig.Port}/" };
            var server = new HttpServer(prefixes);
            
            await server.StartAsync();
        }
    }
}