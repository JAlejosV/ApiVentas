using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApiVentas.Filters
{
    /// <summary>
    /// Filtro de Swagger que corrige la representación de List&lt;IFormFile&gt;
    /// para que Swagger UI permita seleccionar múltiples archivos en el mismo campo.
    /// </summary>
    public class MultiFileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var parameters = context.MethodInfo.GetParameters();
            bool tieneListaArchivos = parameters.Any(p =>
                p.ParameterType == typeof(List<Microsoft.AspNetCore.Http.IFormFile>));

            if (!tieneListaArchivos) return;

            // Reconstruimos el requestBody con el tipo array para los campos List<IFormFile>
            var properties = new Dictionary<string, OpenApiSchema>();
            var required = new HashSet<string>();

            foreach (var param in parameters)
            {
                if (param.ParameterType == typeof(List<Microsoft.AspNetCore.Http.IFormFile>))
                {
                    properties[param.Name] = new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema { Type = "string", Format = "binary" },
                        Description = "Uno o varios archivos .xls / .xlsx"
                    };
                    required.Add(param.Name);
                }
                else if (param.ParameterType == typeof(Microsoft.AspNetCore.Http.IFormFile))
                {
                    properties[param.Name] = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary",
                        Description = "Archivo .xls / .xlsx"
                    };
                    required.Add(param.Name);
                }
                else if (param.ParameterType == typeof(decimal) || param.ParameterType == typeof(decimal?))
                {
                    bool esOpcional = param.HasDefaultValue;
                    properties[param.Name] = new OpenApiSchema
                    {
                        Type = "number",
                        Format = "decimal",
                        Nullable = esOpcional,
                        Description = "Número decimal (positivo o negativo)",
                        Default = esOpcional
                            ? new Microsoft.OpenApi.Any.OpenApiDouble(Convert.ToDouble(param.DefaultValue ?? 0))
                            : null
                    };
                    if (!esOpcional)
                        required.Add(param.Name);
                }
                else if (param.ParameterType == typeof(int) || param.ParameterType == typeof(long) ||
                         param.ParameterType == typeof(double) || param.ParameterType == typeof(float))
                {
                    properties[param.Name] = new OpenApiSchema { Type = "number" };
                    if (!param.HasDefaultValue)
                        required.Add(param.Name);
                }
                else if (param.ParameterType == typeof(string))
                {
                    properties[param.Name] = new OpenApiSchema { Type = "string" };
                    if (!param.HasDefaultValue)
                        required.Add(param.Name);
                }
            }

            if (properties.Count == 0) return;

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = properties,
                            Required = required
                        }
                    }
                }
            };
        }
    }
}
