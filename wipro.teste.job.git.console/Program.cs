using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace wipro.teste.job.console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) => {
                    services.AddHostedService<Worker>();
                    services.AddScoped<IOrquestrador, Orquestrador>();
                    services.AddHttpClient("api", (config) => new HttpClient()
                    {
                        BaseAddress = new Uri("https://localhost:7034")
                    });
                })
                .Build().Run();
        }
    }
}