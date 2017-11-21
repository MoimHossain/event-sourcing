using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperNova.Api.Supports
{
    public class Constants
    {
        public static Info SwaggerInfo = new Info
        {
            Title = "Event Sourcing Demo API",
            Description = "API Specifications",
            Version = "v1",
            License = new License { Name = "SuperNova Software License", Url = "https://SuperNova.nl" },
            Contact = new Contact { Name = "SuperNova Software", Email = "info@SuperNova.nl", Url = "https://SuperNova.nl" }
        };

        public const string BaseUrlPath = "/";
    }
}
