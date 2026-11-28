Informe EC4 - CatalogoGalacticoApi

Parte 1
1. Rutas sin login -> puse [Authorize] y rol Administrador.
2. Coordenadas visibles y acceso a datos ajenos -> null en listados y validacion de dueno/admin.
3. Sin limite ni CORS ni errores -> RateLimit (5/min login), CORS y ProblemDetails.

Parte 2
GET /api/personajes/1: sin token 401 | usuario 403 | admin 200
GET /api/personajes/4: usuario 200
POST /api/planetas: usuario 403 | admin 201

Parte 3
2026-11-27 23:19:50.956 WRN cid=5ed59da6 ip=::1 ua=[curl/8.22.0] huella=14a01dc4e9d6 Acceso denegado (403) al recurso POST /api/planetas en 8 ms (usuario 4)
Muestra fecha, nivel WRN, cid (id peticion), ip, ua, huella, ruta, 403, tiempo y usuario 4.

Parte 4
Docker Hub: pendiente
Render: pendiente
