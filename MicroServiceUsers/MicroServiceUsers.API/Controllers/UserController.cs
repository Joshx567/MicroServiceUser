using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceUser.Application.Interfaces;
using ServiceUser.Domain.Entities;
using System.Security.Claims;

namespace MicroServiceUsers.API.Controllers
{
    [ApiController]
    [Route("api/users")]
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
        [Authorize]
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
            try
            {
                var created = await _service.CreateUser(newUser);
                return CreatedAtAction(nameof(GetById), new { id = created.id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un usuario existente.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] User user)
        {
            try
            {
                user.id = id;
                var updated = await _service.UpdateUser(user);
                return Ok(updated);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message); // 403
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message }); // 400
            }
        }

        /// <summary>
        /// Elimina un usuario.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var ok = await _service.DeleteUser(id);

                if (!ok)
                    return NotFound(new { error = "Usuario no encontrado" });

                return Ok(new { message = "Usuario eliminado correctamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        /// <summary>
        /// Actualiza la contraseña del usuario.
        /// </summary>
        [HttpPut("{id:int}/password")]
        public async Task<IActionResult> UpdatePassword(int id, [FromBody] string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                return BadRequest(new { error = "La contraseña no puede estar vacía" });

            try
            {
                var ok = await _service.UpdatePasswordAsync(id, newPassword);
                if (!ok)
                    return NotFound(new { error = "Usuario no encontrado" });

                return Ok(new { message = "Contraseña actualizada correctamente" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
