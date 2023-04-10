using FSH.WebApi.Infrastructure.Common.Settings;
using FSH.WebApi.Infrastructure.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;

namespace FSH.WebApi.Infrastructure.Cors {

    internal static class Startup {

        private static CorsSettings CorsSettings { get; set; }
        private static string CorsPolicy = nameof(CorsPolicy);
        private static readonly ILogger _logger = Log.ForContext(typeof(Startup));

        internal static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration config) {

            CorsSettings = config.GetSection(nameof(CorsSettings)).Get<CorsSettings>();

            if (config.GetSection("FeatureFlagSettings").GetSection("Cors").Value == "True" && Runtime.CDCI.FrontendContainer?.HostName != null) {
                var frontendUrlList = new List<string> { Runtime.CDCI.FrontendContainer.HostName };
                if (Runtime.CDCI.ThisEnvironment.Name == "dev") {
                    frontendUrlList.Add(CorsSettings.LocalhostFrontendUrl);
                }

                _logger.Information($"FRONTEND_URL ADDED TO CORS:");

                foreach (var item in frontendUrlList)
                {
                    _logger.Information($"url:{item}");
                }
                
                return services.AddCors(opt =>
                    opt.AddPolicy(CorsPolicy, policy =>
                        policy.AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials()
                            .WithOrigins(frontendUrlList.ToArray())
                            .Build()
                            ));
            }
            else {
                _logger.Information($"CORS ALLOWING ANY HOST");
                return services.AddCors(opt =>
                    opt.AddPolicy(CorsPolicy, policy =>
                        policy.AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials()
                            .SetIsOriginAllowed(origin => true) // allow any origin
                            .Build()
                            ));
            }
        }

        internal static IApplicationBuilder UseCorsPolicy(this IApplicationBuilder app, IConfiguration config) {
            return app.UseCors(CorsPolicy);
        }
    }
}