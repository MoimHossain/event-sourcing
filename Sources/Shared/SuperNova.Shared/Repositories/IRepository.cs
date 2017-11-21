using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SuperNova.Shared.Repositories
{
    public interface IRepository
    {
        Task<bool> Init();
    }
}
