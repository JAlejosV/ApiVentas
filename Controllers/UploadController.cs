using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace ApiVentas.Controllers
{
    [Route("api/upload")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Upload()
        {
            try
            {
                var file = Request.Form.Files[0];
                var folderName = Path.Combine("ImagenesPosts");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (file.Length > 0)
                {
                    var fileName = Guid.NewGuid() + ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(folderName + "/", fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {                        
                        file.CopyTo(stream);
                    }
                    return Ok(dbPath);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        [HttpPost("pedidos")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult UploadArchivoPedido()
        {
            try
            {
                var file = Request.Form.Files[0];
                var folderName = Path.Combine("ArchivosPedidos");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                // Crear el directorio si no existe
                if (!Directory.Exists(pathToSave))
                {
                    Directory.CreateDirectory(pathToSave);
                }

                if (file.Length > 0)
                {
                    var fileName = Guid.NewGuid() + "_" + file.FileName;
                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(folderName + "/", fileName);
                    
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {                        
                        file.CopyTo(stream);
                    }
                    return Ok(dbPath);
                }
                else
                {
                    return BadRequest("No se recibió archivo");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
    }
}
