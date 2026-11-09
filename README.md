# Catálogo Galáctico - Web API

API RESTful desarrollada en **.NET 9.0** para gestionar un catálogo de personajes, planetas y eventos históricos. El proyecto implementa una arquitectura modular con servicios, repositorios (EF Core), cifrado de datos sensibles (AES-GCM), hash de contraseñas (Identity) y documentación automática con Swagger.

---

## Requisitos Previos (Para la PC de Defensa)

Antes de empezar, asegúrate de que la computadora del laboratorio tenga instalado lo siguiente:

* **.NET 9.0 SDK** (Obligatorio, si tiene una versión menor, el proyecto no compilará).
* **MySQL Server** (Corriendo en el puerto 3306) y un cliente (MySQL Workbench, DBeaver o consola).
* **Git** instalado.

---

## PASO 1: Configurar la Base de Datos en MySQL

Primero, debemos crear la base de datos y un usuario exclusivo para la API. Abre tu cliente de MySQL e inicia sesión con el usuario `root`. Ejecuta el siguiente script:

```sql
-- 1. Crear la base de datos
CREATE DATABASE catalogo_galactico_db CHARACTER SET utf8mb4;

-- 2. Crear el usuario (puedes cambiar 'ClaveSegura123' por la que prefieras)
CREATE USER 'catalogo_user'@'localhost' IDENTIFIED BY 'ClaveSegura123';

-- 3. Darle todos los permisos sobre esta base de datos
GRANT ALL PRIVILEGES ON catalogo_galactico_db.* TO 'catalogo_user'@'localhost';

-- 4. Aplicar los cambios
FLUSH PRIVILEGES;

```

Para comprobar que todo salió bien, conéctate con el nuevo usuario:

```bash
mysql -u catalogo_user -p catalogo_galactico_db

```

*(Ingresa la clave cuando la pida. Escribe `SHOW TABLES;` y debería estar vacío).*

---

## PASO 2: Clonar y Restaurar el Proyecto

Abre una terminal (PowerShell recomendado) y ejecuta:

```powershell
# 1. Clonar el repositorio
git clone <URL_DE_TU_REPOSITORIO>

# 2. Entrar a la carpeta del proyecto (Donde está el archivo .csproj)
cd CatalogoGalacticoApi

# 3. Restaurar dependencias (Descarga Pomelo MySQL, EF Core, Swagger, etc.)
dotnet restore

```

*Nota: Si la PC del laboratorio da error de paquetes faltantes, instálalos manualmente con:*

```powershell
dotnet add package Pomelo.EntityFrameworkCore.MySql --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.0
dotnet add package Microsoft.AspNetCore.OpenApi --version 9.0.0
dotnet add package Swashbuckle.AspNetCore --version 7.0.0

```

---

## PASO 3: Configurar Secret Manager (IMPORTANTE)

Por seguridad, las credenciales no están en el código. Debemos inyectarlas localmente usando `user-secrets`.

Asegúrate de estar en la carpeta donde está el archivo `.csproj` y ejecuta:

**1. Inicializar los secretos del proyecto:**

```powershell
dotnet user-secrets init

```

**2. Guardar la Cadena de Conexión:**
*(Asegúrate de poner el usuario y contraseña que creaste en el Paso 1)*

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=catalogo_galactico_db;User=catalogo_user;Password=ClaveSegura123"

```

**3. Generar y Guardar la Clave de Cifrado (AES-GCM):**
La API requiere una clave de exactamente 32 bytes (256 bits) en formato Base64 para cifrar las coordenadas de los personajes.

Ejecuta este script en **PowerShell** para generar una clave válida al instante:

```powershell
$bytes = New-Object byte[] 32
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
[Convert]::ToBase64String($bytes)

```

*Copia el texto extraño que te devuelve (ej: `vF8a/9s8X...`) y guárdalo en los secretos ejecutando:*

```powershell
dotnet user-secrets set "Cifrado:Clave" "PEGA_AQUI_LA_CLAVE_GENERADA"

```

**4. Verificar que se guardaron bien:**

```powershell
dotnet user-secrets list

```

---

## 🏗️ PASO 4: Aplicar Migraciones (Entity Framework Core)

Para que EF Core cree las tablas (`Personajes`, `Eventos`, `Planetas`, `Cards`) necesitamos la herramienta `dotnet-ef`.

**1. Instalar la herramienta globalmente (por si la PC no la tiene):**

```powershell
dotnet tool install --global dotnet-ef --version 9.0.0

```

**2. Crear la base de datos a partir de las migraciones existentes:**

```powershell
dotnet ef database update

```

*(Si por alguna razón el comando anterior falla diciendo que no encuentra la cadena de conexión, fuérzalo inyectándole la conexión directa):*

```powershell
dotnet ef database update --connection "Server=localhost;Port=3306;Database=catalogo_galactico_db;Uid=catalogo_user;Password=ClaveSegura123;SslMode=none"

```

---

## ▶️ PASO 5: Ejecutar la API

Con la base de datos lista, arranca el servidor:

```powershell
dotnet run

```

Si todo está correcto, la consola te mostrará una URL (ej: `http://localhost:5000`). Abre el navegador y dirígete a:
👉 **`http://localhost:5000/swagger`**

Allí podrás probar todos los endpoints (CRUD de Planetas, Reportes, Personajes y Eventos).

---

## Troubleshooting / Resolución de Problemas Comunes en Defensa

Si algo falla al desplegar en otra PC, revisa esta tabla:

| Error | Causa Probable | Solución Inmediata |
| --- | --- | --- |
| **`dotnet ef no reconocido`** | La herramienta global no está instalada en esa PC. | Ejecuta: `dotnet tool install --global dotnet-ef --version 9.0.0`. Si sigue sin reconocerlo, cierra y abre PowerShell de nuevo. |
| **`Access denied for user 'catalogo_user'`** | La contraseña en el user-secrets está mal escrita o no le diste `FLUSH PRIVILEGES`. | Vuelve a correr el script SQL de creación de usuario. Luego actualiza el secreto: `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=..."` |
| **`Target framework net9.0 is not supported`** | La PC del laboratorio tiene instalado .NET 8 o inferior. | Descarga e instala rápidamente el SDK de .NET 9.0 desde la página oficial de Microsoft. |
| **`Cannot add or update a child row (Foreign Key constraint fails)`** | Al correr la migración, la tabla `Eventos` tenía datos huérfanos sin un `PlanetaId` válido. | Entra a MySQL, ejecuta `DELETE FROM Eventos;` y vuelve a correr `dotnet ef database update`. |
| **`CryptographicException: The key size is invalid`** | La clave de cifrado guardada en user-secrets no tiene 32 bytes de longitud. | Vuelve a generar la clave con el script de PowerShell del Paso 3, y actualiza el secreto `Cifrado:Clave`. |
| **Visual Studio / Consola se cuelga al compilar** | Archivos basura (`bin`/`obj`) se trajeron del clonado o generaron conflicto con otra versión. | Ejecuta: `Remove-Item -Recurse -Force bin`, luego `Remove-Item -Recurse -Force obj`, y finalmente `dotnet build`. |