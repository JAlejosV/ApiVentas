using Microsoft.OpenApi;
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
            var properties = new Dictionary<string, IOpenApiSchema>();
            var required = new HashSet<string>();

            foreach (var param in parameters)
            {
                if (param.ParameterType == typeof(List<Microsoft.AspNetCore.Http.IFormFile>))
                {
                    properties[param.Name] = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Array,
                        Items = new OpenApiSchema { Type = JsonSchemaType.String, Format = "binary" },
                        Description = "Uno o varios archivos .xls / .xlsx"
                    };
                    required.Add(param.Name);
                }
                else if (param.ParameterType == typeof(Microsoft.AspNetCore.Http.IFormFile))
                {
                    properties[param.Name] = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
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
                        Type = esOpcional ? JsonSchemaType.Number | JsonSchemaType.Null : JsonSchemaType.Number,
                        Format = "decimal",
                        Description = "Número decimal (positivo o negativo)",
                        Default = esOpcional
                            ? System.Text.Json.Nodes.JsonValue.Create(Convert.ToDouble(param.DefaultValue ?? 0))
                            : null
                    };
                    if (!esOpcional)
                        required.Add(param.Name);
                }
                else if (param.ParameterType == typeof(int) || param.ParameterType == typeof(long) ||
                         param.ParameterType == typeof(double) || param.ParameterType == typeof(float))
                {
                    properties[param.Name] = new OpenApiSchema { Type = JsonSchemaType.Number };
                    if (!param.HasDefaultValue)
                        required.Add(param.Name);
                }
                else if (param.ParameterType == typeof(string))
                {
                    properties[param.Name] = new OpenApiSchema { Type = JsonSchemaType.String };
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
                            Type = JsonSchemaType.Object,
                            Properties = properties,
                            Required = required
                        }
                    }
                }
            };
        }
    }
}
