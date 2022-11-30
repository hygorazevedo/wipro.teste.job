using Microsoft.Extensions.Hosting;

namespace wipro.teste.job.console
{
    internal class Worker : IHostedService
    {
        private readonly IOrquestrador _orquestrador;
        public Worker(IOrquestrador orquestrador)
        {
            _orquestrador = orquestrador;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await _orquestrador.ProcessarTarefa();
                await Task.Delay(120000);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}