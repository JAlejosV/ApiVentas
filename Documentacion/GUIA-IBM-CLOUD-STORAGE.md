# 🚀 GUÍA COMPLETA: IBM Cloud Object Storage - Plan Gratuito

## ✅ **ESTADO DEL PROYECTO**
- ✅ Errores de compilación corregidos
- ✅ Paquete AWSSDK.S3 instalado correctamente
- ✅ Servicio IBMCloudStorageService configurado
- ✅ ProveedoresController actualizado con mejores prácticas

---

## 🆓 **PASO 1: Crear cuenta IBM Cloud GRATUITA**

### **1.1 Registro**
```
🌐 URL: https://cloud.ibm.com/registration
📧 Registrarse con email (NO requiere tarjeta de crédito)
✅ Verificar email y acceder al dashboard
```

### **1.2 Plan Lite (Gratuito)**
```
💾 Almacenamiento: 25GB gratis
📥 Operaciones GET: 2,000/mes gratis  
📤 Operaciones PUT: 200/mes gratis
🔒 Sin límite de tiempo (no expira)
```

---

## ☁️ **PASO 2: Crear Cloud Object Storage**

### **2.1 Crear servicio**
1. En el dashboard, clic **"Create resource"**
2. Buscar **"Object Storage"**
3. Seleccionar **"Cloud Object Storage"**
4. Configurar:
   - **Plan**: **Lite (Free)**
   - **Service name**: `sistema-ventas-storage`
   - **Resource group**: Default
5. Clic **"Create"**

### **2.2 Crear bucket**
1. En tu servicio, clic **"Create bucket"**
2. Seleccionar **"Quickly get started"**
3. Configurar:
   - **Bucket name**: `sistema-ventas-proveedores`
   - **Resiliency**: **Regional**
   - **Location**: `us-south` (o región más cercana)
   - **Storage class**: **Standard**
4. Clic **"Create bucket"**

---

## 🔑 **PASO 3: Generar credenciales**

### **3.1 Crear Service Credentials**
1. Menú lateral → **"Service credentials"**
2. Clic **"New credential"**
3. Configurar:
   - **Name**: `sistema-ventas-api-key`
   - **Role**: **Writer**
   - **Advanced options** → **Include HMAC Credential**: ✅
4. Clic **"Add"**

### **3.2 Obtener credenciales**
1. Clic **"View credentials"** 
2. Copiar y guardar datos importantes:

```json
{
  "cos_hmac_keys": {
    "access_key_id": "COPIA_ESTE_VALOR",
    "secret_access_key": "COPIA_ESTE_VALOR"
  },
  "endpoints": "https://control.cloud-object-storage.cloud.ibm.com/v2/endpoints"
}
```

### **3.3 Obtener endpoint regional**
1. Ir a: https://control.cloud-object-storage.cloud.ibm.com/v2/endpoints
2. Buscar tu región (ej: `us-south`)
3. Copiar endpoint **public**:
   - Ejemplo: `https://s3.us-south.cloud-object-storage.appdomain.cloud`

---

## ⚙️ **PASO 4: Configurar aplicación**

### **4.1 Actualizar appsettings.json**
```json
{
  "IBMCloudStorage": {
    "ServiceUrl": "https://s3.us-south.cloud-object-storage.appdomain.cloud",
    "AccessKey": "TU_ACCESS_KEY_ID_AQUI",
    "SecretKey": "TU_SECRET_ACCESS_KEY_AQUI",
    "BucketName": "sistema-ventas-proveedores"
  }
}
```

### **4.2 Configuración de seguridad (Recomendado)**
En lugar de poner credenciales en appsettings.json, usar variables de entorno:

**Windows PowerShell:**
```powershell
$env:IBM_ACCESS_KEY = "tu-access-key-id"
$env:IBM_SECRET_KEY = "tu-secret-access-key"
$env:IBM_BUCKET_NAME = "sistema-ventas-proveedores"
$env:IBM_SERVICE_URL = "https://s3.us-south.cloud-object-storage.appdomain.cloud"
```

**Linux/MacOS:**
```bash
export IBM_ACCESS_KEY="tu-access-key-id"
export IBM_SECRET_KEY="tu-secret-access-key"
export IBM_BUCKET_NAME="sistema-ventas-proveedores"
export IBM_SERVICE_URL="https://s3.us-south.cloud-object-storage.appdomain.cloud"
```

---

## 🧪 **PASO 5: Probar la configuración**

