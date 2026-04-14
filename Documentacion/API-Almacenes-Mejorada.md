# 📋 API Almacenes - Documentación de Endpoints Mejorados

## 🚀 **Mejoras Implementadas**

### ✅ **Características Profesionales**
- **Paginación completa** con metadatos
- **Respuestas estandarizadas** usando `ApiResponse<T>`
- **Manejo robusto de errores** con mensajes descriptivos
- **Validaciones exhaustivas** de entrada
- **Filtros de búsqueda** y estado
- **Documentación Swagger** completa
- **Códigos HTTP apropiados** para cada escenario
- **Versionado de API** (v1)

---

## 📚 **Endpoints Disponibles**

### 1. **GET /api/v1/almacenes** - Obtener Almacenes (Paginado)

**Descripción:** Obtiene una lista paginada de almacenes con filtros opcionales.

**Parámetros de consulta:**
- `page` (int, opcional): Número de página (por defecto: 1)
- `pageSize` (int, opcional): Elementos por página (por defecto: 10, máximo: 100)
- `search` (string, opcional): Término de búsqueda (busca en nombre y código)
- `includeInactive` (bool, opcional): Incluir almacenes inactivos (por defecto: false)

**Ejemplo de solicitud:**
```
GET /api/v1/almacenes?page=1&pageSize=5&search=principal&includeInactive=false
```

**Respuesta exitosa (200):**
```json
{
  "success": true,
  "message": "Almacenes obtenidos exitosamente",
  "data": {
    "items": [
      {
        "idAlmacen": 1,
        "codigoAlmacen": 3,
        "nombreAlmacen": "Almacen 3",
        "estadoRegistro": true
      }
    ],
    "pagination": {
      "currentPage": 1,
      "pageSize": 5,
      "totalPages": 1,
      "totalRecords": 3,
      "hasNext": false,
      "hasPrevious": false
    }
  },
  "errors": null,
  "timestamp": "2025-11-08T01:27:05.3351867Z",
  "path": "/api/v1/almacenes"
}
```

**Errores posibles:**
- `400 Bad Request`: Parámetros inválidos
- `500 Internal Server Error`: Error interno

---

### 2. **GET /api/v1/almacenes/{id}** - Obtener Almacén por ID

**Descripción:** Obtiene un almacén específico por su ID.

**Ejemplo de solicitud:**
```
GET /api/v1/almacenes/1
```

**Respuesta exitosa (200):**
```json
{
  "success": true,
  "message": "Almacén obtenido exitosamente",
  "data": {
    "idAlmacen": 1,
    "codigoAlmacen": 3,
    "nombreAlmacen": "Almacen 3",
    "estadoRegistro": true
  },
  "errors": null,
  "timestamp": "2025-11-08T01:27:05.3351867Z",
  "path": "/api/v1/almacenes/1"
}
```

**Errores posibles:**
- `400 Bad Request`: ID inválido
- `404 Not Found`: Almacén no encontrado
- `500 Internal Server Error`: Error interno

---

### 3. **POST /api/v1/almacenes** - Crear Almacén

**Descripción:** Crea un nuevo almacén.

**Autenticación:** Requerida (Bearer Token)

**Cuerpo de solicitud:**
```json
{
  "codigoAlmacen": 101,
  "nombreAlmacen": "Almacén Central"
}
```

**Respuesta exitosa (201):**
```json
{
  "success": true,
  "message": "Almacén creado exitosamente",
  "data": {
    "idAlmacen": 4,
    "codigoAlmacen": 101,
    "nombreAlmacen": "Almacén Central",
    "estadoRegistro": true
  },
  "errors": null,
  "timestamp": "2025-11-08T01:27:05.3351867Z",
  "path": "/api/v1/almacenes"
}
```

**Errores posibles:**
- `400 Bad Request`: Datos inválidos o faltantes
- `401 Unauthorized`: Token inválido o faltante
- `409 Conflict`: Ya existe un almacén con ese nombre o código
- `500 Internal Server Error`: Error interno

---

### 4. **PATCH /api/v1/almacenes/{id}** - Actualizar Almacén

**Descripción:** Actualiza un almacén existente.

**Autenticación:** Requerida (Bearer Token)

**Cuerpo de solicitud:**
```json
{
  "idAlmacen": 1,
  "codigoAlmacen": 3,
  "nombreAlmacen": "Almacén Principal Actualizado",
  "estadoRegistro": true
}
```

