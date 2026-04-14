// In-memory database for the sales API
const db = {
  productos: [],
  clientes: [],
  ventas: [],
  detallesVenta: [],
  _nextId: {
    productos: 1,
    clientes: 1,
    ventas: 1,
    detallesVenta: 1,
  },
};

function nextId(collection) {
  return db._nextId[collection]++;
}

function reset() {
  db.productos = [];
  db.clientes = [];
  db.ventas = [];
  db.detallesVenta = [];
  db._nextId = { productos: 1, clientes: 1, ventas: 1, detallesVenta: 1 };
}

module.exports = { db, nextId, reset };
