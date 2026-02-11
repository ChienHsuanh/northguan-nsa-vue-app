using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using northguan_nsa_vue_app.Server.DTOs;
using NJsonSchema;
using Namotion.Reflection;

namespace northguan_nsa_vue_app.Server.NSwag
{
    /// <summary>
    /// 簡化的錯誤回應處理器，只為特定操作添加錯誤回應
    /// </summary>
    public class SimpleErrorResponseProcessor : IOperationProcessor
    {
        public bool Process(OperationProcessorContext context)
        {
            var operation = context.OperationDescription.Operation;

            // 為特定操作添加標準錯誤回應，使用 ApiErrorResponse 類型
            AddErrorResponseWithSchema(context, operation, "400", "驗證失敗或請求格式錯誤");
            AddErrorResponseWithSchema(context, operation, "500", "系統內部錯誤");

            return true;
        }

        private void AddErrorResponseWithSchema(OperationProcessorContext context, OpenApiOperation operation, string statusCode, string description)
        {
            if (!operation.Responses.ContainsKey(statusCode))
            {
                var response = new OpenApiResponse
                {
                    Description = description
                };

                // 使用共享的 ApiErrorResponse schema 引用
                response.Content["application/json"] = new OpenApiMediaType
                {
                    Schema = GetOrAddRefSchema(typeof(ApiErrorResponse), context)
                };

                operation.Responses[statusCode] = response;
            }
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