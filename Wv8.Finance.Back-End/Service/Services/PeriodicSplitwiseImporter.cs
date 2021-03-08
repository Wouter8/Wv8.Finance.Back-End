namespace PersonalFinance.Service.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using PersonalFinance.Business.Splitwise;

    /// <summary>
    /// A class for a service which handles a periodic run to import expenses from Splitwise.
    /// </summary>
    public class PeriodicSplitwiseImporter : IHostedService, IDisposable
    {
        /// <summary>
        /// The timer.
        /// </summary>
        private Timer timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodicSplitwiseImporter"/> class.
        /// </summary>
        /// <param name="services">The service provider.</param>
        public PeriodicSplitwiseImporter(IServiceProvider services)
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
            this.timer = new Timer(this.Import, null, TimeSpan.Zero, TimeSpan.FromHours(1));

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
        /// This method retrieves the Splitwise manager and runs the importer.
        /// </summary>
        /// <param name="state">The state. This is not used.</param>
        private void Import(object state)
        {
            using var scope = this.Services.CreateScope();
            using var serviceScope = scope.ServiceProvider
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();

            var manager = serviceScope.ServiceProvider.GetService<ISplitwiseManager>();

            manager.ImportFromSplitwise();
        }
    }
}