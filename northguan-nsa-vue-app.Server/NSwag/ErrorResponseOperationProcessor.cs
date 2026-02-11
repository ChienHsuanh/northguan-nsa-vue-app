using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using northguan_nsa_vue_app.Server.DTOs;
using NJsonSchema;
using Namotion.Reflection;

namespace northguan_nsa_vue_app.Server.NSwag
{
    /// <summary>
    /// NSwag 操作處理器，為所有 API 操作添加統一的錯誤回應文檔
    /// </summary>
    public class ErrorResponseOperationProcessor : IOperationProcessor
    {
        public bool Process(OperationProcessorContext context)
        {
            var operation = context.OperationDescription.Operation;

            // 添加 400 Bad Request (驗證錯誤)
            if (!operation.Responses.ContainsKey("400"))
            {
                var response400 = new OpenApiResponse
                {
                    Description = "驗證失敗或請求格式錯誤"
                };
                response400.Content["application/json"] = new OpenApiMediaType
                {
                    Schema = GetOrAddRefSchema(typeof(ApiErrorResponse), context)
                };
                operation.Responses["400"] = response400;
            }

            // 添加 500 Internal Server Error (系統錯誤)
            if (!operation.Responses.ContainsKey("500"))
            {
                var response500 = new OpenApiResponse
                {
                    Description = "系統內部錯誤"
                };
                response500.Content["application/json"] = new OpenApiMediaType
                {
                    Schema = GetOrAddRefSchema(typeof(ApiErrorResponse), context)
                };
                operation.Responses["500"] = response500;
            }

            return true;
        }

        private JsonSchema GetOrAddRefSchema(Type type, OperationProcessorContext context)
        {
            var schemaName = context.SchemaGenerator.Settings.SchemaNameGenerator.Generate(type);
            if (!context.Document.Definitions.ContainsKey(schemaName))
            {
                var ctxType = type.ToContextualType();
                var schema = context.SchemaGenerator.GenerateWithReferenceAndNullability<JsonSchema>(ctxType, false, context.SchemaResolver);
                return schema;
            }

            var actualSchema = context.Document.Definitions[schemaName];

            var refSchema = new JsonSchema() { Reference = actualSchema };
            return refSchema;
        }
    }
}