**Respuesta exitosa (200):**
```json
{
  "success": true,
  "message": "Almacén actualizado exitosamente",
  "data": {
    "idAlmacen": 1,
    "codigoAlmacen": 3,
    "nombreAlmacen": "Almacén Principal Actualizado",
    "estadoRegistro": true
  },
  "errors": null,
  "timestamp": "2025-11-08T01:27:05.3351867Z",
  "path": "/api/v1/almacenes/1"
}
```

**Errores posibles:**
- `400 Bad Request`: Datos inválidos o ID inconsistente
- `401 Unauthorized`: Token inválido o faltante
- `404 Not Found`: Almacén no encontrado
- `409 Conflict`: Conflicto con nombre o código existente
- `500 Internal Server Error`: Error interno

---

### 5. **PATCH /api/v1/almacenes/{id}/estado** - Cambiar Estado

**Descripción:** Activa o desactiva un almacén.

**Autenticación:** Requerida (Bearer Token)

**Parámetros:**
- `activar` (bool, query): true para activar, false para desactivar

**Ejemplo de solicitud:**
```
PATCH /api/v1/almacenes/1/estado?activar=false
```

**Respuesta exitosa (200):**
```json
{
  "success": true,
  "message": "Almacén desactivado exitosamente",
  "data": {
    "idAlmacen": 1,
    "codigoAlmacen": 3,
    "nombreAlmacen": "Almacén Principal",
    "estadoRegistro": false
  },
  "errors": null,
  "timestamp": "2025-11-08T01:27:05.3351867Z",
  "path": "/api/v1/almacenes/1/estado"
}
```

---

## 🛠 **Características Técnicas**

### **Estructura de Respuesta Estandarizada**
```json
{
  "success": boolean,
  "message": "string",
  "data": object | null,
  "errors": string[] | null,
  "timestamp": "ISO 8601 date",
  "path": "string"
}
```

### **Paginación**
```json
{
  "items": [],
  "pagination": {
    "currentPage": 1,
    "pageSize": 10,
    "totalPages": 5,
    "totalRecords": 50,
    "hasNext": true,
    "hasPrevious": false
  }
}
```

### **Validaciones**
- ✅ IDs deben ser mayores a 0
- ✅ PageSize entre 1 y 100
- ✅ Page debe ser mayor a 0
- ✅ Nombres y códigos únicos
- ✅ Datos requeridos presentes

### **Códigos de Estado HTTP**
- `200 OK`: Operación exitosa
- `201 Created`: Recurso creado
- `400 Bad Request`: Datos inválidos
- `401 Unauthorized`: Autenticación requerida
- `404 Not Found`: Recurso no encontrado
- `409 Conflict`: Conflicto de datos
- `500 Internal Server Error`: Error interno

---

## 🧪 **Ejemplos de Prueba**

### **Prueba de Paginación**
```bash
curl -X GET "http://localhost:5269/api/v1/almacenes?page=1&pageSize=5&search=almacen&includeInactive=false" \
  -H "accept: */*"
```

### **Prueba de Creación**
```bash
curl -X POST "http://localhost:5269/api/v1/almacenes" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "codigoAlmacen": 201,
    "nombreAlmacen": "Almacén Nuevo"
  }'
```

### **Prueba de Actualización**
```bash
curl -X PATCH "http://localhost:5269/api/v1/almacenes/1" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "idAlmacen": 1,
    "codigoAlmacen": 3,
    "nombreAlmacen": "Almacén Actualizado",
    "estadoRegistro": true
  }'
```

---

## ⚡ **Mejoras vs Versión Anterior**

| Aspecto | Antes | Ahora |
|---------|-------|-------|
| **Respuesta** | Array simple | ApiResponse con metadatos |
| **Paginación** | ❌ No | ✅ Completa con metadatos |
| **Filtros** | ❌ No | ✅ Búsqueda y estado |
| **Errores** | Básicos | Descriptivos y estructurados |
| **Validaciones** | Mínimas | Exhaustivas |
| **Documentación** | Limitada | Swagger completo |
| **Versionado** | ❌ No | ✅ /api/v1/ |
| **Estado HTTP** | Básicos | Apropiados por escenario |

La API de almacenes ahora es **completamente profesional** y lista para producción! 🚀