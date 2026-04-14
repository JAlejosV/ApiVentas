/*
// Ejemplo de configuración usando variables de entorno
// En Program.cs, puedes agregar esta configuración:

builder.Services.Configure<IBMCloudStorageOptions>(options =>
{
    options.ServiceUrl = Environment.GetEnvironmentVariable("IBM_SERVICE_URL") 
        ?? builder.Configuration["IBMCloudStorage:ServiceUrl"];
    options.AccessKey = Environment.GetEnvironmentVariable("IBM_ACCESS_KEY") 
        ?? builder.Configuration["IBMCloudStorage:AccessKey"];
    options.SecretKey = Environment.GetEnvironmentVariable("IBM_SECRET_KEY") 
        ?? builder.Configuration["IBMCloudStorage:SecretKey"];
    options.BucketName = Environment.GetEnvironmentVariable("IBM_BUCKET_NAME") 
        ?? builder.Configuration["IBMCloudStorage:BucketName"];
});
*/