using ApiVentas.Attributes;
using ApiVentas.Modelos.ApiResponse;
using ApiVentas.Modelos.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Text;

namespace ApiVentas.Controllers
{
    [Route("api/v1/inventario")]
    [ApiController]
    [Authorize]
    public class InventarioController : ControllerBase
    {
        private static readonly HashSet<string> _allowedExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".xls", ".xlsx" };

        // Cabeceras del inventario físico (excel del conteo)
        private const string H_ARTICULO = "Artículo";
        private const string H_CODIGO_BARRAS = "Código de barras";
        private const string H_CANTIDAD = "Cantidad";

        // Cabeceras del inventario del sistema
        private const string H_COD_INTERNO = "Cod. Interno";
        private const string H_NOMBRE = "Nombre";
        private const string H_STOCK_ACTUAL = "Stock actual";
        private const string H_PRECIO_VENTA = "Precio de venta";

        /// <summary>
        /// Procesa 1 o más archivos de inventario físico (.xls/.xlsx) junto con el inventario del sistema
        /// y retorna la comparación en formato JSON.
        /// Formato inventario físico: Artículo | Código de barras | Cantidad
        /// Formato inventario sistema: Cod. Interno | Nombre | Stock actual | Precio de venta
        /// </summary>
        [HttpPost("comparar")]
        [PermissionRequired("Inventario", "Comparar")]
        public IActionResult Comparar(
            [FromForm] List<IFormFile> archivosInventarioFisico,
            [FromForm] IFormFile archivoInventarioSistema,
            [FromForm] decimal? montoCuadre = null)
        {
            var todosLosErrores = new List<string>();
            todosLosErrores.AddRange(ValidarArchivos(archivosInventarioFisico, archivoInventarioSistema));

            Dictionary<string, FisicoConsolidado> fisico = new();
            if (archivosInventarioFisico?.Count > 0)
            {
                var (fisicoData, erroresFisico) = LeerInventarioFisico(archivosInventarioFisico);
                todosLosErrores.AddRange(erroresFisico);
                if (!erroresFisico.Any()) fisico = fisicoData;
            }

            List<SistemaItem> sistema = new();
            if (archivoInventarioSistema != null)
            {
                var (sistemaData, errorSistema) = LeerInventarioSistema(archivoInventarioSistema);
                if (errorSistema != null) todosLosErrores.Add(errorSistema);
                else sistema = sistemaData;
            }

            if (todosLosErrores.Any())
                return BadRequest(ApiResponse<object>.ErrorResponse("Error al procesar los archivos.", todosLosErrores));

            var resultado = GenerarComparacion(fisico, sistema, montoCuadre ?? 0m);
            return Ok(ApiResponse<InventarioResultadoDto>.SuccessResponse(resultado, "Comparación generada correctamente."));
        }

