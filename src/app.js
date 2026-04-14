const express = require('express');
const productosRouter = require('./routes/productos');
const clientesRouter = require('./routes/clientes');
const ventasRouter = require('./routes/ventas');
const { errorHandler, notFound } = require('./middleware/errorHandler');

const app = express();

app.use(express.json());

app.get('/', (req, res) => {
  res.json({
    nombre: 'ApiVentas - Alejos',
    version: '1.0.0',
    endpoints: {
      productos: '/api/productos',
      clientes: '/api/clientes',
      ventas: '/api/ventas',
    },
  });
});

app.use('/api/productos', productosRouter);
app.use('/api/clientes', clientesRouter);
app.use('/api/ventas', ventasRouter);

app.use(notFound);
app.use(errorHandler);

module.exports = app;
