using Microsoft.EntityFrameworkCore;
using Veterinaria.Datos;
using Veterinaria.Datos.Repositorios;
using Veterinaria.Negocio;
using Veterinaria.Negocio.Servicios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// DbContext (la cadena de conexión vive en appsettings.json)
builder.Services.AddDbContext<VeterinariaContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("VeterinariaDb")));

// Repositorios y servicios
builder.Services.AddScoped<IPropietarioRepositorio, PropietarioRepositorio>();
builder.Services.AddScoped<IMascotaRepositorio, MascotaRepositorio>();
builder.Services.AddScoped<IPropietarioServicio, PropietarioServicio>();
builder.Services.AddScoped<IMascotaServicio, MascotaServicio>();

// AutoMapper: un único MappingProfile en la capa de Negocio
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "Veterinaria API v1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
