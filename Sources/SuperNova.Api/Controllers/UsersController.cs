
using SuperNova.Shared.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SuperNova.Shared.EventStore;
using System;
using System.Threading.Tasks;
using SuperNova.Shared.Supports;
using SuperNova.Shared.DomainObjects;

namespace SuperNova.Api.Controllers
{
    [Produces("application/json")]
    [Route("users")]
    public class UsersController : EventStreamControllerBase
    {
        private ILoggerFactory logFactory;
        private ITenantRepository tenantRepository;
        
        public UsersController(
            IRepositoryFactory repositoryFactory, 
            IEventStore eventStore, 
            ILoggerFactory logFactory) : base(repositoryFactory, eventStore, logFactory)
        {

        }        

        //PUT: User
        [HttpPut("{userId}")]
        public async Task<JsonResult> Put(Guid projectId, Guid userId, [FromBody]string name)
        {
            Ensure.ArgumentNotNullOrWhiteSpace(name, nameof(name));

            await this.ExecuteEditAsync<UserAggregate>(
                this.Tenant, Streams.Users, userId,
                async (aggregate) =>
                {
                    aggregate.RenameUser(userId, name);

                    await Task.CompletedTask;
                }).ConfigureAwait(false);

            return new JsonResult(new { id = userId });
        }

        public class UserPayload {  public string UserName { get; set; } public string Email { get; set; } }

        // POST: User
        [HttpPost]
        public async Task<JsonResult> Post(Guid projectId, [FromBody]UserPayload user)
        {
            Ensure.ArgumentNotNull(user, nameof(user));

            var userId = Guid.NewGuid();
            var tenant = this.Tenant;

            await ExecuteNewAsync(tenant, Streams.Users, userId, async () => {

                var aggregate = new UserAggregate();

                aggregate.RegisterNew(user.UserName, user.Email);

                return await Task.FromResult(aggregate);
            });

            return new JsonResult(new { id = userId });
        }

        // GET api/values
        [HttpGet]
        public JsonResult Get()
        {
            // ToDo : Query read model here.

            return new JsonResult(new
            {
                Name = "Moim Hossain",
                Address = "Zoetermeer"
            });
        }



        private Tenant Tenant
        {
            get
            {
                return new Tenant
                {
                    TenantId = Guid.Parse("{48A7FB91-7B14-4EB7-98FC-B145B6504BB6}"),
                    Name = "ABC Company"
                };
            }
        }
    }
}