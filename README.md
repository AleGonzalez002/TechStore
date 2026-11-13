# TechStore - Examen práctico 3

Aplicación ASP.NET Core MVC para catálogo de productos, roles y reportes.

## Inicio

1. Instala SQL Server LocalDB o actualiza `ConnectionStrings:DefaultConnection` en `appsettings.json`.
2. Desde `TechStore/`, ejecuta `dotnet restore` y `dotnet run`.
3. Al iniciar, la aplicación aplica las migraciones pendientes (solo con SQL Server), crea los roles Administrador, Vendedor y Cliente y registra el administrador inicial cuando `AdminSeed:Email` y `AdminSeed:Password` están configurados (en `appsettings.Development.json` por defecto). Configura `AdminSeed__Email` y `AdminSeed__Password` como variables de entorno para establecer credenciales propias. En desarrollo se incluye `admin@techstore.local` / `TechStoreAdmin123!` para facilitar la evaluación.
4. Los registros públicos crean únicamente usuarios con rol Cliente. El administrador cambia roles desde **Usuarios**.

### SQL Server en Docker (sin LocalDB)

Si no tienes LocalDB, levanta SQL Server en Docker y guarda la cadena de conexión en los secretos de usuario (así la contraseña no queda en el repositorio):

1. En Visual Studio: clic derecho sobre el proyecto `TechStore` → **Administrar secretos de usuario**.
2. Pega en `secrets.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=TechStoreDb;User Id=sa;Password=TU_CONTRASEÑA;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

3. Ejecuta la aplicación: aplica las migraciones y crea los roles y el administrador de desarrollo.

## Permisos

- Administrador: catálogo, edición, eliminación, reportes y asignación de roles.
- Vendedor: consulta, alta y edición de productos, y reportes.
- Cliente: consulta de productos disponibles.

## Reportes

Desde **Reportes**, descarga PDF (QuestPDF) y Excel (ClosedXML). Ambos archivos usan datos actuales de la base y llevan nombre con fecha y hora.

## Logging y errores

Los registros de ASP.NET Core aparecen en la consola y en `TechStore/logs/techstore-AAAAmmdd.log`. Las excepciones se registran y conducen a la vista de error; los rechazos por rol muestran **Acceso denegado**.

## Pruebas

Desde la raíz ejecuta `dotnet test TechStore.Tests/TechStore.Tests.csproj`. Las 15 pruebas unitarias, de integración y E2E cubren consultas por lista e ID, alta, edición y eliminación; el flujo HTTP de registro/login, menús y rutas por rol; asignación de roles; y descarga de los dos reportes. Las pruebas usan xUnit, Moq, EF Core InMemory y el host HTTP de ASP.NET Core.

## Recorrido E2E y capturas para la entrega

Ejecuta la aplicación con SQL Server disponible y captura cada pantalla después de completar el paso:

1. Inicia sesión con el administrador de desarrollo. Captura Login y menú de Administrador.
2. En Usuarios, cambia una cuenta a Vendedor y otra a Cliente; captura la tabla de roles.
3. Entra con Cliente: captura catálogo y comprueba que no aparecen Administración, Reportes ni Usuarios.
4. Intenta abrir `/Productos` o `/Reports` como Cliente: captura Acceso denegado.
5. Entra como Vendedor: captura menú y alta/edición de producto. Comprueba que `/Productos/Delete/{id}` devuelve Acceso denegado.
6. Genera el PDF y abre el `.xlsx`; captura cada archivo.
7. En la consola y `TechStore/logs/`, captura el evento de inicio de sesión y generación de reporte.
8. Captura la tarjeta parcial en Inicio y el resumen del catálogo del ViewComponent.
9. En la terminal, ejecuta `dotnet test`; captura resultados unitarios y de integración.
10. Captura la página personalizada de Error. Para comprobar el middleware en pantalla, fuerza temporalmente una excepción en Development y revierte ese cambio después de la captura.

Incluye las imágenes en el PDF de entrega indicado por la consigna. Esta guía no simula capturas: las evidencias deben obtenerse durante la ejecución local con su base de datos.
