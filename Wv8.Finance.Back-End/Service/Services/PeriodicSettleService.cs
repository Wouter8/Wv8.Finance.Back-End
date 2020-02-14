namespace PersonalFinance.Service.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using PersonalFinance.Business.Transaction;

    /// <summary>
    /// A class for a service which handles a periodic run to settle all needing objects.
    /// </summary>
    public class PeriodicSettleService : IHostedService, IDisposable
    {
        private readonly IPeriodicSettler manager;
        private Timer timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodicSettleService"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        public PeriodicSettleService(IPeriodicSettler manager)
        {
            this.manager = manager;
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(6));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            this.manager.Run();
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
    }
}