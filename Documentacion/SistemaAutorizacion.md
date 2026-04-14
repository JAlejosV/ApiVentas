# Sistema de Autorización Basado en Claims

## 🔄 Mejora Implementada

Se eliminó el código **hardcodeado** del `PermissionRequiredAttribute` y ahora el sistema obtiene los permisos directamente desde las **claims del token JWT** generado durante el login.

## 📋 Cómo Funciona

### 1. **Durante el Login**
```csharp
// En AutenticacionController.cs - Método Login()
var permisos = await _usuarioSistemaRepo.ObtenerPermisosUsuarioAsync(usuario.IdUsuario);

// Se agregan como claims al token JWT
foreach (var permiso in permisos)
{
    claims.Add(new Claim("permiso", $"{permiso.ModuloNombre}.{permiso.Nombre}"));
}
```

### 2. **Durante la Autorización**
```csharp
// En PermissionRequiredAttribute.cs
var userPermissions = context.HttpContext.User
    .FindAll("permiso")
    .Select(c => c.Value)
    .ToList();

var requiredPermission = $"{_module}.{_permission}";
if (!userPermissions.Contains(requiredPermission))
{
    // Acceso denegado
}
```

## 🚀 Ventajas del Nuevo Sistema

### ✅ **Eliminación de Código Hardcodeado**
- **Antes**: Diccionario estático con roles y permisos
- **Ahora**: Permisos dinámicos desde la base de datos

### ✅ **Sincronización Automática**
- Los permisos se obtienen directamente de la base de datos durante el login
- Cualquier cambio en la tabla `RolPermiso` se refleja inmediatamente

### ✅ **Escalabilidad**
- Fácil agregar nuevos módulos y permisos sin tocar código
- Solo se requiere actualizar la base de datos

### ✅ **Mantenibilidad**
- Un solo lugar para gestionar permisos (base de datos)
- Menos propenso a errores de sincronización

## 📝 Formato de Claims de Permisos

Los permisos se almacenan en el token JWT con el formato:
```
Claim Type: "permiso"
Claim Value: "NombreModulo.NombrePermiso"
```

### Ejemplos:
- `"Proveedores.VerListado"`
- `"Proveedores.VerDetalle"`
- `"Proveedores.Crear"`
- `"Proveedores.Editar"`
- `"Productos.VerListado"`
- `"Usuarios.Crear"`

## 🔧 Uso en Controladores

```csharp
[PermissionRequired("Proveedores", "VerListado")]
public async Task<IActionResult> GetProveedores()
{
    // Solo usuarios con permiso "Proveedores.VerListado" pueden acceder
}

[PermissionRequired("Proveedores", "Crear")]
public async Task<IActionResult> CrearProveedor([FromBody] ProveedorCrearDto dto)
{
    // Solo usuarios con permiso "Proveedores.Crear" pueden acceder
}
```

## 🎯 Respuestas de Error Mejoradas

El sistema ahora proporciona información detallada en caso de acceso denegado:

```json
{
    "success": false,
    "message": "Acceso denegado",
    "errors": [
        "No tiene permisos para realizar la acción 'Crear' en el módulo 'Proveedores'",
        "Permiso requerido: Proveedores.Crear",
        "Roles del usuario: [Vendedor]"
    ],
    "timestamp": "2025-11-08T15:30:45Z",
    "path": "/api/v1/proveedores"
}
```

## 📊 Flujo Completo de Autorización

1. **Usuario hace login** → Sistema obtiene permisos de BD
2. **Genera JWT** → Incluye permisos como claims
3. **Usuario hace request** → Envía JWT en header Authorization
4. **Middleware valida JWT** → Extrae claims del token
5. **PermissionRequired ejecuta** → Verifica claim específico
6. **Acceso permitido/denegado** → Basado en permisos reales del usuario

## 🔐 Seguridad

- **No hay código hardcodeado** que pueda quedar desactualizado
- **Permisos en tiempo real** desde la base de datos
- **Token firmado** garantiza integridad de los claims
- **Validación granular** por módulo y acción específica

Este sistema es mucho más robusto, mantenible y seguro que el anterior enfoque con diccionarios hardcodeados.