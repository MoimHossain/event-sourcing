
using SuperNova.Shared.Tests;
using SuperNova.Storage;
using SuperNova.StorageTest.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using SuperNova.Shared.DomainObjects;

namespace SuperNova.StorageTest
{
    [TestClass]
    public class RepositoryFactoryTest
    {
        [TestMethod]
        public async Task TestRepositoryFactory()
        {
            var repo = await new RepositoryFactory(new TestConfigStore(new FakeLogFactory()), new FakeLogFactory()).
                CreateTableRepositoryAsync<FakeTenantRepository>(new Tenant());

            Assert.IsNotNull(repo);
        }
    }
}
