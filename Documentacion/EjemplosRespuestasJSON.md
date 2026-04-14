# Ejemplos de Respuestas JSON para APIs RESTful en .NET 8 + PostgreSQL

## 📋 Estructura General de Respuestas

Todas las respuestas siguen un formato estándar consistente:

```json
{
  "success": boolean,
  "message": "string",
  "data": object | array | null,
  "errors": ["string"],
  "timestamp": "2025-11-04T10:30:00.000Z",
  "path": "/api/v1/endpoint"
}
```

## 🔍 1. LISTAR PRODUCTOS (GET /api/v1/productos)

### ✅ Respuesta Exitosa con Paginación
```json
{
  "success": true,
  "message": "Se encontraron 150 productos",
  "data": {
    "items": [
      {
        "idProducto": 1,
        "codigoProducto": "PROD001",
        "nombreProducto": "Producto Ejemplo 1",
        "stockReal": 100,
        "stockMinimo": 10,
        "estadoRegistro": true,
        "usuarioCreacion": "admin",
        "fechaHoraCreacion": "2025-11-01T08:30:00.000Z",
        "usuarioActualizacion": null,
        "fechaHoraActualizacion": null
      },
      {
        "idProducto": 2,
        "codigoProducto": "PROD002",
        "nombreProducto": "Producto Ejemplo 2",
        "stockReal": 50,
        "stockMinimo": 5,
        "estadoRegistro": true,
        "usuarioCreacion": "admin",
        "fechaHoraCreacion": "2025-11-01T09:15:00.000Z",
        "usuarioActualizacion": "user1",
        "fechaHoraActualizacion": "2025-11-03T14:20:00.000Z"
      }
    ],
    "pagination": {
      "currentPage": 1,
      "pageSize": 10,
      "totalPages": 15,
      "totalRecords": 150,
      "hasNext": true,
      "hasPrevious": false
    }
  },
  "errors": [],
  "timestamp": "2025-11-04T10:30:00.000Z",
  "path": "/api/v1/productos"
}
```

### ❌ Error de Validación (Parámetros Inválidos)
```json
{
  "success": false,
  "message": "Parámetros inválidos",
  "data": null,
  "errors": ["El tamaño de página debe estar entre 1 y 100"],
  "timestamp": "2025-11-04T10:30:00.000Z",
  "path": "/api/v1/productos"
}
```

## 🔍 2. OBTENER PRODUCTO POR ID (GET /api/v1/productos/{id})

### ✅ Producto Encontrado
```json
{
  "success": true,
  "message": "Producto encontrado exitosamente",
  "data": {
    "idProducto": 1,
    "codigoProducto": "PROD001",
    "nombreProducto": "Producto Ejemplo 1",
    "stockReal": 100,
    "stockMinimo": 10,
    "estadoRegistro": true,
    "usuarioCreacion": "admin",
    "fechaHoraCreacion": "2025-11-01T08:30:00.000Z",
    "usuarioActualizacion": null,
    "fechaHoraActualizacion": null
  },
  "errors": [],
  "timestamp": "2025-11-04T10:30:00.000Z",
  "path": "/api/v1/productos/1"
}
```

### ❌ Producto No Encontrado
```json
{
  "success": false,
  "message": "Recurso no encontrado",
  "data": null,
  "errors": ["No se encontró el producto con ID 999"],
  "timestamp": "2025-11-04T10:30:00.000Z",
  "path": "/api/v1/productos/999"
}
```

## ➕ 3. CREAR PRODUCTO (POST /api/v1/productos)

### ✅ Producto Creado Exitosamente
```json
{
  "success": true,
  "message": "Producto creado exitosamente",
  "data": {
    "idProducto": 151,
    "codigoProducto": "PROD151",
    "nombreProducto": "Nuevo Producto",
    "stockReal": 50,
    "stockMinimo": 5,
    "estadoRegistro": true,
    "usuarioCreacion": "user1",
    "fechaHoraCreacion": "2025-11-04T10:30:00.000Z",
    "usuarioActualizacion": null,
    "fechaHoraActualizacion": null
  },
  "errors": [],
  "timestamp": "2025-11-04T10:30:00.000Z",
  "path": "/api/v1/productos"
}
```

### ❌ Error de Validación
```json
{
  "success": false,
  "message": "Error de validación",
  "data": null,
  "errors": [
    "El código del producto es obligatorio",
    "El nombre del producto es obligatorio",
    "El stock mínimo debe ser mayor a 0"
  ],
  "timestamp": "2025-11-04T10:30:00.000Z",
  "path": "/api/v1/productos"
}
```

