# 📁 GUÍA: Almacenamiento Local de Archivos - ProveedoresController

## ✅ **CAMBIO REALIZADO**
He cambiado la configuración de IBM Cloud Storage (que requería tarjeta de crédito) a **almacenamiento local** para que puedas probar inmediatamente sin costos.

---

## 🚀 **CONFIGURACIÓN ACTUAL**

### **📂 Estructura de archivos**
```
📁 wwwroot/
   └── 📁 archivos/
       └── 📁 proveedores/
           ├── 📄 proveedor_1_{guid}.pdf
           ├── 📄 proveedor_1_{guid}.docx
           ├── 📄 proveedor_2_{guid}.xlsx
           └── 📄 ...
```

### **⚙️ Configuración en appsettings.json**
```json
{
  "FileStorage": {
    "Provider": "Local",
    "LocalSettings": {
      "BasePath": "C:\\AppData\\ProveedorArchivos",
      "WebPath": "/archivos",
      "MaxFileSizeMB": 15,
      "AllowedExtensions": [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg", ".gif", ".txt"]
    }
  }
}
```

---

## 🌐 **API EJECUTÁNDOSE**

### **URL Base:**
```
🌐 API: http://localhost:5269
📋 Swagger: http://localhost:5269/swagger
```

### **📋 Endpoints disponibles:**

#### **🔍 Gestión de Proveedores**
```http
GET    /api/v1/proveedores                    # Lista paginada
GET    /api/v1/proveedores/{id}               # Obtener por ID
POST   /api/v1/proveedores                    # Crear proveedor
PATCH  /api/v1/proveedores/{id}               # Actualizar proveedor
PATCH  /api/v1/proveedores/{id}/estado        # Cambiar estado
```

#### **📎 Gestión de Archivos**
```http
GET    /api/v1/proveedores/{id}/con-archivos     # Proveedor + archivos
POST   /api/v1/proveedores/{id}/subir-archivo    # Subir catálogo
DELETE /api/v1/proveedores/archivo/{id}          # Eliminar archivo
GET    /api/v1/proveedores/archivo/{id}/preview  # URL temporal
GET    /api/v1/proveedores/visualizar?token=xxx  # Ver archivo
```

---

## 🧪 **CÓMO PROBAR**

### **1. Abrir Swagger UI**
```
🌐 URL: http://localhost:5269/swagger
```

### **2. Crear un proveedor**
```http
POST /api/v1/proveedores
```
```json
{
  "codigoProveedor": 1001,
  "nombreProveedor": "Proveedor Test",
  "telefono": "123456789",
  "contacto": "contacto@test.com"
}
```

### **3. Subir archivo de catálogo**
```http
POST /api/v1/proveedores/{id}/subir-archivo
```
- Seleccionar archivo (PDF, DOC, Excel, imagen, etc.)
- Máximo 15MB
- Se guarda automáticamente en `wwwroot/archivos/proveedores/`

### **4. Ver proveedor con archivos**
```http
GET /api/v1/proveedores/{id}/con-archivos
```

### **5. Generar URL temporal**
```http
GET /api/v1/proveedores/archivo/{id}/preview
```

### **6. Visualizar archivo**
```http
GET /api/v1/proveedores/visualizar?token={token-temporal}
```

---

## ✅ **FUNCIONALIDADES IMPLEMENTADAS**

### **🔒 Seguridad**
- ✅ **Validación de extensiones**: Solo archivos permitidos
- ✅ **Límite de tamaño**: Máximo 15MB
- ✅ **Tokens temporales**: 30 minutos de validez
- ✅ **Nombres únicos**: `proveedor_{id}_{guid}.ext`
- ✅ **Firmas HMAC-SHA256**: Tokens seguros

### **📊 API Profesional**
- ✅ **Paginación**: `?page=1&pageSize=10&search=term`
- ✅ **Respuestas estandarizadas**: `ApiResponse<T>`
- ✅ **Validaciones exhaustivas**: Parámetros y datos
- ✅ **Manejo de errores**: Códigos HTTP apropiados
- ✅ **Documentación Swagger**: Automática y completa

### **📁 Gestión de Archivos**
- ✅ **Subida de archivos**: Validación completa
- ✅ **Eliminación segura**: BD + archivo físico
- ✅ **Visualización temporal**: Con tokens de acceso
- ✅ **Almacenamiento local**: Sin dependencias externas
- ✅ **Limpieza automática**: En caso de errores

---

## 🎯 **EJEMPLO DE RESPUESTAS**

### **Subir archivo exitoso:**
```json
{
  "success": true,
  "message": "Archivo subido exitosamente al almacenamiento local",
  "data": {
    "mensaje": "Archivo subido exitosamente",
    "idArchivo": 1,
    "nombreArchivo": "catalogo-productos.pdf",
    "rutaArchivo": "archivos/proveedores/proveedor_1_guid.pdf",
    "tamanoArchivo": 2048576,
    "tipoArchivo": "application/pdf"
  },
  "path": "/api/v1/proveedores/1/subir-archivo"
}
```

### **Generar URL temporal:**
```json
{
  "success": true,
  "message": "URL de vista previa generada exitosamente",
  "data": {
    "url": "http://localhost:5269/api/v1/proveedores/visualizar?token=eyJ0eXAi...",
    "token": "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9...",
    "nombreArchivo": "catalogo-productos.pdf",
    "expiracion": "2025-11-08 12:30:00 UTC"
  },
  "path": "/api/v1/proveedores/archivo/1/preview"
}
```

---

## 🔧 **CONFIGURACIÓN DE DESARROLLO**

### **📂 Directorios automáticos**
El sistema crea automáticamente:
```
📁 wwwroot/archivos/proveedores/  # Si no existe
```

### **🌐 Acceso directo a archivos**
Los archivos también son accesibles directamente via HTTP:
```
http://localhost:5269/archivos/proveedores/proveedor_1_guid.pdf
```

### **🗑️ Limpieza automática**
- Si falla guardar en BD → elimina archivo físico
- Al eliminar de BD → elimina archivo físico

---

## 📈 **VENTAJAS DEL ALMACENAMIENTO LOCAL**

### **✅ Beneficios:**
- 🆓 **Sin costos**: No hay cargos por almacenamiento
- ⚡ **Rápido**: Acceso directo al sistema de archivos
- 🔧 **Simple**: No requiere configuración externa
- 🧪 **Ideal para desarrollo**: Pruebas inmediatas
- 🔒 **Control total**: Archivos en tu servidor

### **⚠️ Consideraciones:**
- 📦 **Backup manual**: Respaldar directorio `wwwroot/archivos`
- 💾 **Espacio en disco**: Monitorear uso del servidor
- 🌐 **Escalabilidad**: Para producción considerar nube

---

## 🚀 **PRÓXIMOS PASOS**

### **1. Probar API completa:**
```bash
# API ya ejecutándose en:
http://localhost:5269/swagger
```

### **2. Para migrar a nube después:**
- Cambiar `Provider` a `"IBMCloud"` o `"Azure"`
- Configurar credenciales apropiadas
- El código ya está preparado para ambos

### **3. Personalizar configuración:**
```json
{
  "FileStorage": {
    "LocalSettings": {
      "MaxFileSizeMB": 25,  // Cambiar límite
      "AllowedExtensions": [".pdf", ".xlsx"]  // Restringir tipos
    }
  }
}
```

¡El sistema está completamente funcional y listo para usar con almacenamiento local! 🎯