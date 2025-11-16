using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceUser.Infrastructure.Provider
{
    public interface IUserConnectionProvider
    {
        string GetConnectionString();
    }
}
