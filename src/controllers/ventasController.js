const { db, nextId } = require('../models/db');

function listarVentas(req, res) {
  const ventas = db.ventas.map((v) => ({
    ...v,
    detalles: db.detallesVenta.filter((d) => d.venta_id === v.id),
  }));
  res.json(ventas);
}

function obtenerVenta(req, res) {
  const venta = db.ventas.find((v) => v.id === Number(req.params.id));
  if (!venta) {
    return res.status(404).json({ error: 'Venta no encontrada' });
  }
  res.json({
    ...venta,
    detalles: db.detallesVenta.filter((d) => d.venta_id === venta.id),
  });
}

function crearVenta(req, res) {
  const { cliente_id, detalles } = req.body;

  if (!cliente_id) {
    return res.status(400).json({ error: 'cliente_id es requerido' });
  }
  if (!Array.isArray(detalles) || detalles.length === 0) {
    return res.status(400).json({ error: 'detalles debe ser un arreglo no vacío' });
  }

  const cliente = db.clientes.find((c) => c.id === Number(cliente_id));
  if (!cliente) {
    return res.status(404).json({ error: 'Cliente no encontrado' });
  }

  // Validate products and stock
  for (const detalle of detalles) {
    if (!detalle.producto_id || !detalle.cantidad) {
      return res.status(400).json({ error: 'Cada detalle requiere producto_id y cantidad' });
    }
    const producto = db.productos.find((p) => p.id === Number(detalle.producto_id));
    if (!producto) {
      return res.status(404).json({ error: `Producto ${detalle.producto_id} no encontrado` });
    }
    if (producto.stock < Number(detalle.cantidad)) {
      return res.status(400).json({ error: `Stock insuficiente para el producto ${producto.nombre}` });
    }
  }

  // Create sale
  const ventaId = nextId('ventas');
  let total = 0;
  const nuevosDetalles = [];

  for (const detalle of detalles) {
    const producto = db.productos.find((p) => p.id === Number(detalle.producto_id));
    const cantidad = Number(detalle.cantidad);
    const precioUnitario = producto.precio;
    const subtotal = precioUnitario * cantidad;
    total += subtotal;

    // Reduce stock
    producto.stock -= cantidad;

    const nuevoDetalle = {
      id: nextId('detallesVenta'),
      venta_id: ventaId,
      producto_id: producto.id,
      cantidad,
      precio_unitario: precioUnitario,
      subtotal,
    };
    db.detallesVenta.push(nuevoDetalle);
    nuevosDetalles.push(nuevoDetalle);
  }

  const venta = {
    id: ventaId,
    cliente_id: Number(cliente_id),
    fecha: new Date().toISOString(),
    total,
    estado: 'pendiente',
  };
  db.ventas.push(venta);

  res.status(201).json({ ...venta, detalles: nuevosDetalles });
}

function actualizarEstadoVenta(req, res) {
  const idx = db.ventas.findIndex((v) => v.id === Number(req.params.id));
  if (idx === -1) {
    return res.status(404).json({ error: 'Venta no encontrada' });
  }
  const { estado } = req.body;
  const estadosValidos = ['pendiente', 'pagada', 'cancelada', 'enviada'];
  if (!estado || !estadosValidos.includes(estado)) {
    return res.status(400).json({
      error: `estado debe ser uno de: ${estadosValidos.join(', ')}`,
    });
  }
  db.ventas[idx] = { ...db.ventas[idx], estado };
  res.json({
    ...db.ventas[idx],
    detalles: db.detallesVenta.filter((d) => d.venta_id === db.ventas[idx].id),
  });
}

function eliminarVenta(req, res) {
  const idx = db.ventas.findIndex((v) => v.id === Number(req.params.id));
  if (idx === -1) {
    return res.status(404).json({ error: 'Venta no encontrada' });
  }

  // Restore stock
  const detalles = db.detallesVenta.filter((d) => d.venta_id === db.ventas[idx].id);
  for (const detalle of detalles) {
    const producto = db.productos.find((p) => p.id === detalle.producto_id);
    if (producto) {
      producto.stock += detalle.cantidad;
    }
  }

  // Remove sale details
  db.detallesVenta = db.detallesVenta.filter((d) => d.venta_id !== db.ventas[idx].id);
  db.ventas.splice(idx, 1);
  res.status(204).send();
}

module.exports = {
  listarVentas,
  obtenerVenta,
  crearVenta,
  actualizarEstadoVenta,
  eliminarVenta,
};
