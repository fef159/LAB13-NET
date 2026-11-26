# Publica en Azure la API, CitasApi, el gateway y la base de datos (planes gratuitos).
# Antes: instala Azure CLI y ejecuta "az login". Guía completa en DESPLIEGUE.md.
# Ejemplo:  .\Scripts\desplegar-azure.ps1 -Sufijo abc123 -Region eastus2
param(
    [Parameter(Mandatory)][ValidatePattern('^[a-z0-9]{3,12}$')][string]$Sufijo,
    [Parameter(Mandatory)][string]$Region,
    # Si ya tienes otra base gratuita, indica aquí su región: todas deben estar en la misma
    [string]$RegionSql,
    [string]$AdminSql = 'adminvet'
)
$ErrorActionPreference = 'Stop'
$raiz = Split-Path $PSScriptRoot -Parent
if (-not $RegionSql) { $RegionSql = $Region }

$grupo     = 'rg-veterinaria-s15'
$plan      = 'plan-veterinaria-s15'
$servidor  = if ($RegionSql -eq $Region) { "vet-sql-$Sufijo" } else { "vet-sql-$Sufijo-$RegionSql" }
$baseDatos = 'VeterinariaDB'
$apps = [ordered]@{
    'Veterinaria.Api' = "vet-api-$Sufijo"
    'CitasApi'        = "vet-citas-$Sufijo"
    'GatewayApi'      = "vet-gateway-$Sufijo"
}

function Invoke-Az {
    $salida = & az @args
    if ($LASTEXITCODE -ne 0) { throw "Falló: az $($args[0..2] -join ' ')" }
    $salida
}

function Write-Paso($texto) { Write-Host "`n== $texto" -ForegroundColor Cyan }

# ---------- 0. Comprobaciones y confirmación ----------
[Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12
# Una terminal abierta antes de instalar Azure CLI aún no la tiene en el PATH
$rutaAz = "$env:ProgramFiles\Microsoft SDKs\Azure\CLI2\wbin"
if (-not (Get-Command az -ErrorAction SilentlyContinue) -and (Test-Path "$rutaAz\az.cmd")) { $env:Path += ";$rutaAz" }
foreach ($c in 'az', 'dotnet', 'sqlcmd') {
    if (-not (Get-Command $c -ErrorAction SilentlyContinue)) { throw "No se encontró '$c'. Revisa DESPLIEGUE.md." }
}
$suscripcion = Invoke-Az account show --query name -o tsv
Write-Host "Suscripción: $suscripcion"
Write-Host "Se crearán en '$Region': grupo $grupo, plan F1 $plan, 3 aplicaciones web ($($apps.Values -join ', ')),"
Write-Host "el servidor SQL $servidor y la base gratuita $baseDatos (en '$RegionSql')."
if ((Read-Host '¿Continuar? (s/n)') -ne 's') { Write-Host 'Cancelado.'; return }

# La contraseña se pide aquí y no se guarda en ningún archivo del repositorio
Write-Host "`nInventa una contraseña para el administrador SQL '$AdminSql':"
Write-Host '  - 12 caracteres o más'
Write-Host '  - al menos una mayúscula, una minúscula y un número'
Write-Host '  - símbolos permitidos (opcionales): - _ . #'
$valida = $false
for ($intento = 1; $intento -le 5 -and -not $valida; $intento++) {
    $segura = Read-Host 'Contraseña' -AsSecureString
    $claveSql = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($segura))
    $valida = $claveSql -cmatch '^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z0-9\-_.#]{12,}$'
    if (-not $valida) {
        $falta = @()
        if ($claveSql.Length -lt 12) { $falta += 'llegar a 12 caracteres' }
        if ($claveSql -cnotmatch '[a-z]') { $falta += 'una minúscula' }
        if ($claveSql -cnotmatch '[A-Z]') { $falta += 'una mayúscula' }
        if ($claveSql -notmatch '\d') { $falta += 'un número' }
        if ($claveSql -cmatch '[^A-Za-z0-9\-_.#]') { $falta += 'quitar espacios u otros símbolos' }
        Write-Host "Escribiste $($claveSql.Length) caracteres. Falta: $($falta -join ', '). Escríbela a mano e inténtalo de nuevo." -ForegroundColor Yellow
    }
}
if (-not $valida) { throw 'Demasiados intentos. Vuelve a ejecutar el script.' }

