using MicroServiceUsers.Infrastruture.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ServiceUser.Application.Interfaces;
using ServiceUser.Domain.Entities;
using System.Text.Json;


namespace MicroServiceUsers.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly TokenService _tokenService;
        private readonly IConfiguration _config;

        public AuthController(IUserService userService, TokenService tokenService, IConfiguration config)
        {
            _userService = userService;
            _tokenService = tokenService;
            _config = config; 
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            Console.WriteLine("========== LOGIN REQUEST ==========");
            Console.WriteLine($"Email recibido: {login.Email}");

            var user = await _userService.GetUserByEmailAsync(login.Email);

            if (user == null)
            {
                Console.WriteLine("Usuario NO encontrado en la BD.");
                return Unauthorized("Email o contraseña inválidos");
            }

            // Validación de contraseña
            if (user.password != login.Password)
            {
                Console.WriteLine("ERROR: Contraseña inválida.");
                return Unauthorized("Email o contraseña inválidos");
            }

            Console.WriteLine("Contraseña válida ✔");

            // Generar token
            var token = _tokenService.GenerateToken(
                user.id.ToString(),
                user.name,
                user.email,
                user.role
            );

            Console.WriteLine("=== TOKEN GENERADO ===");
            Console.WriteLine(token);

            // Guardar token en la DB
            var expiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpireMinutes"]));
            user.jwt_token = token;
            user.token_expires_at = expiresAt;
            await _userService.UpdateUserTokenAsync(user.id, token, expiresAt);

            // Respuesta al cliente
            var response = new
            {
                token,
                user = new
                {
                    user.id,
                    user.name,
                    user.email,
                    user.role,
                    mustChangePassword = user.must_change_password
                }
            };

            return Ok(response);
        }


        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // No es necesario hacer nada en el servidor si usas JWT
            Console.WriteLine("Logout realizado, el token ha sido eliminado del lado del cliente.");

            return Ok(new { message = "Logout exitoso. El token ha sido eliminado del cliente." });
        }


        [HttpGet("test/{Email}")]
        public async Task<IActionResult> Test(string Email)
        {
            var u = await _userService.GetUserByEmailAsync(Email);
            return Ok(u);
        }

        [HttpGet("test-token/{email}")]
        public IActionResult TestToken(string email)
        {
            int id = 123;                  // ejemplo de id
            string name = "UsuarioPrueba";  // ejemplo de nombre

            // Convertir el ID a string para GenerateToken
            var token = _tokenService.GenerateToken(
                id.ToString(),
                name,
                email,
                "Admin"
            );

            return Ok(new { token });
        }


        [HttpPut("{id}/token")]
        public async Task<IActionResult> UpdateToken(int id, [FromBody] TokenUpdateDto dto)
        {
            await _userService.UpdateUserTokenAsync(id, dto.Token, dto.ExpiresAt);
            return Ok();
        }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class TokenUpdateDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
