using FSH.WebApi.Infrastructure.Common.Settings;
using FSH.WebApi.Infrastructure.Helpers;
using FSH.WebApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace FSH.WebApi.Infrastructure.CDCI {

    internal static class Startup {
        private static IConfiguration Config { get; set; }

        private static readonly ILogger _logger = Log.ForContext(typeof(Startup));
        private static string DatabaseInstanceEnvVariableShortID { get; set; }

        internal static IServiceCollection AddCDCI(this IServiceCollection services, IConfiguration config) {
            Config = config;

            // Assemble CDCI settings object from Qovery Environment Variables and AppSettings.json
            Runtime.CDCI = config.GetSection("CDCISettings").Get<CDCISettings>();
            Runtime.CDCI.Database = config.GetSection("DatabaseSettings").Get<DatabaseContainer>();
            Runtime.CDCI.QoveryAPIToken = Environment.GetEnvironmentVariable("API_TOKEN_QOVERY");
            Runtime.CDCI.QoveryAPIToken = "Token qov_7503rXkpWTKaBDbQXUOWJb3EXBMtGqRkVAmgsxOiA5nEWqJWyD0VvOe_1329524952";
            _logger.Information($"FOUND QOVERY TOKEN {Runtime.CDCI.QoveryAPIToken}");

            // RESOLVE DATABASE CREDENTIALS FROM ÉNV VARS INSTEAD OF USING JSON CONFIGURATION IF FLAG IS FALSE
            if (
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "LocalDevelopment"
                || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "AddivaDevelopment"
                || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "AbentorProd") {
                Runtime.CDCI.FrontendContainer = new QoveryContainer() { HostName = config.GetSection(nameof(CorsSettings)).Get<CorsSettings>().LocalhostFrontendUrl, Name = "localhost" };
                Runtime.CDCI.ThisEnvironment = new QoveryEnvironment { Name = "localhost" };
            }
            else {
                ResolveQoveryEnvironmentVariablesAndSecrets(); }

            Runtime.CDCI.Database.ConnectionString = $"Server={Runtime.CDCI.Database.HostName}:{Runtime.CDCI.Database.Port};Database={Runtime.CDCI.Database.DatabaseName};User ID={Runtime.CDCI.Database.Username};Password={Runtime.CDCI.Database.Password};Include Error Detail=True";

            return services;
        }

        private static void ResolveQoveryEnvironmentVariablesAndSecrets() {

            // FETCH THIS CONTAINER
            var qoveryContainerId = Environment.GetEnvironmentVariable("QOVERY_CONTAINER_ID");
            var container = new QoveryContainer { Id = qoveryContainerId };
            _logger.Information($"FOUND CONTAINER ID {qoveryContainerId}");
            Runtime.CDCI.ThisContainer = GetQoveryContainer(container.Id);
            Runtime.CDCI.ThisEnvironment = GetQoveryEnvironment(Runtime.CDCI.ThisContainer.Environment.Id);
            Runtime.CDCI.ThisContainer.Name = Runtime.CDCI.ThisContainerName;
            string shortContainerId = Runtime.CDCI.ThisContainer.Id.Substring(0, 8);
            if (container.Id != null) {

                var containerVariables = GetQoveryContainerVariables(Runtime.CDCI.ThisContainer.Id).results;

                var thisBackendVariables = containerVariables.Where(x => x.service_name == Runtime.CDCI.ThisContainerName).ToList();
                var databaseVariables = containerVariables.Where(x => x.service_name == Runtime.CDCI.DatabaseName).ToList();

                Runtime.CDCI.FrontendContainer = new QoveryContainer {
                    Name = Runtime.CDCI.ThisContainer.Name,
                    HostName = $"https://{Runtime.CDCI.ThisEnvironment.Name}-{Runtime.CDCI.ThisProjectName}.netlify.app"
                };

                foreach (var variable in thisBackendVariables) {
                    if (variable.key.EndsWith("HOST_INTERNAL")) {
                        Runtime.CDCI.ThisContainer.HostName = variable.value;
                    }
                }

                foreach (var variable in databaseVariables) {
                    // CHECK IF THIS IS A FEATURE BRANCH
                    // DEV DATABASE SETTINGS ARE HARDCODED FOR NOW, NOT RESOLVED.
                    //if (Runtime.CDCI.ThisEnvironment.Name != "dev" &&
                    //    Runtime.CDCI.ThisEnvironment.Name != "test" &&
                    //    Runtime.CDCI.ThisEnvironment.Name != "prod" && variable.key.EndsWith("HOST")) {
                    //    Runtime.CDCI.Database.UseHardCodedDBSettings = true;
                    //    Runtime.CDCI.Database.HostName = "z55dfc3ed-postgresql.zce5e20e8.jvm.world";
                    //    Runtime.CDCI.Database.Password = "wl8-Cx0Wz6kkZ4hc4avP8-vYxYxk6O8r";
                    //    // Using the DB HOST value to extract the dynamically set ID inside it and re-use for fetching password
                    //    var start = variable.key.IndexOf("QOVERY_POSTGRESQL_") + 18;
                    //    DatabaseInstanceEnvVariableShortID = variable.key.Substring(start, variable.key.IndexOf("_HOST") - start);
                    //}

                    // DEV/TEST/OTHER DB'S ARE ACCESSIBLE FROM PUBLIC INTERNET
                    if (Runtime.CDCI.ThisEnvironment.Name != "prod" && variable.key.EndsWith("HOST")) {
                        Runtime.CDCI.Database.HostName = variable.value;
                        // Using the DB HOST value to extract the dynamically set ID inside it and re-use for fetching password
                        var start = variable.key.IndexOf("QOVERY_POSTGRESQL_") + 18;
                        DatabaseInstanceEnvVariableShortID = variable.key.Substring(start, variable.key.IndexOf("_HOST") - start);
                    }

                    // PRODUCTION DATABASE IS NOT ACCESSIBLE FROM PUBLIC INTERNET
                    if (Runtime.CDCI.ThisEnvironment.Name == "prod" && variable.key.EndsWith("HOST_INTERNAL")) {
                        Runtime.CDCI.Database.HostName = variable.value;
                        // Using the DB HOST value to extract the dynamically set ID inside it and re-use for fetching password
                        var start = variable.key.IndexOf("QOVERY_POSTGRESQL_") + 18;
                        DatabaseInstanceEnvVariableShortID = variable.key.Substring(start, variable.key.IndexOf("_HOST_INTERNAL") - start);
                    }
                    var customFrontendProdUrl = Config.GetSection("CORSSettings").Get<CorsSettings>().NetlifyCustomUrl;
                    if (Runtime.CDCI.ThisEnvironment.Name.StartsWith("prod") && customFrontendProdUrl != String.Empty) {
                        Runtime.CDCI.FrontendContainer.HostName = customFrontendProdUrl;
                    }
                    if (variable.key.EndsWith("PORT")) {
                        Runtime.CDCI.Database.Port = variable.value;
                    }
                    if (variable.key.EndsWith("LOGIN")) {
                        Runtime.CDCI.Database.Username = variable.value;
                    }
                }

                if (!Runtime.CDCI.Database.UseHardCodedDBSettings) {
                    _logger.Information(DatabaseInstanceEnvVariableShortID);
                    Runtime.CDCI.Database.Password = Environment.GetEnvironmentVariable($"QOVERY_POSTGRESQL_{DatabaseInstanceEnvVariableShortID}_PASSWORD");
                    _logger.Information($"FOUND DB PASSWORD: {Runtime.CDCI.Database.Password}");
                }

                _logger.Information("FRONTEND CONTAINER:");
                _logger.Information(JsonConvert.SerializeObject(Runtime.CDCI.FrontendContainer, Formatting.Indented));

                _logger.Information("BACKEND CONTAINER:");
                _logger.Information(JsonConvert.SerializeObject(Runtime.CDCI.ThisContainer, Formatting.Indented));

                _logger.Information("DATABASE:");
                _logger.Information(JsonConvert.SerializeObject(Runtime.CDCI.Database, Formatting.Indented));

            }
        }

        public static QoveryEnvironment GetQoveryEnvironment(string environmentId) {
            var result = new RestClient(Runtime.CDCI.QoveryAPIUrl).TogglQoveryApiCall(RestSharp.Method.GET, $"/environment/{environmentId}", null, Runtime.CDCI.QoveryAPIToken).Result;
            return JsonConvert.DeserializeObject<QoveryEnvironment>(result.Content);
        }


        public static QoveryContainer GetQoveryContainer(string containerId) {
            var result = new RestClient(Runtime.CDCI.QoveryAPIUrl).TogglQoveryApiCall(RestSharp.Method.GET, $"/container/{containerId}", null, Runtime.CDCI.QoveryAPIToken).Result;
            return JsonConvert.DeserializeObject<QoveryContainer>(result.Content);
        }

        public static QoveryContainerVariablesResult GetQoveryContainerVariables(string containerId) {
            var result = new RestClient(Runtime.CDCI.QoveryAPIUrl).TogglQoveryApiCall(RestSharp.Method.GET, $"/container/{containerId}/environmentVariable", null, Runtime.CDCI.QoveryAPIToken).Result;
            return JsonConvert.DeserializeObject<QoveryContainerVariablesResult>(result.Content);
        }
    }
}
