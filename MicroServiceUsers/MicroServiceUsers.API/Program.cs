using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ServiceUser.Application;
using ServiceUser.Infrastructure;
using ServiceUser.Infrastructure.DependencyInjection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// -----------------------
// 1. Registrar Módulos
// -----------------------
builder.Services.AddUserModule(sp =>
    builder.Configuration.GetConnectionString("Postgres"));


// -----------------------
// 2. Registrar UserContext
// -----------------------
builder.Services.AddHttpContextAccessor();
// builder.Services.AddScoped<IUserContext, UserContext>();  // si lo usas

// -----------------------
// 3. Configurar JWT
// -----------------------
var jwtKey = builder.Configuration["Jwt:Key"];
var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// -----------------------
// 4. CORS
// -----------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// -----------------------
// 5. Swagger
// -----------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -----------------------
builder.Services.AddControllers();

var app = builder.Build();

app.UseCors("AllowWebApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
