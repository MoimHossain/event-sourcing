
using SuperNova.Shared.Configs;
using SuperNova.Shared.Repositories.Materialize;
using SuperNova.Shared.Supports;
using SuperNova.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using SuperNova.Shared.DomainObjects;
using SuperNova.Shared.Messaging.Events.Users;
using SuperNova.Shared.Dtos;
using SuperNova.Storage.EventStore;

namespace SuperNova.Materializer.Host
{

    class Program
    {
        static void Main(string[] args)
        {
            Run().Wait();
        }

        private static async Task Run()
        {
            var tenant = new Tenant
            {
                TenantId = Guid.Parse("{48A7FB91-7B14-4EB7-98FC-B145B6504BB6}"),
                Name = "ABC Company"
            };

            var logFactory = new FakeLogFactory();
            var configStore = new TestConfigStore(logFactory);

            var cw = new EventStreamConsumer(configStore, tenant, Streams.Users, $"{Streams.Users}-lease", logFactory);
            await cw.InitAsync();

            var repoFactory = new RepositoryFactory(configStore, logFactory);

            var docRepo = await repoFactory.CreateDocumentRepositoryAsync<IUserMaterialzeRepository>(tenant).ConfigureAwait(false);

            await cw.RunAndBlock((evts) =>
            {

                foreach (var @evt in evts.OrderBy(e => e.Version))
                {
                    if (evt is UserRegistered)
                    {
                        docRepo.AddUserAsync(new UserDto
                        {
                            UserId = (evt as UserRegistered).AggregateId.ToLowercaseAlphaNum(),
                            Name = (evt as UserRegistered).UserName,
                            Email = (evt as UserRegistered).Email
                        }, evt.Version);
                    }

                    else if (evt is UserNameChanged)
                    {


                        docRepo.UpdateUserAsync(new UserDto
                        {
                            UserId = (evt as UserNameChanged).AggregateId.ToLowercaseAlphaNum(),
                            Name = (evt as UserNameChanged).NewName
                        }, evt.Version);
                    }
                }

            }, CancellationToken.None);

        }
    }

    public class TestConfigStore : ConfigStore
    {
        private static Dictionary<string, string> _keyValues = new Dictionary<string, string>
            {
                { "StorageConfig:AccountName", "" },
                { "StorageConfig:AccountKey", "" },
                { "StorageConfig:DocumentEndpoint", "https://<YOUR DB>.documents.azure.com:443/" },
                { "StorageConfig:DocumentKey", "" },
            };

        public TestConfigStore(ILoggerFactory loggerFactory)
            : base(true, (key) => _keyValues[key], loggerFactory)
        {

        }
    }
    public class FakeLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {

        }
    }
    public class FakeLogFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider)
        {

        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FakeLogger();
        }

        public void Dispose()
        {

        }
    }

}
