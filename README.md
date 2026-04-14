# ApiVentas

API REST de Ventas para Alejos, construida con Node.js y Express.

## Instalación

```bash
npm install
```

## Uso

```bash
npm start
```

El servidor escucha por defecto en el puerto `3000` (configurable con la variable de entorno `PORT`).

## Endpoints

### Raíz

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/` | Información general de la API |

### Productos — `/api/productos`

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/productos` | Listar todos los productos |
| POST | `/api/productos` | Crear un producto |
| GET | `/api/productos/:id` | Obtener un producto |
| PUT | `/api/productos/:id` | Actualizar un producto |
| DELETE | `/api/productos/:id` | Eliminar un producto |

**Cuerpo para crear/actualizar producto:**
```json
{
  "nombre": "Manzana",
  "precio": 5.5,
  "stock": 100,
  "descripcion": "Fruta fresca"
}
```

### Clientes — `/api/clientes`

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/clientes` | Listar todos los clientes |
| POST | `/api/clientes` | Crear un cliente |
| GET | `/api/clientes/:id` | Obtener un cliente |
| PUT | `/api/clientes/:id` | Actualizar un cliente |
| DELETE | `/api/clientes/:id` | Eliminar un cliente |

**Cuerpo para crear/actualizar cliente:**
```json
{
  "nombre": "Juan Pérez",
  "email": "juan@example.com",
  "telefono": "555-1234",
  "direccion": "Calle 1"
}
```

### Ventas — `/api/ventas`

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/ventas` | Listar todas las ventas |
| POST | `/api/ventas` | Crear una venta |
| GET | `/api/ventas/:id` | Obtener una venta con sus detalles |
| PUT | `/api/ventas/:id` | Actualizar el estado de una venta |
| DELETE | `/api/ventas/:id` | Eliminar una venta |

**Cuerpo para crear una venta:**
```json
{
  "cliente_id": 1,
  "detalles": [
    { "producto_id": 1, "cantidad": 2 },
    { "producto_id": 3, "cantidad": 1 }
  ]
}
```

**Estados válidos:** `pendiente`, `pagada`, `cancelada`, `enviada`

**Cuerpo para actualizar estado:**
```json
{ "estado": "pagada" }
```

## Tests

```bash
npm test
```
