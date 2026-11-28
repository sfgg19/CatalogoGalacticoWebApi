# Informe EC4 — CatalogoGalacticoApi

**Proyecto:** `CatalogoGalacticoApi` (.NET 9.0, ASP.NET Core, Entity Framework y MySQL)

---

## Parte 1 · Riesgos de seguridad encontrados y correcciones

Al revisar mi API original antes de los cambios, identifiqué tres riesgos reales:

1. **Controladores sin autenticación ni roles:**  
   Ningún controlador tenía `[Authorize]` y en `Program.cs` faltaba configurar `AddAuthentication()` y `UseAuthentication()`. Cualquier persona podía crear, editar o borrar personajes y planetas sin iniciar sesión.  
   *Qué cambié:* Configuré autenticación con JWT en `Program.cs` y agregué `[Authorize]` a los controladores, dejando las rutas de creación, edición y borrado solo para el rol `Administrador` (`[Authorize(Roles = "Administrador")]`).

2. **Exposición de coordenadas y acceso a datos de otros usuarios:**  
   Aunque `CoordenadasOrigen` se guardaba cifrado en la base de datos, los servicios lo descifraban en todos los `GET` públicos y cualquiera podía leer o modificar personajes o cartas ajenas.  
   *Qué cambié:* Dejé `CoordenadasOrigen` en `null` en los listados generales y agregué validación de dueño en `PersonajesController` y `CartasController`: ahora solo el propio usuario (comparando el `sub` del token con el ID) o un `Administrador` pueden consultar o editar ese recurso; si no, la API responde `403 Forbidden`.

3. **Sin límite de intentos en el login, sin CORS ni manejo global de errores:**  
   No había control contra intentos repetidos de login, no existía política CORS ni un manejador global de excepciones.  
   *Qué cambié:* Agregué Rate Limiting (`5` intentos por minuto en `/api/auth/login` y `60` global), configuré CORS leyendo los orígenes permitidos desde `appsettings.json` y agregué `ManejadorGlobalExcepciones` para devolver `ProblemDetails` sin mostrar el detalle interno del error.

---

## Parte 2 · Pruebas de autenticación y autorización

Probé los endpoints localmente sin token, con un token de usuario común (`id = 4`, rol `Usuario`) y con el token de `Administrador`:

| Endpoint | Sin token | Token Usuario (`id = 4`) | Token Administrador |
| :--- | :---: | :---: | :---: |
| `GET /api/personajes` | `401 Unauthorized` | `200 OK` *(sin coordenadas)* | `200 OK` |
| `GET /api/personajes/1` *(ajeno)* | `401 Unauthorized` | `403 Forbidden` | `200 OK` |
| `GET /api/personajes/4` *(propio)* | `401 Unauthorized` | `200 OK` *(con coordenadas)* | `200 OK` |
| `POST /api/planetas` *(solo admin)* | `401 Unauthorized` | `403 Forbidden` | `201 Created` |

---

## Parte 3 · Fragmento de log comentado

Fragmento extraído de `logs/catalogo-20261127.log` al hacer login con el usuario `4` y luego intentar crear un planeta sin permiso (`403`):

```text
2026-11-27 23:19:50.867 INF cid=149cfca6 ip=::1 ua=[Mozilla/5.0 (Windows NT; Windows NT 10.0; es-BO) WindowsPowerShell/5.1.26100.9818] huella=12e203453236 Login correcto para el personaje 4
2026-11-27 23:19:50.869 INF cid=149cfca6 ip=::1 ua=[Mozilla/5.0 (Windows NT; Windows NT 10.0; es-BO) WindowsPowerShell/5.1.26100.9818] huella=12e203453236 POST /api/auth/login respondió 200 en 77 ms (usuario anonimo)
2026-11-27 23:19:50.956 WRN cid=5ed59da6 ip=::1 ua=[curl/8.22.0] huella=14a01dc4e9d6 Acceso denegado (403) al recurso POST /api/planetas en 8 ms (usuario 4)
```