### **5.1 Ejecutar la API**
```bash
cd "C:\Users\Jonathan\Desktop\Sistema Ventas\backend\ApiVentas"
dotnet run
```

### **5.2 Endpoints disponibles**
```
🌐 Base URL: https://localhost:7030/api/v1/proveedores

📥 GET    /proveedores                    - Lista paginada
📄 GET    /proveedores/{id}              - Obtener por ID  
📝 POST   /proveedores                   - Crear proveedor
✏️ PATCH  /proveedores/{id}              - Actualizar
🔄 PATCH  /proveedores/{id}/estado       - Cambiar estado

📎 GET    /proveedores/{id}/con-archivos - Proveedor + archivos
📤 POST   /proveedores/{id}/subir-archivo - Subir catálogo
🗑️ DELETE /proveedores/archivo/{id}       - Eliminar archivo
👁️ GET    /proveedores/archivo/{id}/preview - URL temporal
📺 GET    /proveedores/visualizar?token= - Ver archivo
```

### **5.3 Swagger UI**
```
🌐 URL: https://localhost:7030/swagger
📋 Documentación interactiva completa
🧪 Probar endpoints directamente
```

---

## 🔒 **PASO 6: Configuración de seguridad avanzada**

### **6.1 Validaciones implementadas**
```
✅ Extensiones permitidas: pdf, doc, docx, xls, xlsx, png, jpg, jpeg, gif, txt
✅ Tamaño máximo: 15MB
✅ Nombres únicos: proveedor_{id}_{guid}.ext
✅ Tokens temporales: 30 minutos validez
✅ Firmas HMAC-SHA256 para seguridad
```

### **6.2 Estructura de archivos en IBM Cloud**
```
📁 sistema-ventas-proveedores/
   └── 📁 catalogos/
       └── 📁 proveedores/
           ├── 📄 proveedor_1_uuid1.pdf
           ├── 📄 proveedor_1_uuid2.docx
           ├── 📄 proveedor_2_uuid3.xlsx
           └── 📄 ...
```

---

## 🚨 **PASO 7: Solución de problemas**

### **7.1 Errores comunes**
```
❌ "The type or namespace name 'Amazon' could not be found"
✅ Instalar: dotnet add package AWSSDK.S3

❌ "Access denied" en IBM Cloud
✅ Verificar credenciales y permisos del bucket

❌ "Bucket not found"
✅ Verificar nombre del bucket en configuración

❌ "Endpoint not reachable"  
✅ Verificar URL del endpoint regional
```

### **7.2 Verificar instalación**
```bash
# Verificar paquetes instalados
dotnet list package

# Debe mostrar:
# AWSSDK.S3 4.0.11.1
```

### **7.3 Test de conectividad**
```bash
# Probar compilación
dotnet build

# Ejecutar aplicación
dotnet run

# Verificar endpoints en Swagger
# https://localhost:7030/swagger
```

---

## 📊 **PASO 8: Monitoreo y límites**

### **8.1 Panel de control IBM Cloud**
```
📊 Dashboard → Cloud Object Storage → tu-servicio
📈 Monitorear uso de almacenamiento
📊 Verificar operaciones restantes  
⚠️ Alertas automáticas al llegar a límites
```

### **8.2 Límites del plan gratuito**
```
💾 Almacenamiento: 25GB
📥 GET requests: 2,000/mes
📤 PUT requests: 200/mes  
🔄 DELETE requests: Ilimitado
📊 Lista requests: 200/mes
```

### **8.3 Optimización**
```
✅ Comprimir archivos antes de subir
✅ Usar formatos eficientes (PDF vs DOC)
✅ Implementar cache local temporal
✅ Borrar archivos antiguos automáticamente
```

---

## 🎉 **RESULTADO FINAL**

### **✅ Sistema completamente funcional:**
- ✅ API REST profesional con paginación
- ✅ Respuestas estandarizadas con ApiResponse<T>
- ✅ Manejo de archivos en IBM Cloud Storage gratuito
- ✅ Seguridad con tokens temporales
- ✅ Validaciones exhaustivas
- ✅ Documentación Swagger automática
- ✅ Sin autenticación para pruebas (como solicitaste)

### **🚀 Listo para usar:**
```bash
dotnet run
# API ejecutándose en: https://localhost:7030
# Swagger UI: https://localhost:7030/swagger
```

¡El sistema está completamente configurado y listo para manejar archivos de catálogos de proveedores de manera profesional! 🎯