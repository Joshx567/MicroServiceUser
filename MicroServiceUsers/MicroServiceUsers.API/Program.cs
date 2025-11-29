using MicroServiceUsers.Infrastruture.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using ServiceUser.Application;
using ServiceUser.Application.Interfaces;
using ServiceUser.Application.Services;
using ServiceUser.Infrastructure;
using ServiceUser.Infrastructure.DependencyInjection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// -----------------------
// 1. Registrar módulos y TokenService
// -----------------------
builder.Services.AddUserModule(sp => builder.Configuration.GetConnectionString("Postgres"));
builder.Services.AddScoped<TokenService>();
builder.Services.AddHttpContextAccessor(); // Agregar esto para poder usar IHttpContextAccessor
builder.Services.AddScoped<IUserService, UserService>();

// -----------------------
// 2. Configuración de JWT
// -----------------------
var jwtKey = builder.Configuration["Jwt:Key"];
var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role", // <- esto es clave
            ClockSkew = TimeSpan.Zero // Elimina el desfase del reloj si es necesario
        };
    });

// -----------------------
// 3. CORS
// -----------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.WithOrigins("http://localhost:5279") // Asegúrate de colocar el dominio de tu frontend
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


// -----------------------
// 4. Swagger
// -----------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MicroServiceUsers API",
        Version = "v1",
        Description = "API REST para microservicio de usuarios"
    });
});

// -----------------------
// 5. Controllers
// -----------------------
builder.Services.AddControllers();

var app = builder.Build();


app.UseCors("AllowWebApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MicroServiceUsers API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
