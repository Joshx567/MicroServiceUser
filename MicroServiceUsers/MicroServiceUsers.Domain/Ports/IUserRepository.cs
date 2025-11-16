using ServiceUser.Domain.Entities;

namespace ServiceUser.Domain.Ports
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByEmailAsync(string email);
        Task<bool> UpdatePasswordAsync(int id, string password);
    }
}