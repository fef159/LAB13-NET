using System.Text;
using CitasApi.Datos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<CitasEnMemoria>();

// Mismo JWT que la API de la semana 14: misma clave, emisor y audiencia
var jwt = builder.Configuration.GetSection("Jwt");
var clave = jwt["Key"];
if (string.IsNullOrWhiteSpace(clave) || clave.StartsWith('<') || clave.Length < 32)
    throw new InvalidOperationException("Jwt:Key debe tener al menos 32 caracteres (appsettings.Development.json o variable de entorno Jwt__Key).");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // conservar los nombres cortos (sub, email, role)
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave)),
            RoleClaimType = "role",
            NameClaimType = "email",
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
