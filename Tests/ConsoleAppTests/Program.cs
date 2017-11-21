using SuperNova.ReadModel;
using SuperNova.Shared.DomainObjects;
using SuperNova.Shared.Messaging;

using SuperNova.Shared.Tests;
using SuperNova.Storage.EventStore;
using SuperNova.Storage.Supports;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleAppTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Try().Wait();

            Console.WriteLine("Application completed...press any key");
            Console.ReadLine();
        }

        private static async Task Try()
        {
            var tenant = new Tenant
            {
                TenantId = Guid.Parse("{48A7FB91-7B14-4EB7-98FC-B145B6504BB6}"),
                Name = "ABC Company"
            };

            var aggregateID = Guid.NewGuid();
            var streamName = "productstream";
            var logFactory = new FakeLogFactory();
            var configStore = new TestConfigStore(logFactory);

            await TestReadSide(tenant, streamName, logFactory, configStore);


            //await TestAppendEvents(tenant, aggregateID, streamName, logFactory, configStore);
        }

        private static async Task TestReadSide(Tenant tenant, string streamName, FakeLogFactory logFactory, TestConfigStore configStore)
        {            
            var cw = new EventStreamConsumer(configStore, tenant, streamName, "PRODUCT-LEASE", logFactory);
            await cw.InitAsync();

            await cw.RunAndBlock((evts) =>
            {
                Console.Write("Event received ..", evts);
            }, CancellationToken.None);
        }

        private static async Task TestAppendEvents(Tenant tenant, Guid aggregateID, string streamName, FakeLogFactory logFactory, TestConfigStore configStore)
        {
            var eventStore = new EventStore(configStore, logFactory);

            await UseStreamApi(tenant, aggregateID, streamName, eventStore);

            await UseTxApi(tenant, aggregateID, streamName, eventStore);
        }

        private static async Task UseStreamApi(Tenant tenant, Guid aggregateID, string streamName, EventStore eventStore)
        {
            var stream = await eventStore.GetStreamAsync(tenant, streamName);
            var version = await stream.GetCurrentVersionAsync(aggregateID);
            await stream.EmitEventsAsync(aggregateID, version, new List<EventBase>
            {
                new SampleEvent{ Desc = "Event 1" },
                new SampleEvent{ Desc = "Event 2" }
            }, CancellationToken.None);
        }

        private static async Task UseTxApi(Tenant tenant, Guid aggregateID, string streamName, EventStore eventStore)
        {
            await eventStore.ExecuteInTransactionAsync(tenant, streamName, aggregateID, async (tx) =>
            {
                tx.AddEvent(new SampleEvent { Desc = "Event 3" });
                // Do somthing else 
                tx.AddEvent(new SampleEvent { Desc = "Event 4" });

                tx.AddEvents(new List<EventBase>            {
                    new SampleEvent{ Desc = "Event 5" },
                    new SampleEvent{ Desc = "Event 6" }
                });

                await Task.CompletedTask;
            });
        }
    }

    public class SampleEvent : EventBase
    {
        public string Desc { get; set; }
    }
}
