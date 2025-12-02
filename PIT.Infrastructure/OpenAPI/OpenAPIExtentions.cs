using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using PIT.Infrastructure.Configuration;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;

namespace PIT.Infrastructure.OpenAPI
{
    public static class OpenAPIExtentions
    {
        public static IServiceCollection AddOpenApi(
            this IServiceCollection services,
            IOptions<InfrastructureOptions> options)
        {
            SwaggerOptions swaggerOptions = options.Value.Swagger;
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(swaggerOptions.Version, new OpenApiInfo { Title = "Менеджер Процессов", Version = swaggerOptions.Version });
                // Для моделей - использующих наследование
                c.UseAllOfForInheritance();

                c.SelectSubTypesUsing(baseType =>
                {
                    return Assembly.GetEntryAssembly()?.GetTypes().Where(type => type.IsSubclassOf(baseType));
                });
                c.SelectDiscriminatorNameUsing((baseType) => "TypeName");
                c.SelectDiscriminatorValueUsing((subType) => subType.Name);


                if (swaggerOptions.IncludeXmlComments)
                {
                    string xmlFile = $"{Assembly.GetEntryAssembly()?.GetName().Name}.xml";
                    string xmlFilePath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    c.IncludeXmlComments(
                                            filePath: xmlFilePath,
                                            includeControllerXmlComments: true
                                        );
                }
                c.OrderActionsBy((apiDesc) => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");
                c.OperationFilter<AuthResponsesOperationFilter>();
                c.SchemaFilter<AutoRestSchemaFilter>();
                c.SchemaFilter<DictionaryTKeyEnumTValueSchemaFilter>();

                if (swaggerOptions.EnableJwtAuthentication)
                {
                    // Добавление определения безопасности для JWT Bearer токена
                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Вставьте jwt token"
                    });
                    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("bearer", document)] = []
                    });
                }
            });
            return services;
        }

        public static WebApplication UseOpenApi(
            this WebApplication app,
            IOptions<InfrastructureOptions> options)
        {
            SwaggerOptions swaggerOptions = options.Value.Swagger;
            app.UseSwagger(options =>
            {
                options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
            });
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                if (swaggerOptions.EnableScalar)
                {
                    endpoints.MapScalarApiReference(
                        endpointPrefix: "/scalar/",
                        options =>
                        {
                            options.DotNetFlag = true;
                            options.ShowSidebar = true;
                            options.OpenApiRoutePattern = "/swagger/{documentName}/swagger.json/";
                        }
                    );
                }
            });
            app.UseSwaggerUI(options =>
            {
                options.DefaultModelExpandDepth(2);
                options.DefaultModelRendering(ModelRendering.Model);
                options.DefaultModelsExpandDepth(-1);
                options.DisplayOperationId();
                options.DisplayRequestDuration();
                options.DocExpansion(DocExpansion.None);
                options.EnableDeepLinking();
                options.EnableFilter();
                options.EnablePersistAuthorization();
                options.EnableTryItOutByDefault();
                options.ShowExtensions();
                options.ShowCommonExtensions();
                options.EnableValidator();
                options.SupportedSubmitMethods(SubmitMethod.Get, SubmitMethod.Head);
                options.UseRequestInterceptor("(request) => { return request; }");
                options.UseResponseInterceptor("(response) => { return response; }");
            });
            app.UseStaticFiles();            
            return app;
        }
    }
}
