namespace PersonalFinance.Service
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.SqlServer.NodaTime.Extensions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using PersonalFinance.Business.Account;
    using PersonalFinance.Business.Budget;
    using PersonalFinance.Business.Category;
    using PersonalFinance.Business.Report;
    using PersonalFinance.Business.Transaction;
    using PersonalFinance.Business.Transaction.Processor;
    using PersonalFinance.Business.Transaction.RecurringTransaction;
    using PersonalFinance.Data;
    using PersonalFinance.Service.Middleware;
    using PersonalFinance.Service.Services;
    using Wv8.Core.ModelBinding;

    /// <summary>
    /// The class that configures the services to be used in this back-end.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The settings for the application.</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// The settings of the application.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. This method adds services to the container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // JSON
            services.AddControllers(options =>
                {
                    options.ModelBinderProviders.Insert(0, new MaybeModelBinderProvider());
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(new MaybeJsonConverter());
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            // Cors
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyOrigin();
                    builder.AllowAnyMethod();
                    builder.AllowAnyHeader();
                });
            });

            // DbContext
            services.AddDbContext<Context>(options =>
                    options.UseSqlServer(
                        this.Configuration.GetConnectionString("Default"),
                        sqlOptions => sqlOptions.UseNodaTime()));

            // Managers
            services.AddTransient<IAccountManager, AccountManager>();
            services.AddTransient<ICategoryManager, CategoryManager>();
            services.AddTransient<IBudgetManager, BudgetManager>();
            services.AddTransient<ITransactionManager, TransactionManager>();
            services.AddTransient<IRecurringTransactionManager, RecurringTransactionManager>();
            services.AddTransient<ITransactionProcessor, TransactionProcessor>();
            services.AddTransient<IReportManager, ReportManager>();

            // Services
            services.AddHostedService<PeriodicService>();
        }

        /// <summary>
        /// This method gets called by the runtime. This method configures the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The web hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("CorsPolicy");

            // Custom middleware
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            if (env.IsProduction())
                this.UpdateDatabase(app);

            // Process everything on startup
            this.ProcessAll(app);
        }

        /// <summary>
        /// Updates the database with all pending migrations.
        /// </summary>
        /// <param name="app">The application builder.</param>
        private void UpdateDatabase(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<Context>();

            context.Database.Migrate();
        }

        /// <summary>
        /// Processes all unprocessed transactions in the past.
        /// </summary>
        /// <param name="app">The application builder.</param>
        private void ProcessAll(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            var service = serviceScope.ServiceProvider.GetService<ITransactionProcessor>();

            service.Run();
        }
    }
}
