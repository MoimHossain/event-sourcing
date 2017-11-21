
using SuperNova.Storage.Supports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using SuperNova.Shared.DomainObjects;

namespace SuperNova.StorageTest.Supports
{
    [TestClass]
    public class PartitionKeyExtensionsTest

    {
        [TestMethod]
        public void GetPartitionKeyTest()
        {
            // Arrange
            var dt = DateTime.Now;

            // Act
            var key = PartitionKeyExtensions.GetPartitionKey(dt);
            // Assert
            Assert.AreEqual(key, string.Format("{0}-{1}-{2}", dt.Year, dt.Month, dt.Day));
        }

        [TestMethod]
        public void ToSafeStorageKeyTest()
        {
            // Arrange
            var gid = Guid.NewGuid();
            // Act
            var val = PartitionKeyExtensions.ToSafeStorageKey(gid);
            // Assert
            Assert.AreEqual(val, gid.ToString("N").Replace("-", string.Empty));
        }

        [TestMethod]
        public void GetKeyPairTest()
        {
            // Arrange
            var tenant = new Tenant { TenantId = Guid.NewGuid(), Name = "ABC.com" };
            // Act
            var keys = PartitionKeyExtensions.GetKeyPair(tenant);

            var k = PartitionKeyExtensions.ToSafeStorageKey(tenant.TenantId);
            // Assert
            Assert.AreEqual(keys.PartitionKey, k);
        }
    }
}
