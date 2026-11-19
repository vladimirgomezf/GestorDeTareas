# Gestor de tareas

API REST de tareas hecha con ASP.NET Core 10, EF Core y SQLite.

## Ejecutar

Se necesita el SDK de .NET 10. Desde la raíz de la solución:

```powershell
dotnet run --project .\GestorDeTareas.Api
```

Con el perfil `http`, la API escucha en `http://localhost:5138`. Al arrancar aplica las migraciones pendientes y, en el repositorio, usa `GestorDeTareas.Data\tasks.db`, independientemente del directorio desde el que se inicie. En una publicación que no incluya la raíz de la solución, usa `tasks.db` junto a la aplicación. La cadena puede cambiarse con `ConnectionStrings__Tasks`, por ejemplo `Data Source=mis-tareas.db`.

En el entorno `Development`, la documentación interactiva está en `http://localhost:5138/swagger` y el documento OpenAPI en `http://localhost:5138/swagger/v1/swagger.json`. Swagger no se habilita en otros entornos.

Para ejecutar las pruebas de integración:

```powershell
dotnet test
```

Las pruebas usan una conexión SQLite en memoria y no dependen de la base persistente de desarrollo.

## API

Todas las rutas están bajo `/api/tasks`. Los cuerpos JSON usan `title`, `description`, `state`, `responsible` y `dueDate` (formato `YYYY-MM-DD`). Los estados admitidos son `pendiente`, `en curso` y `hecha`.

| Método | Ruta | Resultado |
| --- | --- | --- |
| `POST` | `/api/tasks` | Crea una tarea; responde `201 Created` y cabecera `Location`. |
| `GET` | `/api/tasks/{id}` | Devuelve una tarea o `404 Not Found`. |
| `POST` | `/api/tasks/list` | Lista tareas con filtros y paginación recibidos en el cuerpo JSON. |
| `PUT` | `/api/tasks/{id}` | Reemplaza los datos de una tarea; responde `200 OK`. |
| `DELETE` | `/api/tasks/{id}` | Elimina una tarea; responde `204 No Content`. |

El listado recibe un cuerpo JSON con `estado` y `responsable` opcionales, además de `page` (por defecto `1`) y `pageSize` (por defecto `10`, máximo `100`). El filtro de responsable ignora mayúsculas/minúsculas y requiere coincidencia completa. La respuesta contiene `items`, `page`, `pageSize` y `totalCount`. Los errores de validación devuelven `400 Bad Request` con `ValidationProblemDetails`; recursos inexistentes devuelven `404`.

Ejemplo de consulta del listado:

```json
{
  "estado": "pendiente",
  "responsable": "Ana",
  "page": 1,
  "pageSize": 10
}
```

Ejemplo de creación:

```json
{
  "title": "Preparar informe",
  "description": "Revisar las métricas del trimestre",
  "state": "pendiente",
  "responsible": "Ana",
  "dueDate": "2026-12-01"
}
```

Título (máximo 200 caracteres), descripción (2000) y responsable (120) son obligatorios; también lo son un estado reconocido y una fecha límite válida.

## Decisiones

- La solución separa el transporte en `GestorDeTareas.Api`, la lógica de negocio, servicios y DTOs en `GestorDeTareas.Business`, y el modelo persistido, `DbContext`, migraciones y SQLite en `GestorDeTareas.Data`.
- La API compone las dependencias entre proyectos y configura SQLite; Business depende de Data para la persistencia, no al revés.
- EF Core guarda el estado como texto y la fecha como `DateOnly`; una migración inicial crea la tabla e índices de estado y responsable.
- La migración se aplica al arrancar para facilitar el uso local; la conexión se configura con `ConnectionStrings:Tasks`.
- La paginación empieza en 1, ordena por ID para que sea estable y limita el tamaño a 100.
- SQLite en memoria a través de `WebApplicationFactory` permite probar el recorrido HTTP y las migraciones sin compartir datos entre ejecuciones de prueba.

## Qué mejoraría con más tiempo

- Añadir autenticación/autorización y control de concurrencia para varios usuarios.
- Añadir orden configurable y filtros parciales.
- Integrar logging estructurado, health checks y una estrategia de migraciones adecuada para despliegues con varias instancias.
- Aumentar cobertura para cuerpos JSON malformados y límites exactos de cada campo.

## Uso de IA

Se pidió a un asistente de IA implementar una API de tareas en ASP.NET Core con SQLite, migraciones, validación, filtros, paginación, al menos tres pruebas y documentación de ejecución y decisiones. Se aceptaron la estructura de Minimal API, EF Core, DTOs y pruebas de integración con SQLite aislado; durante la revisión se corrigieron los valores predeterminados de la paginación y se evitó convertir DTOs dentro de una consulta traducida por EF. El README se redactó contrastando las rutas, estados, límites y comandos con la implementación, en lugar de dejar las afirmaciones de la plantilla sin revisar.