        /// <summary>
        /// Procesa 1 o más archivos de inventario físico junto con el inventario del sistema
        /// y retorna el resultado como archivo Excel descargable.
        /// </summary>
        [HttpPost("exportar")]
        [PermissionRequired("Inventario", "Exportar")]
        public IActionResult Exportar(
            [FromForm] List<IFormFile> archivosInventarioFisico,
            [FromForm] IFormFile archivoInventarioSistema,
            [FromForm] decimal? montoCuadre = null)
        {
            var todosLosErrores = new List<string>();
            todosLosErrores.AddRange(ValidarArchivos(archivosInventarioFisico, archivoInventarioSistema));

            Dictionary<string, FisicoConsolidado> fisico = new();
            if (archivosInventarioFisico?.Count > 0)
            {
                var (fisicoData, erroresFisico) = LeerInventarioFisico(archivosInventarioFisico);
                todosLosErrores.AddRange(erroresFisico);
                if (!erroresFisico.Any()) fisico = fisicoData;
            }

            List<SistemaItem> sistema = new();
            if (archivoInventarioSistema != null)
            {
                var (sistemaData, errorSistema) = LeerInventarioSistema(archivoInventarioSistema);
                if (errorSistema != null) todosLosErrores.Add(errorSistema);
                else sistema = sistemaData;
            }

            if (todosLosErrores.Any())
                return BadRequest(ApiResponse<object>.ErrorResponse("Error al procesar los archivos.", todosLosErrores));

            var resultado = GenerarComparacion(fisico, sistema, montoCuadre ?? 0m);
            var excelBytes = GenerarExcel(resultado);

            return File(
                excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Comparacion_Inventario_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        // ─── Validación ──────────────────────────────────────────────────────────

        private List<string> ValidarArchivos(List<IFormFile> archivosInventarioFisico, IFormFile archivoInventarioSistema)
        {
            var errores = new List<string>();

            if (archivosInventarioFisico == null || archivosInventarioFisico.Count == 0)
                errores.Add("Debe proporcionar al menos un archivo de inventario físico.");
            else
            {
                foreach (var file in archivosInventarioFisico)
                {
                    var ext = Path.GetExtension(file.FileName);
                    if (!_allowedExtensions.Contains(ext))
                        errores.Add($"El archivo '{file.FileName}' no tiene un formato válido. Solo se aceptan .xls y .xlsx.");
                }
            }

            if (archivoInventarioSistema == null)
                errores.Add("Debe proporcionar el archivo de inventario del sistema.");
            else
            {
                var extSistema = Path.GetExtension(archivoInventarioSistema.FileName);
                if (!_allowedExtensions.Contains(extSistema))
                    errores.Add("El archivo del inventario del sistema no tiene un formato válido. Solo se aceptan .xls y .xlsx.");
            }

            return errores;
        }

        // ─── Lectura de inventario físico ─────────────────────────────────────────

        private (Dictionary<string, FisicoConsolidado> datos, List<string> errores)
            LeerInventarioFisico(List<IFormFile> archivos)
        {
            var consolidado = new Dictionary<string, FisicoConsolidado>(StringComparer.OrdinalIgnoreCase);
            var errores = new List<string>();
            bool validarCabeceras = true;

            foreach (var archivo in archivos)
            {
                var (items, error) = LeerExcelFisicoInterno(archivo, validarCabeceras);
                if (error != null)
                {
                    errores.Add(error);
                    continue;
                }

                foreach (var item in items)
                {
                    if (consolidado.TryGetValue(item.CodigoBarras, out var existente))
                        existente.Cantidad += item.Cantidad;
                    else
                        consolidado[item.CodigoBarras] = new FisicoConsolidado
                        {
                            Articulo = item.Articulo,
                            CodigoBarras = item.CodigoBarras,
                            Cantidad = item.Cantidad
                        };
                }
            }

            return (consolidado, errores);
        }

        private (List<FisicoItem> items, string error) LeerExcelFisicoInterno(IFormFile archivo, bool validarCabeceras)
        {
            try
            {
                using var ms = new MemoryStream();
                archivo.CopyTo(ms);
                ms.Position = 0;

                var workbook = AbrirWorkbook(Path.GetExtension(archivo.FileName), ms);
                var sheet = workbook.GetSheetAt(0);
                var headerRow = sheet?.GetRow(0);

                if (validarCabeceras)
                {
                    if (headerRow == null || headerRow.LastCellNum < 3)
                        return (null, $"El archivo '{archivo.FileName}' tiene formato incorrecto. " +
                            $"Se esperan al menos 3 columnas: '{H_ARTICULO}', '{H_CODIGO_BARRAS}', '{H_CANTIDAD}'.");

                    var col0 = NormalizarCabecera(ObtenerValorCelda(headerRow.GetCell(0)));
                    var col1 = NormalizarCabecera(ObtenerValorCelda(headerRow.GetCell(1)));
                    var col2 = NormalizarCabecera(ObtenerValorCelda(headerRow.GetCell(2)));

                    if (col0 != NormalizarCabecera(H_ARTICULO) ||
                        col1 != NormalizarCabecera(H_CODIGO_BARRAS) ||
                        col2 != NormalizarCabecera(H_CANTIDAD))
                    {
                        return (null, $"El archivo '{archivo.FileName}' tiene formato incorrecto. " +
                            $"Las cabeceras deben ser: '{H_ARTICULO}', '{H_CODIGO_BARRAS}', '{H_CANTIDAD}'.");
                    }
                }

                var items = new List<FisicoItem>();
                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row == null) continue;

                    var articulo = ObtenerValorCelda(row.GetCell(0))?.Trim() ?? string.Empty;
                    var codigoBarras = ObtenerValorCelda(row.GetCell(1))?.Trim();
                    var cantidadStr = ObtenerValorCelda(row.GetCell(2));

                    if (string.IsNullOrWhiteSpace(codigoBarras)) continue;

                    decimal.TryParse(cantidadStr, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal cantidad);

                    items.Add(new FisicoItem
                    {
                        Articulo = articulo,
                        CodigoBarras = codigoBarras,
                        Cantidad = cantidad
                    });
                }

                return (items, null);
            }
            catch (Exception ex)
            {
                return (null, $"Error al leer el archivo '{archivo.FileName}': {ex.Message}");
            }
        }

