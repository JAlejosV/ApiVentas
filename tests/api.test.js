const request = require('supertest');
const app = require('../src/app');
const { reset } = require('../src/models/db');

beforeEach(() => reset());

describe('GET /', () => {
  it('devuelve información de la API', async () => {
    const res = await request(app).get('/');
    expect(res.status).toBe(200);
    expect(res.body.nombre).toBe('ApiVentas - Alejos');
    expect(res.body.endpoints).toHaveProperty('productos');
    expect(res.body.endpoints).toHaveProperty('clientes');
    expect(res.body.endpoints).toHaveProperty('ventas');
  });
});

describe('Productos', () => {
  it('lista productos vacía al inicio', async () => {
    const res = await request(app).get('/api/productos');
    expect(res.status).toBe(200);
    expect(res.body).toEqual([]);
  });

  it('crea un producto', async () => {
    const res = await request(app).post('/api/productos').send({
      nombre: 'Manzana',
      precio: 5.5,
      stock: 100,
      descripcion: 'Fruta fresca',
    });
    expect(res.status).toBe(201);
    expect(res.body.id).toBe(1);
    expect(res.body.nombre).toBe('Manzana');
    expect(res.body.precio).toBe(5.5);
    expect(res.body.stock).toBe(100);
  });

  it('falla al crear producto sin nombre', async () => {
    const res = await request(app).post('/api/productos').send({ precio: 10 });
    expect(res.status).toBe(400);
    expect(res.body.error).toMatch(/nombre/i);
  });

  it('falla al crear producto con precio negativo', async () => {
    const res = await request(app).post('/api/productos').send({ nombre: 'X', precio: -1 });
    expect(res.status).toBe(400);
  });

  it('obtiene un producto por id', async () => {
    await request(app).post('/api/productos').send({ nombre: 'Pan', precio: 2 });
    const res = await request(app).get('/api/productos/1');
    expect(res.status).toBe(200);
    expect(res.body.nombre).toBe('Pan');
  });

  it('devuelve 404 para producto inexistente', async () => {
    const res = await request(app).get('/api/productos/999');
    expect(res.status).toBe(404);
  });

  it('actualiza un producto', async () => {
    await request(app).post('/api/productos').send({ nombre: 'Leche', precio: 10 });
    const res = await request(app).put('/api/productos/1').send({ precio: 12 });
    expect(res.status).toBe(200);
    expect(res.body.precio).toBe(12);
    expect(res.body.nombre).toBe('Leche');
  });

  it('elimina un producto', async () => {
    await request(app).post('/api/productos').send({ nombre: 'Arroz', precio: 8 });
    const del = await request(app).delete('/api/productos/1');
    expect(del.status).toBe(204);
    const get = await request(app).get('/api/productos/1');
    expect(get.status).toBe(404);
  });
});

describe('Clientes', () => {
  it('lista clientes vacía al inicio', async () => {
    const res = await request(app).get('/api/clientes');
    expect(res.status).toBe(200);
    expect(res.body).toEqual([]);
  });

  it('crea un cliente', async () => {
    const res = await request(app).post('/api/clientes').send({
      nombre: 'Juan Pérez',
      email: 'juan@example.com',
      telefono: '555-1234',
      direccion: 'Calle 1',
    });
    expect(res.status).toBe(201);
    expect(res.body.id).toBe(1);
    expect(res.body.nombre).toBe('Juan Pérez');
  });

  it('falla al crear cliente sin nombre', async () => {
    const res = await request(app).post('/api/clientes').send({ email: 'a@b.com' });
    expect(res.status).toBe(400);
  });

  it('falla al crear cliente con email duplicado', async () => {
    await request(app).post('/api/clientes').send({ nombre: 'A', email: 'dup@x.com' });
    const res = await request(app).post('/api/clientes').send({ nombre: 'B', email: 'dup@x.com' });
    expect(res.status).toBe(409);
  });

  it('actualiza un cliente', async () => {
    await request(app).post('/api/clientes').send({ nombre: 'Ana', email: 'ana@x.com' });
    const res = await request(app).put('/api/clientes/1').send({ telefono: '999' });
    expect(res.status).toBe(200);
    expect(res.body.telefono).toBe('999');
    expect(res.body.nombre).toBe('Ana');
  });

  it('elimina un cliente', async () => {
    await request(app).post('/api/clientes').send({ nombre: 'Carlos' });
    const del = await request(app).delete('/api/clientes/1');
    expect(del.status).toBe(204);
    const get = await request(app).get('/api/clientes/1');
    expect(get.status).toBe(404);
  });
});

