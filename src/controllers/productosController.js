const { db, nextId } = require('../models/db');

function listarProductos(req, res) {
  res.json(db.productos);
}

function obtenerProducto(req, res) {
  const producto = db.productos.find((p) => p.id === Number(req.params.id));
  if (!producto) {
    return res.status(404).json({ error: 'Producto no encontrado' });
  }
  res.json(producto);
}

function crearProducto(req, res) {
  const { nombre, precio, stock, descripcion } = req.body;
  if (!nombre || precio === undefined) {
    return res.status(400).json({ error: 'nombre y precio son requeridos' });
  }
  if (typeof precio !== 'number' || precio < 0) {
    return res.status(400).json({ error: 'precio debe ser un número positivo' });
  }
  const producto = {
    id: nextId('productos'),
    nombre,
    precio,
    stock: stock !== undefined ? stock : 0,
    descripcion: descripcion || '',
  };
  db.productos.push(producto);
  res.status(201).json(producto);
}

function actualizarProducto(req, res) {
  const idx = db.productos.findIndex((p) => p.id === Number(req.params.id));
  if (idx === -1) {
    return res.status(404).json({ error: 'Producto no encontrado' });
  }
  const { nombre, precio, stock, descripcion } = req.body;
  if (precio !== undefined && (typeof precio !== 'number' || precio < 0)) {
    return res.status(400).json({ error: 'precio debe ser un número positivo' });
  }
  const producto = db.productos[idx];
  db.productos[idx] = {
    ...producto,
    nombre: nombre !== undefined ? nombre : producto.nombre,
    precio: precio !== undefined ? precio : producto.precio,
    stock: stock !== undefined ? stock : producto.stock,
    descripcion: descripcion !== undefined ? descripcion : producto.descripcion,
  };
  res.json(db.productos[idx]);
}

function eliminarProducto(req, res) {
  const idx = db.productos.findIndex((p) => p.id === Number(req.params.id));
  if (idx === -1) {
    return res.status(404).json({ error: 'Producto no encontrado' });
  }
  db.productos.splice(idx, 1);
  res.status(204).send();
}

module.exports = {
  listarProductos,
  obtenerProducto,
  crearProducto,
  actualizarProducto,
  eliminarProducto,
};
