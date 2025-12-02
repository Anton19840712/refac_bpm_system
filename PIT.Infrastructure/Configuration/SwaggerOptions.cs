namespace PIT.Infrastructure.Configuration
{
    /// <summary>
    /// Настройки Swagger/OpenAPI
    /// </summary>
    public sealed class SwaggerOptions
    {
        public const string SectionName = "Swagger";
        /// <summary>
        /// Название API
        /// </summary>
        public string Title { get; set; } = "API";

        /// <summary>
        /// Описание API
        /// </summary>
        public string Description { get; set; } = "API Documentation";

        /// <summary>
        /// Версия API
        /// </summary>
        public string Version { get; set; } = "v1";

        /// <summary>
        /// Контактная информация
        /// </summary>
        public SwaggerContactOptions Contact { get; set; } = new();

        /// <summary>
        /// Информация о лицензии
        /// </summary>
        public SwaggerLicenseOptions License { get; set; } = new();

        /// <summary>
        /// Включить XML комментарии
        /// </summary>
        public bool IncludeXmlComments { get; set; } = true;

        /// <summary>
        /// Включить примеры Problem Details
        /// </summary>
        public bool IncludeProblemDetailsExamples { get; set; } = true;

        /// <summary>
        /// Включить OAuth2/JWT настройки
        /// </summary>
        public bool EnableJwtAuthentication { get; set; } = false;

        /// <summary>
        /// Включить добавить OpenAPI документ от библиотеки Scalar
        /// </summary>
        public bool EnableScalar { get; set; } = false;

        /// <summary>
        /// Endpoint для Swagger JSON
        /// </summary>
        public string RoutePrefix { get; set; } = "swagger";
    }

    public sealed class SwaggerContactOptions
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public sealed class SwaggerLicenseOptions
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
