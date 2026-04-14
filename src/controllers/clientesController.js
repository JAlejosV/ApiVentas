const { db, nextId } = require('../models/db');

function listarClientes(req, res) {
  res.json(db.clientes);
}

function obtenerCliente(req, res) {
  const cliente = db.clientes.find((c) => c.id === Number(req.params.id));
  if (!cliente) {
    return res.status(404).json({ error: 'Cliente no encontrado' });
  }
  res.json(cliente);
}

function crearCliente(req, res) {
  const { nombre, email, telefono, direccion } = req.body;
  if (!nombre) {
    return res.status(400).json({ error: 'nombre es requerido' });
  }
  if (email && db.clientes.find((c) => c.email === email)) {
    return res.status(409).json({ error: 'Ya existe un cliente con ese email' });
  }
  const cliente = {
    id: nextId('clientes'),
    nombre,
    email: email || '',
    telefono: telefono || '',
    direccion: direccion || '',
  };
  db.clientes.push(cliente);
  res.status(201).json(cliente);
}

function actualizarCliente(req, res) {
  const idx = db.clientes.findIndex((c) => c.id === Number(req.params.id));
  if (idx === -1) {
    return res.status(404).json({ error: 'Cliente no encontrado' });
  }
  const { nombre, email, telefono, direccion } = req.body;
  if (
    email &&
    db.clientes.find((c) => c.email === email && c.id !== Number(req.params.id))
  ) {
    return res.status(409).json({ error: 'Ya existe un cliente con ese email' });
  }
  const cliente = db.clientes[idx];
  db.clientes[idx] = {
    ...cliente,
    nombre: nombre !== undefined ? nombre : cliente.nombre,
    email: email !== undefined ? email : cliente.email,
    telefono: telefono !== undefined ? telefono : cliente.telefono,
    direccion: direccion !== undefined ? direccion : cliente.direccion,
  };
  res.json(db.clientes[idx]);
}

function eliminarCliente(req, res) {
  const idx = db.clientes.findIndex((c) => c.id === Number(req.params.id));
  if (idx === -1) {
    return res.status(404).json({ error: 'Cliente no encontrado' });
  }
  db.clientes.splice(idx, 1);
  res.status(204).send();
}

module.exports = {
  listarClientes,
  obtenerCliente,
  crearCliente,
  actualizarCliente,
  eliminarCliente,
};
