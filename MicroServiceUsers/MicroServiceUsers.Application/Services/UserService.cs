using ServiceUser.Application.Interfaces;
using ServiceUser.Domain.Entities;
using ServiceUser.Domain.Ports;
using ServiceUser.Domain.Rules;
using ServiceUser.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ServiceUser.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public Task<User> GetUserById(int id) => _userRepository.GetByIdAsync(id);

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            var allUsers = await _userRepository.GetAllAsync();

            // 🔥 Obtener el rol directamente desde el JWT
            var userClaims = _httpContextAccessor.HttpContext?.User;
            var roles = userClaims?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();

            Console.WriteLine($"[DEBUG] Roles del usuario autenticado desde JWT: {string.Join(", ", roles)}");

            // Filtrado según el rol
            if (roles.Contains("SuperAdmin"))
                return allUsers;

            if (roles.Contains("Admin"))
                return allUsers.Where(u => u.role == "Admin" || u.role == "Instructor");

            if (roles.Contains("Instructor"))
                return allUsers.Where(u => u.role == "Instructor");

            return Enumerable.Empty<User>();
        }

        public async Task<User> CreateUser(User newUser)
        {
            // Validar usuario
            var validationResult = UserValidator.Validar(newUser);
            if (validationResult.IsFailure)
                throw new ArgumentException(validationResult.Error);

            // Validar contraseña (la propiedad password tiene la contraseña temporal)
            var passwordResult = PasswordRules.Validar(newUser.password);
            if (passwordResult.IsFailure)
                throw new ArgumentException(passwordResult.Error);

            newUser.must_change_password = true;

            return await _userRepository.CreateAsync(newUser);
        }
        public async Task<User> UpdateUser(User userToUpdate)
        {
            var userClaims = _httpContextAccessor.HttpContext?.User;
            var roles = userClaims?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();

            Console.WriteLine($"[DEBUG] Roles del usuario autenticado desde JWT: {string.Join(", ", roles)}");

            // Verificar si el rol del usuario se está cambiando a 'Admin' y si el usuario autenticado no es Admin
            if (userToUpdate.role == "Admin" && !roles.Contains("Admin"))
            {
                throw new UnauthorizedAccessException("No tienes permisos para asignar el rol de Admin.");
            }

            // Validar los datos del usuario actualizado
            var validationResult = UserValidator.Validar(userToUpdate);
            if (validationResult.IsFailure)
                throw new ArgumentException(validationResult.Error);

            // Actualizar en el repositorio
            return await _userRepository.UpdateAsync(userToUpdate);
        }

        public async Task<bool> DeleteUser(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return false;

            // 🚫 NO PERMITIR eliminar SuperAdmin
            if (user.role == "SuperAdmin")
                throw new InvalidOperationException("El SuperAdmin no puede ser eliminado.");

            return await _userRepository.DeleteByIdAsync(userId);
        }


        public async Task<bool> UpdatePasswordAsync(int userId, string newPassword)
        {
            // Validar contraseña
            var passwordResult = PasswordRules.Validar(newPassword);
            if (passwordResult.IsFailure)
                throw new ArgumentException(passwordResult.Error);

            // Ahora usa "password" y "must_change_password"
            return await _userRepository.UpdatePasswordAsync(userId, newPassword);
        }

        public async Task UpdateUserTokenAsync(int userId, string token, DateTime expiresAt)
        {
            // Obtener usuario por ID
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("Usuario no encontrado");

            // Asignar token y expiración
            user.jwt_token = token;

            // Si el campo en User es nullable, usa .Value, si no, asigna directamente
            user.token_expires_at = expiresAt;

            // Guardar cambios
            await _userRepository.UpdateAsync(user);
        }

    }
}