### ❌ Conflicto (Producto Ya Existe)
```json
{
  "success": false,
  "message": "Conflicto de datos",
  "data": null,
  "errors": ["Ya existe un producto con el código PROD001"],
  "timestamp": "2025-11-04T10:30:00.000Z",
  "path": "/api/v1/productos"
}
```

## ✏️ 4. ACTUALIZAR PRODUCTO COMPLETO (PUT /api/v1/productos/{id})

### ✅ Producto Actualizado
```json
{
  "success": true,
  "message": "Producto actualizado exitosamente",
  "data": {
    "idProducto": 1,
    "codigoProducto": "PROD001",
    "nombreProducto": "Producto Actualizado",
    "stockReal": 120,
    "stockMinimo": 15,
    "estadoRegistro": true,
    "usuarioCreacion": "admin",
    "fechaHoraCreacion": "2025-11-01T08:30:00.000Z",
    "usuarioActualizacion": "user1",
    "fechaHoraActualizacion": "2025-11-04T10:30:00.000Z"
  },
  "errors": [],
  "timestamp": "2025-11-04T10:30:00.000Z",
  "path": "/api/v1/productos/1"
}
```

## ✏️ 5. ACTUALIZACIÓN PARCIAL (PATCH /api/v1/productos/{id})

### ✅ Campos Específicos Actualizados
```json
{
  "success": true,
  "message": "Producto actualizado exitosamente",
  "data": {
    "idProducto": 1,
    "codigoProducto": "PROD001",
    "nombreProducto": "Producto Parcialmente Actualizado",
    "stockReal": 80,
    "stockMinimo": 10,
    "estadoRegistro": true,
    "usuarioCreacion": "admin",
    "fechaHoraCreacion": "2025-11-01T08:30:00.000Z",
    "usuarioActualizacion": "user1",
    "fechaHoraActualizacion": "2025-11-04T10:30:00.000Z"
  },
  "errors": [],
  "timestamp": "2025-11-04T10:30:00.000Z",
  "path": "/api/v1/productos/1"
}
```

## 🔄 6. CAMBIAR ESTADO (PATCH /api/v1/productos/{id}/estado)

### ✅ Estado Cambiado (Habilitar/Deshabilitar)
```json
{
  "success": true,
  "message": "Producto deshabilitado exitosamente",
  "data": {
    "idProducto": 1,
    "codigoProducto": "PROD001",
    "nombreProducto": "Producto Ejemplo",
    "stockReal": 80,
    "stockMinimo": 10,
    "estadoRegistro": false,
    "usuarioCreacion": "admin",
    "fechaHoraCreacion": "2025-11-01T08:30:00.000Z",
    "usuarioActualizacion": "user1",
    "fechaHoraActualizacion": "2025-11-04T10:30:00.000Z"
  },
  "errors": [],
  "timestamp": "2025-11-04T10:30:00.000Z",
  "path": "/api/v1/productos/1/estado"
}
```

## 🗑️ 7. ELIMINAR PRODUCTO (DELETE /api/v1/productos/{id})

### ✅ Producto Eliminado
```json
{
  "success": true,
  "message": "Producto eliminado exitosamente",
  "data": null,
  "errors": [],
  "timestamp": "2025-11-04T10:30:00.000Z",
  "path": "/api/v1/productos/1"
}
```

## 🔐 8. AUTENTICACIÓN - LOGIN (POST /api/v1/auth/login)

### ✅ Login Exitoso
```json
{
  "success": true,
  "message": "Autenticación exitosa",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "tokenType": "Bearer",
    "expiresIn": 3600,
    "expiresAt": "2025-11-04T11:30:00.000Z",
    "user": {
      "id": 1,
      "username": "usuario123",
      "email": "usuario@example.com",
      "name": "Juan Pérez",
      "roles": ["User"]
    }
  },
  "errors": [],
  "timestamp": "2025-11-04T10:30:00.000Z",
  "path": "/api/v1/auth/login"
}
```

### ❌ Credenciales Inválidas
```json
{
  "success": false,
  "message": "Credenciales inválidas",
  "data": null,
  "errors": ["Usuario o contraseña incorrectos"],
  "timestamp": "2025-11-04T10:30:00.000Z",
  "path": "/api/v1/auth/login"
}
```

## 👤 9. REGISTRO DE USUARIO (POST /api/v1/auth/register)

