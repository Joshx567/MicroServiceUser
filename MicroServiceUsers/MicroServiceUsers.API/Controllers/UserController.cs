using Microsoft.AspNetCore.Mvc;
using ServiceUser.Application.Interfaces;
using ServiceUser.Domain.Entities;

namespace MicroServiceUsers.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;

        public UserController(IUserService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obtiene todos los usuarios.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _service.GetAllUsers();
            return Ok(users);
        }

        /// <summary>
        /// Obtiene un usuario por ID.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _service.GetUserById(id);
            if (user == null)
                return NotFound(new { error = "Usuario no encontrado" });

            return Ok(user);
        }

        /// <summary>
        /// Crea un nuevo usuario.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] User newUser)
        {
            var created = await _service.CreateUser(newUser);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Actualiza un usuario existente.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] User user)
        {
            user.Id = id;

            var updated = await _service.UpdateUser(user);
            if (updated == null)
                return NotFound(new { error = "Usuario no encontrado" });

            return Ok(updated);
        }

        /// <summary>
        /// Elimina un usuario.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteUser(id);
            if (!ok)
                return NotFound(new { error = "Usuario no encontrado" });

            return Ok(new { message = "Usuario eliminado correctamente" });
        }

        /// <summary>
        /// Actualiza la contraseña del usuario.
        /// </summary>
        [HttpPut("{id:int}/password")]
        public async Task<IActionResult> UpdatePassword(int id, [FromBody] string newPassword)
        {
            var ok = await _service.UpdatePasswordAsync(id, newPassword);
            if (!ok)
                return NotFound(new { error = "Usuario no encontrado" });

            return Ok(new { message = "Contraseña actualizada correctamente" });
        }
    }
}
