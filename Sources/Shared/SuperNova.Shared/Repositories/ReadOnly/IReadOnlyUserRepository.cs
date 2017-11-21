using SuperNova.Shared.DomainObjects;
using SuperNova.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SuperNova.Shared.Repositories.ReadOnly
{
    public interface IReadOnlyUserRepository : IRepository
    {
        Task<IEnumerable<UserDto>> QueryAsync(Expression<Func<UserDto, bool>> predicate);
    }
}
