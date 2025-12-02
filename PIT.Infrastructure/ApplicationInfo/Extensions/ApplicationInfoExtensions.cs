using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIT.Infrastructure.ApplicationInfo.Extensions
{
    /// <summary>
    /// Расширения для регистрации информации о приложении
    /// </summary>
    public static class ApplicationInfoExtensions
    {
        /// <summary>
        /// Добавляет провайдер информации о приложении
        /// </summary>
        public static IServiceCollection AddPitApplicationInfo(this IServiceCollection services)
        {
            services.AddSingleton<ApplicationInfoProvider>();
            services.AddHostedService<StartupBanner>();
            return services;
        }

        /// <summary>
        /// Добавляет endpoint для получения информации о приложении
        /// </summary>
        public static IEndpointRouteBuilder MapPitApplicationInfo(
            this IEndpointRouteBuilder endpoints,
            string pattern = "/info")
        {
            endpoints.MapGet(pattern, async (ApplicationInfoProvider provider, CancellationToken cancellationToken) =>
            {
                var info = await provider.GetApplicationInfoAsync(cancellationToken);
                return Results.Ok(info);
            })
            .WithName("ApplicationInfo")
            .WithTags("Infrastructure")
            .Produces<Models.ApplicationInfoModel>(StatusCodes.Status200OK);

            return endpoints;
        }
    }
}
