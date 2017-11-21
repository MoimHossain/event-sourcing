using System;
using System.Collections.Generic;
using System.Text;
using SuperNova.Shared.DomainObjects;
using System.Threading.Tasks;
using SuperNova.Shared.Dtos;

namespace SuperNova.Shared.Repositories.Materialize
{
    public interface IUserMaterialzeRepository : IRepository
    {
        Task AddUserAsync(UserDto currentState, long version);
        Task UpdateUserAsync(UserDto currentState, long version);
    }
}