# ---------- 1. Grupo, plan y aplicaciones ----------
Write-Paso 'Grupo de recursos y plan gratuito'
if ((Invoke-Az group exists --name $grupo) -ne 'true') {
    Invoke-Az group create --name $grupo --location $Region -o none
}
$planActual = Invoke-Az appservice plan list --resource-group $grupo --query "[?name=='$plan'].location" -o tsv
if (-not $planActual) {
    # --is-linux false: las versiones nuevas de Azure CLI crean planes Linux por defecto
    Invoke-Az appservice plan create --name $plan --resource-group $grupo --location $Region --sku F1 --is-linux false -o none
}
else { Write-Host "El plan ya existe en $planActual." }

Write-Paso 'Aplicaciones web (.NET 10)'
$hosts = @{}
foreach ($p in $apps.Keys) {
    Invoke-Az webapp create --name $apps[$p] --resource-group $grupo --plan $plan --runtime 'DOTNET:10' -o none
    Invoke-Az webapp update --name $apps[$p] --resource-group $grupo --https-only true -o none
    $hosts[$p] = Invoke-Az webapp show --name $apps[$p] --resource-group $grupo --query defaultHostName -o tsv
    Write-Host "$p -> https://$($hosts[$p])"
}

# ---------- 2. Base de datos ----------
Write-Paso 'Servidor SQL y base de datos gratuita'
Invoke-Az sql server create --name $servidor --resource-group $grupo --location $RegionSql --admin-user $AdminSql --admin-password $claveSql -o none
Invoke-Az sql server firewall-rule create --name ServiciosAzure --server $servidor --resource-group $grupo --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0 -o none
$existe = Invoke-Az sql db list --server $servidor --resource-group $grupo --query "[?name=='$baseDatos'].name" -o tsv
if (-not $existe) {
    # AutoPause: al agotar el límite gratuito la base se pausa y no genera cobros
    Invoke-Az sql db create --name $baseDatos --server $servidor --resource-group $grupo --edition GeneralPurpose --family Gen5 --capacity 2 --compute-model Serverless --use-free-limit true --free-limit-exhaustion-behavior AutoPause --backup-storage-redundancy Local -o none
}

Write-Paso 'Script SQL en la base de la nube'
$conexionSql = @('-S', "tcp:$servidor.database.windows.net,1433", '-d', $baseDatos, '-U', $AdminSql, '-N', '-b', '-l', '60')
$consulta = "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.tables WHERE name IN ('Propietarios','Mascotas','Usuarios')"
$env:SQLCMDPASSWORD = $claveSql
try {
    $tablas = $null
    for ($i = 1; $i -le 6 -and $null -eq $tablas; $i++) {
        $ErrorActionPreference = 'Continue'
        # Se une en una sola línea: el mensaje de error de sqlcmd llega partido
        $r = (& sqlcmd @conexionSql -h -1 -W -Q $consulta 2>&1 | ForEach-Object { "$_" }) -join ' '
        $codigo = $LASTEXITCODE
        $ErrorActionPreference = 'Stop'
        if ($codigo -eq 0) { $tablas = [int]$r.Trim() }
        elseif ($r -match "IP\s+address\s+'([\d\.]+)'") {
            Write-Host "Permitiendo tu IP en el firewall del servidor SQL..."
            Invoke-Az sql server firewall-rule create --name MiEquipo --server $servidor --resource-group $grupo --start-ip-address $Matches[1] --end-ip-address $Matches[1] -o none
            Start-Sleep -Seconds 20
        }
        else { Write-Host "La base aún no responde (intento $i): $($r.Trim())"; Start-Sleep -Seconds 20 }
    }
    if ($null -eq $tablas) { throw 'No se pudo conectar con la base de datos.' }
    if ($tablas -eq 0) {
        & sqlcmd @conexionSql -f 65001 -i (Join-Path $raiz 'Scripts\VeterinariaDB_nube.sql')
        if ($LASTEXITCODE -ne 0) { throw 'El script SQL falló.' }
    }
    elseif ($tablas -eq 3) { Write-Host 'Las tablas ya existen: no se vuelve a ejecutar el script.' }
    else { throw "La base tiene solo $tablas de las 3 tablas. Revísala antes de continuar." }
}
finally { Remove-Item Env:\SQLCMDPASSWORD -ErrorAction SilentlyContinue }

