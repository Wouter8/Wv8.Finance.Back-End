namespace PersonalFinance.Service.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using PersonalFinance.Business.Transaction.Processor;
    using PersonalFinance.Data;
    using PersonalFinance.Data.External.Splitwise;

    /// <summary>
    /// A class for a service which handles a periodic run to process all needed objects.
    /// </summary>
    public class PeriodicProcessorService : IHostedService, IDisposable
    {
        /// <summary>
        /// The timer.
        /// </summary>
        private Timer timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodicProcessorService"/> class.
        /// </summary>
        /// <param name="services">The service provider.</param>
        public PeriodicProcessorService(IServiceProvider services)
        {
            this.Services = services;
        }

        /// <summary>
        /// The service provider.
        /// </summary>
        public IServiceProvider Services { get; }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.timer = new Timer(this.ProcessAll, null, TimeSpan.Zero, TimeSpan.FromHours(6));

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.timer?.Dispose();
        }

        /// <summary>
        /// This method retrieves the process service and runs it.
        /// </summary>
        /// <param name="state">The state. This is not used.</param>
        private void ProcessAll(object state)
        {
            using var scope = this.Services.CreateScope();
            using var serviceScope = scope.ServiceProvider
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();

            using var context = serviceScope.ServiceProvider.GetService<Context>();
            var splitwiseContext = serviceScope.ServiceProvider.GetService<ISplitwiseContext>();

            var processor = new TransactionProcessor(context, splitwiseContext);

            processor.ProcessAll();
        }
    }
}