### ✅ Usuario Registrado
```json
{
  "success": true,
  "message": "Usuario registrado exitosamente",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "tokenType": "Bearer",
    "expiresIn": 3600,
    "expiresAt": "2025-11-04T11:30:00.000Z",
    "user": {
      "id": 2,
      "username": "nuevousuario",
      "email": "nuevo@example.com",
      "name": "Nuevo Usuario",
      "roles": ["User"]
    }
  },
  "errors": [],
  "timestamp": "2025-11-04T10:30:00.000Z",
  "path": "/api/v1/auth/register"
}
```

### ❌ Usuario Ya Existe
```json
{
  "success": false,
  "message": "Usuario ya existe",
  "data": null,
  "errors": ["Ya existe un usuario con el nombre usuario123"],
  "timestamp": "2025-11-04T10:30:00.000Z",
  "path": "/api/v1/auth/register"
}
```

## 👤 10. PERFIL DE USUARIO (GET /api/v1/auth/profile)

### ✅ Perfil Obtenido (Con Token Bearer)
```json
{
  "success": true,
  "message": "Perfil obtenido exitosamente",
  "data": {
    "id": 1,
    "username": "usuario123",
    "email": "usuario@example.com",
    "name": "Juan Pérez",
    "roles": ["User", "Admin"]
  },
  "errors": [],
  "timestamp": "2025-11-04T10:30:00.000Z",
  "path": "/api/v1/auth/profile"
}
```

### ❌ Token Inválido/Expirado
```json
{
  "success": false,
  "message": "Acceso no autorizado",
  "data": null,
  "errors": ["Token inválido o expirado"],
  "timestamp": "2025-11-04T10:30:00.000Z",
  "path": "/api/v1/auth/profile"
}
```

## 🔓 11. LOGOUT (POST /api/v1/auth/logout)

### ✅ Logout Exitoso
```json
{
  "success": true,
  "message": "Sesión cerrada exitosamente",
  "data": null,
  "errors": [],
  "timestamp": "2025-11-04T10:30:00.000Z",
  "path": "/api/v1/auth/logout"
}
```

## ✅ 12. VALIDAR TOKEN (GET /api/v1/auth/validate-token)

### ✅ Token Válido
```json
{
  "success": true,
  "message": "Token válido",
  "data": {
    "isValid": true,
    "userId": 1,
    "username": "usuario123",
    "expiresAt": "2025-11-04T11:30:00.000Z"
  },
  "errors": [],
  "timestamp": "2025-11-04T10:30:00.000Z",
  "path": "/api/v1/auth/validate-token"
}
```

## 🔧 13. ERRORES GLOBALES

### ❌ Error 500 - Error Interno
```json
{
  "success": false,
  "message": "Error interno del servidor",
  "data": null,
  "errors": ["Ha ocurrido un error inesperado. Por favor, contacte al administrador."],
  "timestamp": "2025-11-04T10:30:00.000Z",
  "path": "/api/v1/productos"
}
```

### ❌ Error 401 - No Autorizado
```json
{
  "success": false,
  "message": "Acceso no autorizado",
  "data": null,
  "errors": ["Token inválido o expirado"],
  "timestamp": "2025-11-04T10:30:00.000Z",
  "path": "/api/v1/productos"
}
```

### ❌ Error 403 - Prohibido
```json
{
  "success": false,
  "message": "Acceso prohibido",
  "data": null,
  "errors": ["No tiene permisos para realizar esta operación"],
  "timestamp": "2025-11-04T10:30:00.000Z",
  "path": "/api/v1/productos"
}
```

## 📝 Headers para Autenticación

Para endpoints que requieren autenticación, incluir:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json
Accept: application/json
```

## 🏆 Mejores Prácticas Implementadas

1. **Consistencia**: Todas las respuestas siguen el mismo formato
2. **Códigos HTTP Apropiados**: 200, 201, 400, 401, 403, 404, 409, 500
3. **Mensajes Descriptivos**: Claros y útiles para el frontend
4. **Paginación Estándar**: Con metadata completa
5. **Manejo de Errores**: Detallado y consistente
6. **Seguridad**: JWT Bearer tokens con expiración
7. **Versionado**: URLs con versión (/api/v1/)
8. **Documentación**: Headers ProducesResponseType para Swagger
9. **Validación**: Comprehensive input validation
10. **Timestamps**: UTC para todas las fechas

## 🚀 Uso del Bearer Token

```bash
# Ejemplo de petición con curl
curl -X GET "https://api.example.com/api/v1/productos" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json"
```