using System.Net;
using FSH.Core.Common;
using FSH.Core.Dto.CDCI;
using FSH.WebApi.Infrastructure.Common.Settings;
using FSH.WebApi.Infrastructure.Helpers;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RestSharp;
using Serilog;
using static Google.Rpc.Context.AttributeContext.Types;

namespace FSH.Infrastructure.Auth;

public class CDCIService : ICDCIService {

    private readonly ILogger Logger;
    private QoveryEnvironmentResult NewEnvironment { get; set; }
    private QoveryContainerResult NewContainer { get; set; }
    private string LatestProductionTag { get; set; }

    public CDCIService(IConfiguration config, ILogger logger) {
        Logger = logger;
        Runtime.CDCI = config.GetSection("CDCI").Get<CDCISettings>();
    }

    public async Task<CreateBackendResponse> CreateCustomerBackend(CreateBackendRequest request, CancellationToken cancellationToken) {

        //#######################################################################
        //### PIPELINE CONFIGURATON AND VARIABLES
        //#######################################################################

        //## THESE VALUES NEEDS TO BE CONFIGURED FOR EACH PROJECT
        var projectId = "9d0a2ead-2060-47ff-a0fd-1eebaeb2e0f4"; // settings
        var registryId = "bd834fff-5eb0-4105-992a-e8bbb7cdac24"; // settings

        //## CONTAINER RESOURCES, 1 CPU = 1000, RAM = Megabytes
        var containerCpu = 250;
        var containerRAM = 256;
        var containerInternalPort = 5060;
        var containerExternalPort = 443;
        var minNodes = 1;
        var maxNodes = 1;

        //## DO NOT CHANGE THESE VALUES
        var qoveryToken = "Token qov_7503rXkpWTKaBDbQXUOWJb3EXBMtGqRkVAmgsxOiA5nEWqJWyD0VvOe_1329524952"; // settings
        var devopsPAT = "ceetfh5ykqvjfg36t5v3ofzdp2him4vffji6qfd2hfjhzl7tb6ma"; // settings
        var clusterId = "ce5e20e8-fb97-441a-9a54-3c0dcdf8922b"; // settings
        var prodContainerImageName = "dashboard-backend-prod";


        //#######################################################################
        //### GET LATEST PRODUCTION TAG
        //#######################################################################
        LatestProductionTag = await GetLatestProductionTag();

        //#######################################################################
        //### CHECK IF ENVIRONMENT EXISTS
        //#######################################################################

        try {
            var result = await new APIClient(Runtime.CDCI.QoveryAPIUrl).QoveryApiCall(Method.GET, $"project/$(projectId)/environment", null, Runtime.CDCI.QoveryAPIToken);
            var environments = JsonConvert.DeserializeObject<List<QoveryEnvironmentResult>>(result.Content);

            foreach (var env in environments) {
                if (env.Name == request.TenantName) {
                    Log.Debug($"{env.Name} - {env.Id}");
                    Log.Debug("FOUND EXISTING ENVIRONMENT FOR BRANCH: $(branchName) - ENVIRONMENT ID: $environmentId");
                    return new CreateBackendResponse { EnvironmentExists = true };
                }
            }
        }
        catch (Exception e) { Log.Debug(e.Message); if (e.InnerException != null) Log.Debug(e.InnerException.Message); }

        //#######################################################################
        //### IF IT DOES NOT EXIST, CREATE A NEW PROJECT ENVIRONMENT
        //#######################################################################
        Log.Debug($"COULD NOT FIND AN EXISTING ENVIRONMENT CALLED {request.TenantName}... PROCEEDING");

        var newEnv = new QoveryEnvironmentRequest {
            Name = request.TenantName,
            Cluster = clusterId,
            Mode = "PRODUCTION"
        };

        try {
            var createEnvResult = await new APIClient(Runtime.CDCI.QoveryAPIUrl).QoveryApiCall(Method.POST, $"project/$(projectId)/environment", null, Runtime.CDCI.QoveryAPIToken);
            NewEnvironment = JsonConvert.DeserializeObject<QoveryEnvironmentResult>(createEnvResult.Content);
            Log.Debug(JsonConvert.SerializeObject(NewEnvironment, Formatting.Indented));
        }
        catch(Exception e) { Log.Debug(e.Message); if (e.InnerException != null) Log.Debug(e.InnerException.Message); }

        //#######################################################################
        //### CREATE A NEW BACKEND CONTAINER FROM LATEST DOCKER IMAGE IN AWS
        //#######################################################################

        Log.Debug($"CREATiNG A NEW BACKEND CONTAINER FROM LATEST DOCKER IMAGE IN AWS");

        var newContainer = new QoveryContainerRequest
        {
            Name = request.TenantName,
            Registry_id = registryId,
            Image_name = prodContainerImageName,
            Tag = LatestProductionTag,
            Cpu = containerCpu,
            Memory = containerRAM,
            Min_running_instances = minNodes,
            Max_running_instances = maxNodes,
            Ports = new List<QoveryContainerPort> { new QoveryContainerPort {
                    Name = "api",
                    Internal_port = containerInternalPort,
                    External_port = containerExternalPort,
                    Publicly_accessible = true,
                    Is_default = true,
                    Protocol = "HTTP"
                }
            }
        };

        try {
            var createContainerResult = await new APIClient(Runtime.CDCI.QoveryAPIUrl).QoveryApiCall(Method.POST, $"environment/{NewEnvironment.Id})/container", JsonConvert.SerializeObject(newContainer), Runtime.CDCI.QoveryAPIToken);
            NewContainer = JsonConvert.DeserializeObject<QoveryContainerResult>(createContainerResult.Content);
            Log.Debug(JsonConvert.SerializeObject(NewContainer, Formatting.Indented));
        }
        catch (Exception e) { Log.Debug(e.Message); if (e.InnerException != null) Log.Debug(e.InnerException.Message); }



        //#########################################################################
        //### CHECK CONTAINER STATE, WAIT FOR IT TO BECOME AVAILABLE
        //#########################################################################

        Log.Debug($"CHECK CONTAINER STATE, WAIT FOR IT TO BECOME AVAILABLE");

        try {

            Task.Run(async () => {
                string environmentState = "";
                while (environmentState != "DEPLOYING" || environmentState != "BUILDING" || environmentState != "CANCELING" || environmentState != "DELETING" || environmentState != "DELETE_QUEUED")
                {
                    try
                    {
                        Log.Debug($"GET CONTAINER STATE for {NewContainer.Id}");
                        var createEnvResult = await new APIClient(Runtime.CDCI.QoveryAPIUrl).QoveryApiCall(Method.GET, $"environment/{NewEnvironment.Id})/status", null, Runtime.CDCI.QoveryAPIToken);
                        environmentState = JsonConvert.DeserializeObject<QoveryEnvironmentStatusResult>(createEnvResult.Content).State;
                        Log.Debug($"CONTAINER STATE: {environmentState}");
                        if (environmentState == "DEPLOYING" || environmentState == "BUILDING" || environmentState == "CANCELING" || environmentState == "DELETING" || environmentState == "DELETE_QUEUED") {
                            Log.Debug("CONTAINER STATE IS $environmentState. LET'S WAIT 15 SECONDS AND TRY AGAIN"
                        }
                        Thread.Sleep(15000);
                    }
                    catch (Exception e)
                    {
                        Log.Debug(e.Message);
                        if (e.InnerException != null) Log.Debug(e.InnerException.Message);
                    }
                }
            });
        }
        catch (Exception e) { Log.Debug(e.Message); if (e.InnerException != null) Log.Debug(e.InnerException.Message); }



        //#######################################################################
        //### SET ENVIRONMENT VARIABLES AND ALIASES
        //#######################################################################

        //Log.Debug($"CREATiNG A NEW BACKEND CONTAINER FROM LATEST DOCKER IMAGE IN AWS");

        //var newContainer = new QoveryContainerRequest
        //{
        //    Name = request.TenantName,
        //    Registry_id = registryId,
        //    Image_name = prodContainerImageName,
        //    Tag = GetLatestProductionTag(),
        //    Cpu = containerCpu,
        //    Memory = containerRAM,
        //    Min_running_instances = minNodes,
        //    Max_running_instances = maxNodes,
        //    Ports = new List<QoveryContainerPort> { new QoveryContainerPort {
        //            Name = "api",
        //            Internal_port = containerInternalPort,
        //            External_port = containerExternalPort,
        //            Publicly_accessible = true,
        //            Is_default = true,
        //            Protocol = "HTTP"
        //        }
        //    }
        //};

        //try
        //{
        //    var createEnvResult = await new APIClient(Runtime.CDCI.QoveryAPIUrl).QoveryApiCall(Method.POST, $"container/$(containerId)/secret", JsonConvert.SerializeObject(newContainer), Runtime.CDCI.QoveryAPIToken);
        //    var environment = JsonConvert.DeserializeObject<QoveryEnvironmentResult>(createEnvResult.Content);
        //    Log.Debug(JsonConvert.SerializeObject(environment, Formatting.Indented));
        //}
        //catch (Exception e) { Log.Debug(e.Message); if (e.InnerException != null) Log.Debug(e.InnerException.Message); }


        //if condition: ne(variables['containerId'], 'create_container')

        //        Log.Debug-Host "SET QOVERY API SECRET for $(containerId)"

        //        var $url = "https://api.qovery.com/container/$(containerId)/secret"
        //        var $head = @{ Authorization = "$(qoveryToken)" }
        //var $body = @{
        //            "key" = "API_TOKEN_QOVERY"
        //          "value" = "$(qoveryToken)"
        //        } | ConvertTo - Json
        //        Log.Debug($body

        //        try
        //        {
        //            Log.Debug($url
        //            $secretResponse = Invoke - RestMethod - Uri $url - Method Post - Body $body - Headers $head - ContentType application / json
        //            $secretResponse.PSObject.Properties.Value | foreach-object {
        //              $_ | select *
        //            }
        //        }
        //        catch
        //        {
        //            Log.Debug("ERROR"
        //            $_.Exception | select *
        //          }

        //#######################################################################
        //### DEPLOY THE CONTAINER INTO THE EKS CLUSTER
        //#######################################################################

        Log.Debug($"DEPLOY THE CONTAINER {NewContainer.Id} INTO THE EKS CLUSTER");

        var deployment = new QoveryContainerRequest { Image_tag = LatestProductionTag };

        try {
            var createEnvResult = await new APIClient(Runtime.CDCI.QoveryAPIUrl).QoveryApiCall(Method.POST, $"environment/{NewEnvironment.Id})/container", JsonConvert.SerializeObject(deployment), Runtime.CDCI.QoveryAPIToken);
            var environment = JsonConvert.DeserializeObject<QoveryEnvironmentResult>(createEnvResult.Content);
            Log.Debug(JsonConvert.SerializeObject(environment, Formatting.Indented));
        }
        catch (Exception e) { Log.Debug(e.Message); if (e.InnerException != null) Log.Debug(e.InnerException.Message); }
    }

    public Task<DeleteBackendResponse> DeleteCustomerBackend(DeleteBackendRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<DisableBackendResponse> DisableCustomerBackend(DisableBackendRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    
    public async Task<string> GetLatestProductionTag() {
        try {
            var createEnvResult = await new APIClient(Runtime.CDCI.QoveryAPIUrl).QoveryApiCall(Method.POST, $"environment/{NewEnvironment.Id})/container", JsonConvert.SerializeObject(deployment), Runtime.CDCI.QoveryAPIToken);
            var environment = JsonConvert.DeserializeObject<QoveryEnvironmentResult>(createEnvResult.Content);
            Log.Debug(JsonConvert.SerializeObject(environment, Formatting.Indented));
        }
        catch (Exception e) { Log.Debug(e.Message); if (e.InnerException != null) Log.Debug(e.InnerException.Message); }
    }
}