        // ─── Lectura de inventario del sistema ────────────────────────────────────

        private (List<SistemaItem> datos, string error) LeerInventarioSistema(IFormFile archivo)
        {
            try
            {
                using var ms = new MemoryStream();
                archivo.CopyTo(ms);
                ms.Position = 0;

                var workbook = AbrirWorkbook(Path.GetExtension(archivo.FileName), ms);
                var sheet = workbook.GetSheetAt(0);
                var headerRow = sheet?.GetRow(0);

                if (headerRow == null || headerRow.LastCellNum < 4)
                    return (null, $"El archivo '{archivo.FileName}' tiene formato incorrecto. " +
                        $"Se esperan 4 columnas: '{H_COD_INTERNO}', '{H_NOMBRE}', '{H_STOCK_ACTUAL}', '{H_PRECIO_VENTA}'.");

                var col0 = NormalizarCabecera(ObtenerValorCelda(headerRow.GetCell(0)));
                var col1 = NormalizarCabecera(ObtenerValorCelda(headerRow.GetCell(1)));
                var col2 = NormalizarCabecera(ObtenerValorCelda(headerRow.GetCell(2)));
                var col3 = NormalizarCabecera(ObtenerValorCelda(headerRow.GetCell(3)));

                if (col0 != NormalizarCabecera(H_COD_INTERNO) ||
                    col1 != NormalizarCabecera(H_NOMBRE) ||
                    col2 != NormalizarCabecera(H_STOCK_ACTUAL) ||
                    col3 != NormalizarCabecera(H_PRECIO_VENTA))
                {
                    return (null, $"El archivo '{archivo.FileName}' tiene formato incorrecto. " +
                        $"Las cabeceras deben ser: '{H_COD_INTERNO}', '{H_NOMBRE}', '{H_STOCK_ACTUAL}', '{H_PRECIO_VENTA}'.");
                }

                var items = new List<SistemaItem>();
                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row == null) continue;

                    var codInterno = ObtenerValorCelda(row.GetCell(0))?.Trim();
                    var nombre = ObtenerValorCelda(row.GetCell(1))?.Trim() ?? string.Empty;
                    var stockStr = ObtenerValorCelda(row.GetCell(2));
                    var precioStr = ObtenerValorCelda(row.GetCell(3));

                    if (string.IsNullOrWhiteSpace(codInterno)) continue;

                    decimal.TryParse(stockStr, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal stock);
                    decimal.TryParse(precioStr, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal precio);

                    items.Add(new SistemaItem
                    {
                        CodInterno = codInterno,
                        Nombre = nombre,
                        StockActual = stock,
                        PrecioVenta = precio
                    });
                }

