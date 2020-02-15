﻿namespace PersonalFinance.Service.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using PersonalFinance.Business.Transaction;
    using PersonalFinance.Data;

    /// <summary>
    /// A class for a service which handles a periodic run to settle all needing objects.
    /// </summary>
    public class PeriodicSettleService : IHostedService, IDisposable
    {
        /// <summary>
        /// The timer.
        /// </summary>
        private Timer timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodicSettleService"/> class.
        /// </summary>
        /// <param name="services">The service provider.</param>
        public PeriodicSettleService(IServiceProvider services)
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
            this.timer = new Timer(this.DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(6));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            using var scope = this.Services.CreateScope();
            using var serviceScope = scope.ServiceProvider
                    .GetRequiredService<IServiceScopeFactory>()
                    .CreateScope();

            var service = serviceScope.ServiceProvider.GetService<IPeriodicSettler>();
            service.Run();
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