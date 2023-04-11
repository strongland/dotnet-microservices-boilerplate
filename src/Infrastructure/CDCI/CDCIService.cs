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
    private QoveryEnvironment NewEnvironment { get; set; }

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
        //### CHECK IF ENVIRONMENT EXISTS
        //#######################################################################

        try {
            var result = await new APIClient(Runtime.CDCI.QoveryAPIUrl).QoveryApiCall(Method.GET, $"project/$(projectId)/environment", null, Runtime.CDCI.QoveryAPIToken);
            var environments = JsonConvert.DeserializeObject<List<QoveryEnvironment>>(result.Content);

            foreach (var env in environments) {
                if (env.Name == request.TenantName) {
                    Log.Debug($"{env.Name} - {env.Id}");
                    Log.Debug("FOUND EXISTING ENVIRONMENT FOR BRANCH: $(branchName) - ENVIRONMENT ID: $environmentId");
                    return new CreateBackendResponse { EnvironmentExists = true };
                }
            }
        }
        catch (Exception e) { Log.Debug(e.Message); if (e.InnerException != null) Log.Debug(e.InnerException.Message); }

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
            NewEnvironment = JsonConvert.DeserializeObject<QoveryEnvironment>(createEnvResult.Content);
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
            Tag = GetLatestProductionTag(),
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

        try
        {
            var createEnvResult = await new APIClient(Runtime.CDCI.QoveryAPIUrl).QoveryApiCall(Method.POST, $"environment/{NewEnvironment.Id})/container", newContainer, Runtime.CDCI.QoveryAPIToken);
            var environment = JsonConvert.DeserializeObject<QoveryEnvironment>(createEnvResult.Content);
            Log.Debug(JsonConvert.SerializeObject(environment, Formatting.Indented));
        }
        catch (Exception e) { Log.Debug(e.Message); if (e.InnerException != null) Log.Debug(e.InnerException.Message); }


        if ("$(containerId)" - eq "create_container") {
                    Log.Debug("I could not find a container, let's create one"

                  $url = "https://api.qovery.com/environment/$(environmentId)/container"
                  $head = @{ Authorization = "$(qoveryToken)" }
                  $body = @{
                        "name" = "$(repoName)"
                        "registry_id" = "$(registryId)"
                        "image_name" = "$(containerImageName)"
                        "tag" = "$(Build.SourceVersion)"
                        "cpu" =$(containerCpu)
                        "memory" =$(containerRAM)
                        "min_running_instances" = $(minNodes)
                        "max_running_instances" = $(maxNodes)
                        "ports" = @( @{
                            "name" = "api"
                            "internal_port" = $(containerInternalPort)
                            "external_port" = $(containerExternalPort)
                            "publicly_accessible" = "true"
                            "is_default" = "true"
                            "protocol" = "HTTP"
                            }
                          )
                      } | ConvertTo - Json
                  try
                    {
                        Log.Debug($body
                      $container = Invoke - RestMethod - Uri $url - Method Post - Headers $head - Body $body - ContentType application / json
                      $container | select *
                      $newContainerId = $container."id"
                      Log.Debug("CONTAINER ID:"
                      Log.Debug($newContainerId
                      Log.Debug("##vso[task.setvariable variable=containerId]$newContainerId"
                  }
                    catch
                    {
                        Log.Debug("ERROR"
                      $_.Exception | select *
                    }
                }
                else
                {
                    Log.Debug("I found $(containerId) for application $(containerImageName) so I will not create a new container."
                }


        //#########################################################################
        //### CHECK CONTAINER STATE, WAIT FOR IT TO BECOME AVAILABLE
        //#########################################################################

                if ("$(containerId)" - ne "create_container") {
                    Log.Debug("GET CONTAINER STATE for $(containerId)"


                var $url = "https://api.qovery.com/environment/$(environmentId)/status"
                var $head = @{ Authorization = "$(qoveryToken)" }
                    try
                    {
                        Log.Debug($url
                        $environment = Invoke - RestMethod - Uri $url - Method Get - Headers $head - ContentType application / json
                        $environmentState = $environment."state"
                        # Pause for 5 seconds per loop
                    while (($environmentState - eq "DEPLOYING") -Or($environmentState - eq "BUILDING") - Or($environmentState - eq "CANCELING") - Or($environmentState - eq "DELETING") - Or($environmentState - eq "DELETE_QUEUED")) {
                            Log.Debug("CONTAINER STATE IS $environmentState. LET'S WAIT 15 SECONDS AND TRY AGAIN"
                        # Sleep 5 seconds
                        Start - Sleep - s 15
                        $containers = Invoke - RestMethod - Uri $url - Method Get - Headers $head - ContentType application / json
                        $environmentState = $containers."state"
                    }
                        Log.Debug("CONTAINER STATE: $environmentState"
                        $containers | select *
                        Log.Debug("##vso[task.setvariable variable=environmentState]$environmentState"
                    }
                    catch
                    {
                        Log.Debug("ERROR"
                        $_.Exception | select *
                      }
                }

//#######################################################################
//### SET ENVIRONMENT VARIABLES AND ALIASES
//#######################################################################
            if condition: ne(variables['containerId'], 'create_container')

                Log.Debug-Host "SET QOVERY API SECRET for $(containerId)"

                var $url = "https://api.qovery.com/container/$(containerId)/secret"
                var $head = @{ Authorization = "$(qoveryToken)" }
        var $body = @{
                    "key" = "API_TOKEN_QOVERY"
                  "value" = "$(qoveryToken)"
                } | ConvertTo - Json
                Log.Debug($body

                try
                {
                    Log.Debug($url
                    $secretResponse = Invoke - RestMethod - Uri $url - Method Post - Body $body - Headers $head - ContentType application / json
                    $secretResponse.PSObject.Properties.Value | foreach-object {
                      $_ | select *
                    }
                }
                catch
                {
                    Log.Debug("ERROR"
                    $_.Exception | select *
                  }

        //#######################################################################
        //### DEPLOY THE CONTAINER INTO THE EKS CLUSTER
        //#######################################################################

                Log.Debug("CONTAINER ID: $(containerId)"
                Log.Debug("ENVIRONMENT STATE: $(environmentState)"
                if (("$(containerId)" - ne "create_container") -And("$(environmentState)" - ne "DEPLOYING")) {
                    Log.Debug("I found $(containerId) already created, let's redeploy the image"
                  Log.Debug("DEPLOY CONTAINER"

                  var $url = "https://api.qovery.com/container/$(containerId)/deploy"
                  var $head = @{ Authorization = "$(qoveryToken)" }
            var $body = @{
            var $body = @{
                        "image_tag" = "$(Build.SourceVersion)"
                    } | ConvertTo - Json
                  try
                    {
                        Log.Debug($body
                      Log.Debug($url
                      $deployment = Invoke - RestMethod - Uri $url - Method Post - Headers $head - Body $body - ContentType application / json
                      $deployment | select *
                  }
                    catch
                    {
                        Log.Debug("ERROR"
                      $_.Exception | select *
                    }
                }
                else
                {
                    Log.Debug("Epic doom, something is broken with getting the containerId or the container is busy deploying"
                }

        var result = await new RestClient("").POST(null,null);
        var json = JsonConvert.DeserializeObject<CreateBackendResponse>(result.Content);
        return json;
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

    }
}