                return (items, null);
            }
            catch (Exception ex)
            {
                return (null, $"Error al leer el archivo del sistema '{archivo.FileName}': {ex.Message}");
            }
        }

        // ─── Comparación ──────────────────────────────────────────────────────────

        private InventarioResultadoDto GenerarComparacion(
            Dictionary<string, FisicoConsolidado> fisico,
            List<SistemaItem> sistema,
            decimal montoCuadre = 0)
        {
            var items = new List<InventarioResultadoItemDto>();

            foreach (var prod in sistema)
            {
                if (prod.Nombre.StartsWith("ZZ", StringComparison.OrdinalIgnoreCase)) continue;

                fisico.TryGetValue(prod.CodInterno, out var fisicoItem);
                decimal totalFisico = fisicoItem?.Cantidad ?? 0m;
                decimal diferencia = totalFisico - prod.StockActual;

                decimal monto = diferencia * prod.PrecioVenta;

                items.Add(new InventarioResultadoItemDto
                {
                    CodigoBarras = prod.CodInterno,
                    Articulo = prod.Nombre,
                    TotalFisico = totalFisico,
                    TotalSistema = prod.StockActual,
                    Diferencia = diferencia,
                    Precio = prod.PrecioVenta,
                    Monto = monto
                });
            }

            // Mayor diferencia negativa primero (mayor pérdida al tope)
            items = items.OrderBy(x => x.Monto).ToList();

            decimal totalMonto = items.Sum(x => x.Monto);

            return new InventarioResultadoDto
            {
                Items = items,
                TotalMonto = totalMonto,
                MontoCuadre = montoCuadre,
                TotalFinal = totalMonto + montoCuadre
            };
        }

        // ─── Generación de Excel resultado ───────────────────────────────────────

        private byte[] GenerarExcel(InventarioResultadoDto resultado)
        {
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Comparación Inventario");

            int totalFilas = resultado.Items.Count;
            int ultimaFilaExcel = totalFilas + 1; // fila Excel (1-based) del último producto

            // ── Formatos numéricos
            var dataFormat = workbook.CreateDataFormat();
            short fmtInt = dataFormat.GetFormat("0");
            short fmtDec = dataFormat.GetFormat("0.##");

            var intStyle = workbook.CreateCellStyle();
            intStyle.DataFormat = fmtInt;

            var decStyle = workbook.CreateCellStyle();
            decStyle.DataFormat = fmtDec;

            // ── Fuente negrita reutilizada en cabeceras
            IFont boldFont = workbook.CreateFont();
            boldFont.IsBold = true;

            // ── Cabecera normal: solo negrita
            ICellStyle headerNormalStyle = workbook.CreateCellStyle();
            headerNormalStyle.SetFont(boldFont);

            // ── Función local: estilo negrita + color de relleno (fuente opcional con color)
            XSSFCellStyle CrearEstiloColorBold(byte r, byte g, byte b, IFont fuente = null)
            {
                var st = (XSSFCellStyle)workbook.CreateCellStyle();
                st.SetFont(fuente ?? boldFont);
                st.FillPattern = FillPattern.SolidForeground;
                st.SetFillForegroundColor(new XSSFColor(new byte[] { r, g, b }));
                return st;
            }

            // ── Fuentes de color para celdas H1/J1/L1 y columnas E/G
            XSSFFont CrearFuenteColorBold(byte r, byte g, byte b)
            {
                var f = (XSSFFont)workbook.CreateFont();
                f.IsBold = true;
                f.SetColor(new XSSFColor(new byte[] { r, g, b }));
                return f;
            }

            XSSFFont CrearFuenteColor(byte r, byte g, byte b)
            {
                var f = (XSSFFont)workbook.CreateFont();
                f.SetColor(new XSSFColor(new byte[] { r, g, b }));
                return f;
            }

            var fontRojoBold   = CrearFuenteColorBold(192,   0,   0);  // rojo intenso, negrita
            var fontAzulBold   = CrearFuenteColorBold(  0,  70, 127);  // azul intenso, negrita
            var fontRojoDatos  = CrearFuenteColor(192,   0,   0);      // rojo intenso
            var fontAzulDatos  = CrearFuenteColor(  0,  70, 127);      // azul intenso

            // ── Estilos para etiquetas I1/K1/M1 (fondo + negrita negra)
            var styleNaranja  = CrearEstiloColorBold(242, 145, 97);  // #F29161  I1
            var styleVerde    = CrearEstiloColorBold(102, 194, 23);  // #66C217  K1
            var styleAmarillo = CrearEstiloColorBold(240, 225, 24);  // #F0E118  M1

            // ── Estilos para valores H1/J1/L1: fondo + texto coloreado según signo
            var styleNaranjaRojo  = CrearEstiloColorBold(242, 145, 97, fontRojoBold);
            var styleNaranjaAzul  = CrearEstiloColorBold(242, 145, 97, fontAzulBold);
            var styleVerdeRojo    = CrearEstiloColorBold(102, 194, 23, fontRojoBold);
            var styleVerdeAzul    = CrearEstiloColorBold(102, 194, 23, fontAzulBold);
            var styleAmarilloRojo = CrearEstiloColorBold(240, 225, 24, fontRojoBold);
            var styleAmarilloAzul = CrearEstiloColorBold(240, 225, 24, fontAzulBold);

            // ── Estilos numéricos con color para columnas E (diferencia) y G (monto)
            ICellStyle CrearEstiloDatosColor(short fmt, XSSFFont fuente)
            {
                var st = (XSSFCellStyle)workbook.CreateCellStyle();
                st.DataFormat = fmt;
                st.SetFont(fuente);
                return st;
            }

            var intStyleRojo  = CrearEstiloDatosColor(fmtInt, fontRojoDatos);
            var decStyleRojo  = CrearEstiloDatosColor(fmtDec, fontRojoDatos);
            var intStyleVerde = CrearEstiloDatosColor(fmtInt, fontAzulDatos);
            var decStyleVerde = CrearEstiloDatosColor(fmtDec, fontAzulDatos);

            // ── Inmovilizar fila superior
            sheet.CreateFreezePane(0, 1);

            // ── Fila 1: cabeceras (NPOI row index 0)
            var headerRow = sheet.CreateRow(0);
            // Alineación izquierda para estilos de cabecera
            headerNormalStyle.Alignment   = HorizontalAlignment.Left;
            styleNaranja.Alignment        = HorizontalAlignment.Left;
            styleVerde.Alignment          = HorizontalAlignment.Left;
            styleAmarillo.Alignment       = HorizontalAlignment.Left;
            styleNaranjaRojo.Alignment    = HorizontalAlignment.Left;
            styleNaranjaAzul.Alignment    = HorizontalAlignment.Left;
            styleVerdeRojo.Alignment      = HorizontalAlignment.Left;
            styleVerdeAzul.Alignment      = HorizontalAlignment.Left;
            styleAmarilloRojo.Alignment   = HorizontalAlignment.Left;
            styleAmarilloAzul.Alignment   = HorizontalAlignment.Left;

            string[] headers = { "Codigo", "Producto", "total fisico", "total sistema", "diferencia", "precio", "monto" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = headerRow.CreateCell(i);
                cell.SetCellValue(headers[i]);
                cell.CellStyle = headerNormalStyle;
            }

            // H1 (col 7): sumatoria del monto = SUM(G2:Gn)
            var sumaCell = headerRow.CreateCell(7);
            sumaCell.SetCellFormula($"SUM(G2:G{ultimaFilaExcel})");
            sumaCell.CellStyle = resultado.TotalMonto < 0 ? styleNaranjaRojo : styleNaranjaAzul;

            // I1 (col 8): etiqueta
            var labelSuma = headerRow.CreateCell(8);
            labelSuma.SetCellValue("Suma Monto");
            labelSuma.CellStyle = styleNaranja;

            // J1 (col 9): monto cuadre (valor editable)
            var cuadreCell = headerRow.CreateCell(9);
            cuadreCell.SetCellValue((double)resultado.MontoCuadre);
            cuadreCell.CellStyle = resultado.MontoCuadre < 0 ? styleVerdeRojo : styleVerdeAzul;

            // K1 (col 10): etiqueta
            var labelCuadre = headerRow.CreateCell(10);
            labelCuadre.SetCellValue("Monto Cuadre");
            labelCuadre.CellStyle = styleVerde;

            // L1 (col 11): Total Final = H1+J1
            var totalFinalCell = headerRow.CreateCell(11);
            totalFinalCell.SetCellFormula("H1+J1");
            totalFinalCell.CellStyle = resultado.TotalFinal < 0 ? styleAmarilloRojo : styleAmarilloAzul;

            // M1 (col 12): etiqueta
            var labelTotal = headerRow.CreateCell(12);
            labelTotal.SetCellValue("Total");
            labelTotal.CellStyle = styleAmarillo;

            // ── Filas de datos (NPOI row index 1 en adelante)
            for (int i = 0; i < totalFilas; i++)
            {
                var item = resultado.Items[i];
                var row = sheet.CreateRow(i + 1);
                int excelRow = i + 2; // fila Excel para fórmulas (cabecera=1, datos desde 2)

                row.CreateCell(0).SetCellValue(item.CodigoBarras);
                row.CreateCell(1).SetCellValue(item.Articulo);

                // C: total fisico
                var cTotal = row.CreateCell(2);
                cTotal.SetCellValue((double)item.TotalFisico);
                cTotal.CellStyle = EsEntero(item.TotalFisico) ? intStyle : decStyle;

                // D: total sistema
                var cSistema = row.CreateCell(3);
                cSistema.SetCellValue((double)item.TotalSistema);
                cSistema.CellStyle = EsEntero(item.TotalSistema) ? intStyle : decStyle;

                // E: diferencia = C-D (fórmula)
                var cDiff = row.CreateCell(4);
                cDiff.SetCellFormula($"C{excelRow}-D{excelRow}");
                cDiff.CellStyle = item.Diferencia < 0
                    ? (EsEntero(item.Diferencia) ? intStyleRojo  : decStyleRojo)
                    : (EsEntero(item.Diferencia) ? intStyleVerde : decStyleVerde);

                // F: precio
                var cPrecio = row.CreateCell(5);
                cPrecio.SetCellValue((double)item.Precio);
                cPrecio.CellStyle = EsEntero(item.Precio) ? intStyle : decStyle;

                // G: monto = E*F (fórmula)
                var cMonto = row.CreateCell(6);
                cMonto.SetCellFormula($"E{excelRow}*F{excelRow}");
                cMonto.CellStyle = item.Monto < 0
                    ? (EsEntero(item.Monto) ? intStyleRojo  : decStyleRojo)
                    : (EsEntero(item.Monto) ? intStyleVerde : decStyleVerde);
            }

            // ── Auto-ajustar ancho + margen de relleno
            for (int i = 0; i < 13; i++)
            {
                sheet.AutoSizeColumn(i);
                sheet.SetColumnWidth(i, sheet.GetColumnWidth(i) + 1024);
            }
            // Columna B (índice 1): ancho para nombres de producto
            sheet.SetColumnWidth(1, 7500);
            // Columnas de valores numéricos H(7), J(9), L(11)
            sheet.SetColumnWidth(7,  3500);
            sheet.SetColumnWidth(9,  3500);
            sheet.SetColumnWidth(11, 3500);
            // Columnas de etiquetas I(8), K(10), M(12): ajustadas al texto corto
            sheet.SetColumnWidth(8,  3800);
            sheet.SetColumnWidth(10, 3800);
            sheet.SetColumnWidth(12, 2500);

            using var ms = new MemoryStream();
            workbook.Write(ms);
            return ms.ToArray();
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        private static bool EsEntero(decimal valor) => valor == Math.Floor(valor);

        private static string NormalizarCabecera(string s) =>
            s?.Trim().Normalize(NormalizationForm.FormC).ToLowerInvariant();

        private static IWorkbook AbrirWorkbook(string extension, Stream stream)
        {
            return string.Equals(extension, ".xls", StringComparison.OrdinalIgnoreCase)
                ? new HSSFWorkbook(stream)
                : (IWorkbook)new XSSFWorkbook(stream);
        }

        private static string ObtenerValorCelda(ICell cell)
        {
            if (cell == null) return null;
            return cell.CellType switch
            {
                CellType.Numeric => cell.NumericCellValue.ToString(System.Globalization.CultureInfo.InvariantCulture),
                CellType.String => cell.StringCellValue,
                CellType.Formula => cell.CachedFormulaResultType == CellType.Numeric
                    ? cell.NumericCellValue.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    : cell.StringCellValue,
                CellType.Boolean => cell.BooleanCellValue.ToString(),
                _ => string.Empty
            };
        }

        // ─── Modelos internos ─────────────────────────────────────────────────────

        private class FisicoItem
        {
            public string Articulo { get; set; }
            public string CodigoBarras { get; set; }
            public decimal Cantidad { get; set; }
        }

        private class FisicoConsolidado
        {
            public string Articulo { get; set; }
            public string CodigoBarras { get; set; }
            public decimal Cantidad { get; set; }
        }

        private class SistemaItem
        {
            public string CodInterno { get; set; }
            public string Nombre { get; set; }
            public decimal StockActual { get; set; }
            public decimal PrecioVenta { get; set; }
        }
    }
}
