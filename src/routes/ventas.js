const { Router } = require('express');
const {
  listarVentas,
  obtenerVenta,
  crearVenta,
  actualizarEstadoVenta,
  eliminarVenta,
} = require('../controllers/ventasController');

const router = Router();

router.get('/', listarVentas);
router.get('/:id', obtenerVenta);
router.post('/', crearVenta);
router.put('/:id', actualizarEstadoVenta);
router.delete('/:id', eliminarVenta);

module.exports = router;