# ---------- 3. Secretos como configuración de cada aplicación ----------
Write-Paso 'Clave JWT y cadena de conexión'
$bytes = New-Object byte[] 48
[Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
$claveJwt = [Convert]::ToBase64String($bytes)
$cadena = "Server=tcp:$servidor.database.windows.net,1433;Initial Catalog=$baseDatos;User ID=$AdminSql;Password=$claveSql;Encrypt=True;TrustServerCertificate=False;Connection Timeout=60"
foreach ($p in $apps.Keys) {
    $ajustes = @(@{ name = 'Jwt__Key'; value = $claveJwt })
    if ($p -eq 'Veterinaria.Api') { $ajustes += @{ name = 'ConnectionStrings__VeterinariaDb'; value = $cadena } }
    $temporal = [IO.Path]::GetTempFileName()
    try {
        [IO.File]::WriteAllText($temporal, (ConvertTo-Json -InputObject $ajustes))
        Invoke-Az webapp config appsettings set --name $apps[$p] --resource-group $grupo --settings "@$temporal" -o none
    }
    finally { Remove-Item $temporal -Force }
}

# ---------- 4. ocelot.Production.json con los hosts reales ----------
Write-Paso 'ocelot.Production.json'
$ocelot = [IO.File]::ReadAllText((Join-Path $raiz 'GatewayApi\ocelot.json'))
$ocelot = $ocelot.Replace('"DownstreamScheme": "http"', '"DownstreamScheme": "https"')
$ocelot = $ocelot.Replace('"Host": "localhost", "Port": 5249', "`"Host`": `"$($hosts['Veterinaria.Api'])`", `"Port`": 443")
$ocelot = $ocelot.Replace('"Host": "localhost", "Port": 5056', "`"Host`": `"$($hosts['CitasApi'])`", `"Port`": 443")
$ocelot = $ocelot.Replace('http://localhost:5000', "https://$($hosts['GatewayApi'])")
if ($ocelot -match 'localhost') { throw 'Quedan direcciones localhost en ocelot.Production.json.' }
[IO.File]::WriteAllText((Join-Path $raiz 'GatewayApi\ocelot.Production.json'), $ocelot)

# ---------- 5. Publicar ----------
$trabajo = Join-Path ([IO.Path]::GetTempPath()) "veterinaria-s15-$Sufijo"
foreach ($p in $apps.Keys) {
    Write-Paso "Publicando $p"
    $salida = Join-Path $trabajo $p
    $zip = "$salida.zip"
    if (Test-Path $salida) { Remove-Item $salida -Recurse -Force }
    dotnet publish (Join-Path $raiz $p) -c Release -o $salida --nologo
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish falló en $p." }
    # Los archivos locales con secretos no se suben
    Remove-Item (Join-Path $salida 'appsettings.Development.json'), (Join-Path $salida 'appsettings.Production.json') -ErrorAction SilentlyContinue
    Compress-Archive -Path (Join-Path $salida '*') -DestinationPath $zip -Force
    Invoke-Az webapp deploy --name $apps[$p] --resource-group $grupo --src-path $zip --type zip -o none
}

# ---------- 6. Comprobación ----------
Write-Paso 'Comprobación (el primer request puede tardar)'
function Get-Codigo($url) {
    try { [int](Invoke-WebRequest $url -UseBasicParsing -TimeoutSec 100).StatusCode }
    catch { if ($_.Exception.Response) { [int]$_.Exception.Response.StatusCode } else { 0 } }
}
$pruebas = @(
    @("https://$($hosts['Veterinaria.Api'])/api/mascotas", 200),
    @("https://$($hosts['CitasApi'])/api/citas", 401),
    @("https://$($hosts['GatewayApi'])/gateway/mascotas", 200)
)
foreach ($t in $pruebas) {
    $codigo = 0
    for ($i = 1; $i -le 4 -and $codigo -ne $t[1]; $i++) {
        $codigo = Get-Codigo $t[0]
        if ($codigo -ne $t[1]) { Start-Sleep -Seconds 15 }
    }
    $color = if ($codigo -eq $t[1]) { 'Green' } else { 'Red' }
    Write-Host "$($t[0])  ->  $codigo (esperado $($t[1]))" -ForegroundColor $color
}

Write-Host "`nGateway:  https://$($hosts['GatewayApi'])"
Write-Host "API:      https://$($hosts['Veterinaria.Api'])"
Write-Host "CitasApi: https://$($hosts['CitasApi'])"
Write-Host 'Haz commit de GatewayApi\ocelot.Production.json (no contiene claves).'