describe('Ventas', () => {
  beforeEach(async () => {
    await request(app).post('/api/productos').send({ nombre: 'Agua', precio: 3, stock: 50 });
    await request(app).post('/api/productos').send({ nombre: 'Refresco', precio: 7, stock: 20 });
    await request(app).post('/api/clientes').send({ nombre: 'María López' });
  });

  it('lista ventas vacía al inicio', async () => {
    const res = await request(app).get('/api/ventas');
    expect(res.status).toBe(200);
    expect(res.body).toEqual([]);
  });

  it('crea una venta con detalles', async () => {
    const res = await request(app).post('/api/ventas').send({
      cliente_id: 1,
      detalles: [{ producto_id: 1, cantidad: 2 }],
    });
    expect(res.status).toBe(201);
    expect(res.body.total).toBe(6);
    expect(res.body.detalles).toHaveLength(1);
    expect(res.body.detalles[0].subtotal).toBe(6);
    expect(res.body.estado).toBe('pendiente');
  });

  it('reduce el stock al crear una venta', async () => {
    await request(app).post('/api/ventas').send({
      cliente_id: 1,
      detalles: [{ producto_id: 1, cantidad: 5 }],
    });
    const prod = await request(app).get('/api/productos/1');
    expect(prod.body.stock).toBe(45);
  });

  it('falla con stock insuficiente', async () => {
    const res = await request(app).post('/api/ventas').send({
      cliente_id: 1,
      detalles: [{ producto_id: 1, cantidad: 999 }],
    });
    expect(res.status).toBe(400);
    expect(res.body.error).toMatch(/stock/i);
  });

  it('falla si cliente no existe', async () => {
    const res = await request(app).post('/api/ventas').send({
      cliente_id: 999,
      detalles: [{ producto_id: 1, cantidad: 1 }],
    });
    expect(res.status).toBe(404);
  });

  it('falla si producto no existe', async () => {
    const res = await request(app).post('/api/ventas').send({
      cliente_id: 1,
      detalles: [{ producto_id: 999, cantidad: 1 }],
    });
    expect(res.status).toBe(404);
  });

  it('actualiza el estado de una venta', async () => {
    await request(app).post('/api/ventas').send({
      cliente_id: 1,
      detalles: [{ producto_id: 1, cantidad: 1 }],
    });
    const res = await request(app).put('/api/ventas/1').send({ estado: 'pagada' });
    expect(res.status).toBe(200);
    expect(res.body.estado).toBe('pagada');
  });

  it('falla al actualizar con estado inválido', async () => {
    await request(app).post('/api/ventas').send({
      cliente_id: 1,
      detalles: [{ producto_id: 1, cantidad: 1 }],
    });
    const res = await request(app).put('/api/ventas/1').send({ estado: 'invalido' });
    expect(res.status).toBe(400);
  });

  it('elimina una venta y restaura el stock', async () => {
    await request(app).post('/api/ventas').send({
      cliente_id: 1,
      detalles: [{ producto_id: 1, cantidad: 3 }],
    });
    const del = await request(app).delete('/api/ventas/1');
    expect(del.status).toBe(204);
    const prod = await request(app).get('/api/productos/1');
    expect(prod.body.stock).toBe(50);
  });

  it('obtiene una venta con sus detalles', async () => {
    await request(app).post('/api/ventas').send({
      cliente_id: 1,
      detalles: [
        { producto_id: 1, cantidad: 2 },
        { producto_id: 2, cantidad: 1 },
      ],
    });
    const res = await request(app).get('/api/ventas/1');
    expect(res.status).toBe(200);
    expect(res.body.detalles).toHaveLength(2);
    expect(res.body.total).toBe(13);
  });
});

describe('Ruta no encontrada', () => {
  it('devuelve 404 para rutas inexistentes', async () => {
    const res = await request(app).get('/api/ruta-inexistente');
    expect(res.status).toBe(404);
  });
});
