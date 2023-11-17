using FluentMigrator.Runner;
using HomeworkApp.Dal.Extensions;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.Dal.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HomeworkApp.IntegrationTests.Fixtures
{
    public class TestFixture
    {
        public IUserRepository UserRepository { get; }

        public ITaskRepository TaskRepository { get; }

        public ITaskLogRepository TaskLogRepository { get; }

        public ITakenTaskRepository TakenTaskRepository { get; }

        public IUserScheduleRepository UserScheduleRepository { get; }

        public ITaskCommentRepository TaskCommentRepository { get; }
        
        public IRateLimiterRepository RateLimiterRepository { get; }
        
        public int RateLimiterMaxRequestsPerTtl { get; } 

        public TestFixture()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddDalInfrastructure(config)
                        .AddDalRepositories();
                })
                .Build();
            
            ClearDatabase(host);
            host.MigrateUp();

            var serviceProvider = host.Services;
            UserRepository = serviceProvider.GetRequiredService<IUserRepository>();
            TaskRepository = serviceProvider.GetRequiredService<ITaskRepository>();
            TaskLogRepository = serviceProvider.GetRequiredService<ITaskLogRepository>();
            TakenTaskRepository = serviceProvider.GetRequiredService<ITakenTaskRepository>();
            UserScheduleRepository = serviceProvider.GetRequiredService<IUserScheduleRepository>();
            TaskCommentRepository = serviceProvider.GetRequiredService<ITaskCommentRepository>();
            RateLimiterRepository = serviceProvider.GetRequiredService<IRateLimiterRepository>();
            
            FluentAssertionOptions.UseDefaultPrecision();

            RateLimiterMaxRequestsPerTtl = int.Parse(config.GetSection(nameof(RateLimiterRedisOptions))
                .GetSection(nameof(RateLimiterRedisOptions.MaxRequestsPerTtl)).Value!);
        }

        private static void ClearDatabase(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateDown(0);
        }
    }
}