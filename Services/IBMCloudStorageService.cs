using Amazon.S3;
using Amazon.S3.Model;
using System.Text;

namespace ApiVentas.Services
{
    public interface ICloudStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string fileName);
        Task<bool> DeleteFileAsync(string fileName);
        Task<Stream> DownloadFileAsync(string fileName);
        string GeneratePreSignedUrl(string fileName, TimeSpan expiration);
    }

    public class IBMCloudStorageService : ICloudStorageService
    {
        private readonly AmazonS3Client _s3Client;
        private readonly string _bucketName;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IBMCloudStorageService> _logger;

        public IBMCloudStorageService(IConfiguration configuration, ILogger<IBMCloudStorageService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _bucketName = _configuration["IBMCloudStorage:BucketName"];

            var config = new AmazonS3Config()
            {
                ServiceURL = _configuration["IBMCloudStorage:ServiceUrl"],
                ForcePathStyle = true,
                UseHttp = false
            };

            _s3Client = new AmazonS3Client(
                _configuration["IBMCloudStorage:AccessKey"],
                _configuration["IBMCloudStorage:SecretKey"],
                config
            );
        }

        public async Task<string> UploadFileAsync(IFormFile file, string fileName)
        {
            try
            {
                using var stream = file.OpenReadStream();
                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileName,
                    InputStream = stream,
                    ContentType = file.ContentType,
                    ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
                };

                var response = await _s3Client.PutObjectAsync(request);
                
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    _logger.LogInformation($"Archivo {fileName} subido exitosamente a IBM Cloud Object Storage");
                    return fileName;
                }

                throw new Exception($"Error subiendo archivo: {response.HttpStatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error subiendo archivo {fileName}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string fileName)
        {
            try
            {
                var request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileName
                };

                var response = await _s3Client.DeleteObjectAsync(request);
                
                if (response.HttpStatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    _logger.LogInformation($"Archivo {fileName} eliminado exitosamente de IBM Cloud Object Storage");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error eliminando archivo {fileName}: {ex.Message}");
                return false;
            }
        }

        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileName
                };

                var response = await _s3Client.GetObjectAsync(request);
                return response.ResponseStream;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error descargando archivo {fileName}: {ex.Message}");
                throw;
            }
        }

        public string GeneratePreSignedUrl(string fileName, TimeSpan expiration)
        {
            try
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _bucketName,
                    Key = fileName,
                    Expires = DateTime.UtcNow.Add(expiration),
                    Verb = HttpVerb.GET
                };

                return _s3Client.GetPreSignedURL(request);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generando URL pre-firmada para {fileName}: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            _s3Client?.Dispose();
        }
    }
}