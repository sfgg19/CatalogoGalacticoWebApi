# USO_DE_IA.md

**Herramientas usadas:** Gemini.

**Qué le pedí en momentos clave:**
- Crear la entidad `Planeta`, relacionarla 1:N con `Eventos` (reemplazando `Ubicacion`) y generar sus DTOs.
- Implementar `PlanetaService` con LINQ para el reporte analítico.
- Solucionar los errores de compilación con `.NET 9.0` y `Microsoft.AspNetCore.OpenApi`.
- Resolver el error de Foreign Key al ejecutar la migración en MySQL.

**Qué acepté:**
- La lógica de los servicios, los mapeos de DTOs y la sintaxis de las consultas LINQ.

**Qué corregí y por qué:**
- **Controladores:** La IA intentó reescribir `PersonajesController`. Me rehusé a cambiarlo y la obligué a corregir las interfaces (`IPersonajeService`) para respetar mi arquitectura y el uso estricto de `ResultadoOperacion`.
- **Archivo .csproj:** Al solucionar dependencias, la IA borró mi `UserSecretsId`. Le reclamé el error y exigí un `.csproj` que mantenga mi conexión segura y la versión correcta de `Pomelo MySQL`.
- **Migraciones en MySQL:** Al fallar la clave foránea `PlanetaId`, la IA sugirió alterar el código C# de la migración. Rechacé la idea y solucioné el problema vaciando los registros de prueba directamente en MySQL usando sentencias `DELETE`, logrando una migración completamente limpia.