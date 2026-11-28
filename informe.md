# Informe técnico — CatalogoGalacticoApi (EC 4)

**Proyecto:** `CatalogoGalacticoApi` (.NET 9.0 / ASP.NET Core / MySQL)  
**Alcance cubierto:** Partes 1, 2 y 3 (Seguridad, Autenticación/Autorización y Logs/Errores)

---

## Parte 1 · Riesgos de seguridad identificados y correcciones

Al revisar el proyecto antes de aplicar los cambios, se encontraron tres problemas reales en el código:

1. **Rutas abiertas sin autenticación ni control de roles:**  
   Ningún controlador (`PersonajesController`, `CartasController`, `EventosController`, `PlanetasController`, `ReportesController`) tenía `[Authorize]`, y `Program.cs` llamaba a `app.UseAuthorization()` sin haber configurado `AddAuthentication()` ni `UseAuthentication()`. En la práctica, cualquier persona sin identificarse podía borrar personajes (`DELETE /api/personajes/{id}`), borrar planetas (`DELETE /api/planetas/{id}`), sobrescribir contraseñas (`PUT /api/personajes/{id}`) o simular batallas.  
   *Corrección:* Se instaló `Microsoft.AspNetCore.Authentication.JwtBearer` (`9.0.0`), se configuró la validación de JWT en `Program.cs` y se protegió cada controlador con `[Authorize]`, reservando las rutas de creación, edición, eliminación y simulación crítica con `[Authorize(Roles = "Administrador")]`.

2. **Exposición de datos sensibles descifrados y acceso a objetos ajenos (BOLA/IDOR):**  
   Aunque `CoordenadasOrigen` se guardaba cifrado con AES-GCM en MySQL, `PersonajeService` y `EventoService` lo descifraban en todos los `GET` (`GET /api/personajes`, `GET /api/personajes/ranking`, `GET /api/eventos`, etc.), entregando el dato en claro a cualquiera. Tampoco había validación para impedir que un usuario leyera o modificara el personaje o la carta de otro.  
   *Corrección:* Se dejó `CoordenadasOrigen` en `null` en los listados públicos y se validó la propiedad en `PersonajesController` (`GET /api/personajes/{id}`, `GET /api/personajes/{id}/eventos`, `PUT /api/personajes/{id}`) y `CartasController` (`POST /api/cartas`, `PUT /api/cartas/{id}`): si el `sub` del token no coincide con el dueño y el rol no es `Administrador`, la API devuelve `403 Forbidden`.

3. **Sin límite de intentos, sin política CORS ni manejo global de errores:**  
   `Program.cs` no tenía `AddRateLimiter()`, `AddCors()` ni un manejador global de excepciones, lo que dejaba abierta la puerta a ataques de fuerza bruta, orígenes no restringidos y respuestas inconsistentes ante fallos inesperados.  
   *Corrección:* Se configuró Rate Limiting por IP (`60` peticiones/min globales y `5` peticiones/min en `login` devolviendo `429`), CORS leyendo `Cors:OrigenesPermitidos` desde `appsettings.json` y el manejador global `ManejadorGlobalExcepciones` con `ProblemDetails`.

---

## Parte 2 · Comprobación de autenticación y autorización

Las pruebas se ejecutaron contra la API levantada localmente usando un personaje administrador (seed desde `user-secrets`) y un personaje común (`id = 4`, `Rol = "Usuario"`):

| Endpoint probado | Sin token | Token usuario común (`Usuario`) | Token administrador (`Administrador`) |
| :--- | :---: | :---: | :---: |
| `GET /api/personajes` | `401 Unauthorized` | `200 OK` *(coordenadas ocultas)* | `200 OK` |
| `GET /api/personajes/1` *(recurso ajeno)* | `401 Unauthorized` | `403 Forbidden` | `200 OK` |
| `GET /api/personajes/4` *(recurso propio)* | `401 Unauthorized` | `200 OK` *(con coordenadas)* | `200 OK` |
| `POST /api/planetas` *(solo admin)* | `401 Unauthorized` | `403 Forbidden` | `201 Created` |

---

## Parte 3 · Análisis de fragmento de log

Fragmento real extraído de `logs/catalogo-20261127.log` durante la secuencia de login con el usuario común (`id = 4`) y un intento bloqueado con `403`:

```text
2026-11-27 23:19:50.867 INF cid=149cfca6 ip=::1 ua=[Mozilla/5.0 (Windows NT; Windows NT 10.0; es-BO) WindowsPowerShell/5.1.26100.9818] huella=12e203453236 Login correcto para el personaje 4
2026-11-27 23:19:50.869 INF cid=149cfca6 ip=::1 ua=[Mozilla/5.0 (Windows NT; Windows NT 10.0; es-BO) WindowsPowerShell/5.1.26100.9818] huella=12e203453236 POST /api/auth/login respondió 200 en 77 ms (usuario anonimo)
2026-11-27 23:19:50.956 WRN cid=5ed59da6 ip=::1 ua=[curl/8.22.0] huella=14a01dc4e9d6 Acceso denegado (403) al recurso POST /api/planetas en 8 ms (usuario 4)
```

**Qué muestra cada campo:**
- **`2026-11-27 23:19:50.867` (Timestamp):** Fecha y hora exacta con milisegundos en la que se escribió el evento.
- **`INF` / `WRN` (Nivel):** `INF` indica operación normal (login y petición atendida) y `WRN` marca un rechazo controlado (`403` por falta de permisos).
- **`cid=149cfca6` (Correlation ID):** Identificador de 8 caracteres generado en el servidor por `ContextoPeticionMiddleware`; permite unir las dos líneas que pertenecen a la misma petición de login (el mensaje interno de `AuthService` y el resumen final del middleware).
- **`ip=::1` (IP del cliente):** Dirección IPv6 local desde donde se hizo la petición.
- **`ua=[...]` (User-Agent):** Cliente que realizó la llamada (`WindowsPowerShell` en el login y `curl/8.22.0` en la prueba del `403`), truncado a un máximo de 200 caracteres.
- **`huella=12e203453236` (Huella):** Primeros 12 caracteres hexadecimales del hash SHA-256 calculado sobre `ip|ua` para agrupar peticiones del mismo cliente sin depender de sesiones.
- **`POST /api/auth/login` / `POST /api/planetas`:** Método HTTP y ruta del recurso solicitado.
- **`respondió 200` / `Acceso denegado (403)`:** Código de estado HTTP devuelto por la API.
- **`en 77 ms` / `en 8 ms`:** Tiempo total de ejecución medido con `Stopwatch`.
- **`(usuario anonimo)` / `(usuario 4)`:** Valor del claim `sub` del JWT; en el login aún no hay token (`anonimo`), mientras que en `POST /api/planetas` ya identifica al personaje `4`.

---

## Parte 4 · Despliegue (Pendiente de completar)

- **Imagen pública en Docker Hub:** *(pendiente)*
- **URL del servicio en Render:** *(pendiente)*
- **Verificación de `/health`:** *(pendiente)*
