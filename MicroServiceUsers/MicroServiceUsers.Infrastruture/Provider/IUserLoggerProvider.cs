using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceUser.Infrastructure.Provider
{
    public interface IUserLoggerProvider
    {
        Task LogInfo(string message);
        Task LogError(string message, Exception ex = null);
        Task LogWarning(string message);
    }
}
