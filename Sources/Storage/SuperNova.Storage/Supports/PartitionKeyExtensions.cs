
using SuperNova.Shared.DomainObjects;
using SuperNova.Shared.Supports;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNova.Storage.Supports
{
    public static class PartitionKeyExtensions
    {
        public static string GetPartitionKey(DateTime dt)
        {
            return string.Format("{0}-{1}-{2}", dt.Year, dt.Month, dt.Day);
        }

        public static string ToSafeStorageKey(this Guid id)
        {
            return SafetyExtensions.ToLowercaseAlphaNum(id);
        }

        public static KeysPair GetKeyPair(this Tenant tenant)
        {
            Ensure.ArgumentNotNull(tenant, nameof(tenant));

            return new KeysPair(
                tenant.TenantId.ToSafeStorageKey(),
                tenant.TenantId.ToSafeStorageKey());
        }
    }
}
