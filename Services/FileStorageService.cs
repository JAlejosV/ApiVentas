using Microsoft.Extensions.Options;

namespace ApiVentas.Services
{
    public enum StorageProvider
    {
        Local,
        Azure,
        AWS
    }

    public class FileStorageSettings
    {
        public StorageProvider Provider { get; set; } = StorageProvider.Local;
        public LocalStorageSettings LocalSettings { get; set; } = new();
        public AzureStorageSettings AzureSettings { get; set; } = new();
    }

    public class LocalStorageSettings
    {
        public string BasePath { get; set; } = "wwwroot/archivos";
        public string WebPath { get; set; } = "/archivos";
        public int MaxFileSizeMB { get; set; } = 15;
        public string[] AllowedExtensions { get; set; } = { ".pdf", ".doc", ".docx", ".jpg", ".png" };
    }

    public class AzureStorageSettings
    {
        public string ConnectionString { get; set; } = "";
        public string ContainerName { get; set; } = "proveedores";
    }

    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, int providerId);
        Task<bool> DeleteFileAsync(string filePath);
        Task<Stream> GetFileStreamAsync(string filePath);
        string GetFileUrl(string filePath);
        bool ValidateFile(IFormFile file);
    }

    public class LocalFileStorageService : IFileStorageService
    {
        private readonly FileStorageSettings _options;
        private readonly IWebHostEnvironment _environment;

        public LocalFileStorageService(IOptions<FileStorageSettings> options, IWebHostEnvironment environment)
        {
            _options = options.Value;
            _environment = environment;
        }

        public async Task<string> SaveFileAsync(IFormFile file, int providerId)
        {
            // Usar ruta configurable fuera de wwwroot para mayor seguridad
            var uploadsPath = Path.Combine(_options.LocalSettings.BasePath, "proveedores");
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Retornar ruta relativa para la BD
            return Path.Combine("proveedores", fileName).Replace("\\", "/");
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_options.LocalSettings.BasePath, filePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }
            return false;
        }

        public async Task<Stream> GetFileStreamAsync(string filePath)
        {
            var fullPath = Path.Combine(_options.LocalSettings.BasePath, filePath);
            if (File.Exists(fullPath))
            {
                return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            }
            throw new FileNotFoundException($"Archivo no encontrado: {filePath}");
        }

        public string GetFileUrl(string filePath)
        {
            return $"{_options.LocalSettings.WebPath}/{filePath.Replace("\\", "/")}";
        }

        public bool ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // Validar tamaño
            var maxSize = _options.LocalSettings.MaxFileSizeMB * 1024 * 1024;
            if (file.Length > maxSize)
                return false;

            // Validar extensión
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return _options.LocalSettings.AllowedExtensions.Contains(extension);
        }
    }
}
