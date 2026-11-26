# Despliegue en la nube (Semana 15)

| Pieza | Proveedor (plan gratuito) | Motivo |
|---|---|---|
| API de la S14 + base de datos | MonsterASP.NET (1 sitio + 1 base SQL Server) | Admite .NET 10 y SQL Server: no hay que cambiar el proveedor de EF Core |
| CitasApi (microservicio 2) | Render (Web Service, Docker) | Se publica desde el repositorio de GitHub |
| GatewayApi | Render (Web Service, Docker) | Igual que CitasApi |

Antes de empezar revisa las condiciones vigentes de cada plan. En Render el servicio gratuito se
duerme tras 15 minutos sin tráfico y el primer request tarda alrededor de un minuto.

## 1. Base de datos y API (MonsterASP.NET)

1. Crea la cuenta, un sitio web gratuito y una base de datos MSSQL.
2. Ejecuta `Scripts/VeterinariaDB_nube.sql` sobre esa base (desde SSMS o el editor del panel).
3. En `Veterinaria.Api` crea `appsettings.Production.json` (está en `.gitignore`):

```json
{
  "ConnectionStrings": { "VeterinariaDb": "<cadena de conexión que entrega el panel>" },
  "Jwt": { "Key": "<clave nueva de 32 caracteres o más>" }
}
```

4. Publica el proyecto `Veterinaria.Api` (en Visual Studio: Publicar → Web Deploy, con el perfil
   que descargas del panel). `appsettings.Development.json` no se usa en producción: no hace falta subirlo.
5. Comprueba `https://<tu-api>.runasp.net/api/mascotas`.

## 2. Repositorio

Sube la rama a GitHub. Comprueba que no aparezcan `appsettings.Development.json` ni
`appsettings.Production.json`.

## 3. CitasApi (Render)

1. New → Web Service → elige el repositorio y la rama.
2. Root Directory: `CitasApi`. Runtime: Docker. Plan: Free.
3. Variable de entorno `Jwt__Key`: la misma clave que usa la API.
4. Comprueba que `https://<citas>.onrender.com/api/citas` responde 401.

## 4. GatewayApi (Render)

1. Edita `GatewayApi/ocelot.Production.json`: reemplaza `REEMPLAZAR-API.runasp.net`,
   `REEMPLAZAR-CITAS.onrender.com` y `REEMPLAZAR-GATEWAY.onrender.com` por los hosts reales. Haz commit y push.
2. New → Web Service. Root Directory: `GatewayApi`. Runtime: Docker. Plan: Free.
3. Variable de entorno `Jwt__Key`: la misma clave.
4. Si Render asigna al gateway un host distinto del previsto, corrige `BaseUrl` y vuelve a subir.

## 5. Pruebas en la nube

En `pruebas-gateway.http` cambia `@gateway` y `@citas` por las direcciones públicas y repite
200, 201, 204, 401, 403, 404 y 502. Para el 502, suspende el servicio CitasApi en Render y, al
terminar, reactívalo. Si el proveedor responde con otro código para un servicio suspendido,
anótalo tal como salga.
