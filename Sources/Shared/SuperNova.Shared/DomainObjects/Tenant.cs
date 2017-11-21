using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNova.Shared.DomainObjects
{
    public class Tenant 
    {
        public Guid TenantId { get; set; }
        public string Name { get; set; }
    }
}
