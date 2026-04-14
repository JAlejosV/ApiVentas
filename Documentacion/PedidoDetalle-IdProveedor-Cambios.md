# Cambios en PedidoDetalle - Integridad Referencial IdProveedor

## Fecha
2 de abril de 2026

## Objetivo
Adaptar el API para reflejar los cambios en la base de datos que garantizan la integridad referencial entre `Pedido`, `PedidoDetalle` y `ProductoProveedor` a través del campo `IdProveedor`.

## Problema Resuelto
Anteriormente, `PedidoDetalle` referenciaba `IdProductoProveedor` (que internamente tiene un `IdProveedor`), pero no había validación para garantizar que ese `IdProveedor` del `ProductoProveedor` coincidiera con el `IdProveedor` del `Pedido`.

## Cambios en Base de Datos (Ya realizados)
```sql
-- Constraint único en ProductoProveedor
ALTER TABLE ProductoProveedor
ADD CONSTRAINT UQ_ProductoProveedor_IdProveedorIdProductoProveedor
UNIQUE (IdProductoProveedor, IdProveedor);

-- Agregar IdProveedor en PedidoDetalle
ALTER TABLE PedidoDetalle
ADD COLUMN IdProveedor INTEGER NOT NULL;

-- FK compuesta: IdProductoProveedor pertenece al mismo IdProveedor
ALTER TABLE PedidoDetalle
ADD CONSTRAINT FK_PedidoDetalle_ProductoProveedor_Proveedor
FOREIGN KEY (IdProductoProveedor, IdProveedor)
REFERENCES ProductoProveedor (IdProductoProveedor, IdProveedor);

-- FK: IdProveedor del detalle coincide con el del Pedido
ALTER TABLE PedidoDetalle
ADD CONSTRAINT FK_PedidoDetalle_Pedido_Proveedor
FOREIGN KEY (IdPedido, IdProveedor)
REFERENCES Pedido (IdPedido, IdProveedor);

-- Constraint único en Pedido
ALTER TABLE Pedido
ADD CONSTRAINT UQ_Pedido_IdPedidoIdProveedor
UNIQUE (IdPedido, IdProveedor);
```

## Flujo de Integridad Garantizado
```
Pedido.IdProveedor ──────────────────────────────┐
                                                   ▼
PedidoDetalle.IdProveedor ──► debe coincidir en ambas FK
                                                   ▼
ProductoProveedor.IdProveedor ◄── mismo proveedor garantizado
```

## Cambios Realizados en el API

### 1. Modelo PedidoDetalle.cs
**Archivo:** `ApiVentas/Modelos/PedidoDetalle.cs`

**Cambio:** Agregada la propiedad `IdProveedor`
```csharp
[Required(ErrorMessage = "El proveedor es obligatorio")]
public int IdProveedor { get; set; }
```

### 2. ApplicationDbContext.cs
**Archivo:** `ApiVentas/Data/ApplicationDbContext.cs`

**Cambio:** Agregado el mapeo de la columna `idproveedor`
```csharp
entity.Property(e => e.IdProveedor).HasColumnName("idproveedor");
```

### 3. DTOs Actualizados

#### PedidoDetalleDto.cs
**Archivo:** `ApiVentas/Modelos/Dtos/PedidoDetalleDto.cs`

**Cambio:** Agregada la propiedad de lectura
```csharp
public int IdProveedor { get; set; }
```

#### PedidoDetalleActualizarDto.cs
**Archivo:** `ApiVentas/Modelos/Dtos/PedidoDetalleActualizarDto.cs`

**Cambio:** Agregada la propiedad con validación
```csharp
[Required(ErrorMessage = "El proveedor es obligatorio")]
public int IdProveedor { get; set; }
```

#### PedidoDetalleCrearDto.cs
**Sin cambios.** El `IdProveedor` se asigna automáticamente desde el `Pedido` en el controlador.

### 4. PedidosController.cs
**Archivo:** `ApiVentas/Controllers/PedidosController.cs`

#### Método CrearPedido
**Cambio:** Asignación automática del `IdProveedor` a cada detalle
```csharp
// Asignar usuario y proveedor a los detalles
foreach (var detalle in pedido.PedidoDetalles)
{
    detalle.IdProveedor = pedido.IdProveedor; // Garantizar que coincida con el pedido
    detalle.UsuarioCreacion = usuarioActual;
    detalle.FechaHoraCreacion = DateTime.UtcNow;
}
```

#### Método ActualizarPedido
**Cambio:** Asignación automática del `IdProveedor` a cada detalle
```csharp
// Asignar usuario y proveedor a los detalles
foreach (var detalle in pedido.PedidoDetalles)
{
    detalle.IdPedido = id;
    detalle.IdProveedor = pedido.IdProveedor; // Garantizar que coincida con el pedido
    detalle.UsuarioCreacion = usuarioActual;
    detalle.FechaHoraCreacion = DateTime.UtcNow;
}
```

## Validación de Integridad

La integridad referencial ahora está garantizada por:

1. **A nivel de base de datos:** Las foreign keys compuestas impiden inconsistencias.
2. **A nivel de API:** El controlador asigna automáticamente el `IdProveedor` del `Pedido` a cada `PedidoDetalle`.

### Ejemplo de flujo:
1. Usuario crea un `Pedido` con `IdProveedor = 5`
2. Usuario agrega detalles con `IdProductoProveedor = 123`
3. El API automáticamente asigna `IdProveedor = 5` a cada detalle
4. La BD valida que:
   - El `ProductoProveedor` con `IdProductoProveedor = 123` tenga `IdProveedor = 5`
   - El `IdProveedor = 5` del detalle coincida con el del `Pedido`

## Estado de Compilación
✅ Sin errores de compilación
✅ Validaciones implementadas
✅ Integridad referencial garantizada

## Notas Importantes

- **No se requiere migración de Entity Framework** ya que los cambios de BD se hicieron directamente con SQL.
- El `IdProveedor` en `PedidoDetalleCrearDto` **NO es necesario** porque se asigna automáticamente en el controlador desde el `Pedido`.
- Si el usuario intenta crear un detalle con un `ProductoProveedor` que no pertenece al mismo proveedor del `Pedido`, la BD rechazará la operación con un error de foreign key.

## Próximos Pasos Recomendados

1. **Testing:** Probar la creación y actualización de pedidos verificando que:
   - Se asigna correctamente el `IdProveedor` en cada detalle
   - La BD rechaza detalles con proveedores diferentes
   
2. **Validación de frontend:** Asegurar que el frontend solo muestre `ProductoProveedor` del mismo proveedor seleccionado en el `Pedido`.

3. **Manejo de errores:** Considerar agregar mensajes de error más descriptivos cuando falle una FK compuesta.
