# Despliegue en Azure (Semana 15)

| Pieza | Servicio de Azure (plan gratuito) |
|---|---|
| GatewayApi, Veterinaria.Api y CitasApi | App Service, plan F1 (Windows), una aplicación web por proyecto |
| Base de datos | Azure SQL Database, oferta gratuita (serverless) |

Motivo: un solo proveedor y SQL Server, así no se cambia el proveedor de EF Core ni la lógica
de la API. Revisa las condiciones vigentes: F1 tiene una cuota diaria de CPU (60 minutos) y la
base gratuita un límite mensual (100 000 segundos de vCore).

## 1. Preparación (una sola vez)

1. Ten una suscripción de Azure activa. En este laboratorio se usó una prueba gratuita; también
   sirve [Azure for Students](https://azure.microsoft.com/free/students/).
2. Instala Azure CLI y abre una terminal nueva:

   ```powershell
   winget install --exact --id Microsoft.AzureCLI
   ```

3. Inicia sesión (se abre el navegador):

   ```powershell
   az login
   ```

4. Elige una región que tu suscripción permita y que tenga cuota para el plan F1. Si hay
   restricciones, aparecen en el portal: Policy → Assignments → "Allowed resource deployment
   regions".

## 2. Publicar todo

Desde la carpeta de la solución, con un sufijo propio (3 a 12 letras minúsculas o números)
para que los nombres sean únicos:

```powershell
.\Scripts\desplegar-azure.ps1 -Sufijo TUSUFIJO -Region TUREGION
```

Si la suscripción ya tiene otra base gratuita, la nueva puede tener que ir en la misma región.
Indícala con `-RegionSql`. El despliegue de este laboratorio fue:

```powershell
.\Scripts\desplegar-azure.ps1 -Sufijo s15k7q -Region westus3 -RegionSql swedencentral
```

El script pide confirmación y una contraseña nueva para el administrador SQL, y luego:

1. Crea el grupo `rg-veterinaria-s15`, el plan F1 y las tres aplicaciones web.
2. Crea el servidor SQL y la base gratuita (se pausa al agotar el límite) y ejecuta
   `Scripts/VeterinariaDB_nube.sql`.
3. Genera una clave JWT nueva y la configura, igual, en las tres aplicaciones. La cadena de
   conexión queda como configuración de la API. Ninguna se guarda en el repositorio.
4. Escribe los hosts reales en `GatewayApi/ocelot.Production.json`.
5. Publica los tres proyectos y comprueba que responden.

Si falla a la mitad, corrige la causa y vuelve a ejecutarlo con el mismo sufijo.

Errores frecuentes:

- **Región no permitida** o sin capacidad: usa otra de la lista de tu suscripción.
- **"Current Limit (F1 App Service instances): 0":** esa región no tiene cuota para el plan
  gratuito; prueba otra. En la prueba gratuita usada para este laboratorio funcionó `westus3`.
- **`InternalServerError` al crear la base:** en este laboratorio ocurrió en `westus3`; se
  resolvió creando la base en la región de la otra base gratuita (`-RegionSql swedencentral`).
- **Nombre en uso:** cambia el sufijo.
- **500 en el primer request:** la base puede estar reanudándose; espera un minuto y repite.

## 3. Después de publicar

Haz commit y push de `GatewayApi/ocelot.Production.json`.

Los usuarios de prueba del script SQL tienen una clave que está en el repositorio. Es aceptable
para el laboratorio; elimina los recursos cuando ya esté calificado.

## 4. Pruebas en la nube

En `pruebas-gateway.http` cambia `@gateway` y `@citas` por las direcciones `https://...` y repite
200, 201, 204, 401, 403 y 404.

**502:** al detener una aplicación en App Service, Azure responde 403 por ella, así que el gateway
no devolvería 502. Para provocarlo, apunta temporalmente la ruta de citas (índice 6) a un host
que no existe:

```powershell
az webapp config appsettings set --name vet-gateway-TUSUFIJO --resource-group rg-veterinaria-s15 --settings Routes__6__DownstreamHostAndPorts__0__Host=citas-detenido.invalid
```

Prueba `GET /gateway/citas` con token, comprueba que las mascotas siguen respondiendo y deshaz
el cambio:

```powershell
az webapp config appsettings delete --name vet-gateway-TUSUFIJO --resource-group rg-veterinaria-s15 --setting-names Routes__6__DownstreamHostAndPorts__0__Host
```

Explica en el video que así se simula la caída del microservicio.

## 5. Eliminar los recursos (después de la calificación)

Esto borra definitivamente las tres aplicaciones y la base de datos:

```powershell
az group delete --name rg-veterinaria-s15
```
