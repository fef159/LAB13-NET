using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Tabla de rutas: ocelot.json (local) y, si existe, ocelot.{Entorno}.json (nube).
// Las variables de entorno van al final para poder sobrescribir cualquier valor al publicar.
builder.Configuration
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// El gateway valida el JWT con la misma clave, emisor y audiencia que los servicios
var jwt = builder.Configuration.GetSection("Jwt");
var clave = jwt["Key"];
if (string.IsNullOrWhiteSpace(clave) || clave.StartsWith('<') || clave.Length < 32)
    throw new InvalidOperationException("Jwt:Key debe tener al menos 32 caracteres (appsettings.Development.json o variable de entorno Jwt__Key).");

builder.Services.AddAuthentication()
    .AddJwtBearer("Bearer", options =>
    {
        options.MapInboundClaims = false;
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

builder.Services.AddOcelot();

var app = builder.Build();

await app.UseOcelot();

app.Run();
