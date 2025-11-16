using ServiceUser.Domain.Entities;

namespace ServiceUser.Application.Interfaces
{
    public interface IUserService
    {
        Task<User> GetUserById(int id);
        Task<IEnumerable<User>> GetAllUsers();
        Task<User> CreateUser(User newUser);
        Task<User> UpdateUser(User userToUpdate); // <- devuelve el usuario actualizado
        Task<bool> DeleteUser(int userId);
        Task<bool> UpdatePasswordAsync(int userId, string newHash);
    }
}
