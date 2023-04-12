using FSH.Core.Common;
using FSH.Core.Dto.CDCI;
using FSH.WebApi.Infrastructure.Common.Settings;
using FSH.WebApi.Infrastructure.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using Newtonsoft.Json;
using RestSharp;

namespace FSH.Infrastructure.Auth;

public class CDCIService : ICDCIService {

    private string QoveryProjectId { get; set; }
    private string QoveryProductionContainerName { get; set; }
    private string QoveryToken { get; set; }
    private string ClusterId { get; set; }

    private QoveryEnvironmentResult NewEnvironment { get; set; }
    private QoveryContainerResult NewContainer { get; set; }
    private QoveryEnvironmentResult DeployedEnvironment { get; set; }
    private string LatestProductionTag { get; set; }

    public CDCIService(IConfiguration config) {
        Runtime.CDCI = config.GetSection("CDCISettings").Get<CDCISettings>();

        QoveryProjectId = "9d0a2ead-2060-47ff-a0fd-1eebaeb2e0f4"; // settings
        QoveryProductionContainerName = "dashboard-backend"; // settings


        //## DO NOT CHANGE THESE VALUES
        QoveryToken = "Token qov_7503rXkpWTKaBDbQXUOWJb3EXBMtGqRkVAmgsxOiA5nEWqJWyD0VvOe_1329524952"; // settings
        ClusterId = "ce5e20e8-fb97-441a-9a54-3c0dcdf8922b"; // settings

    }

    public CreateBackendResponse CreateBackendInstance(CreateBackendRequest request, CancellationToken cancellationToken) {

        //## CONTAINER RESOURCES, 1 CPU = 1000, RAM = Megabytes
        var containerCpu = 250; // settings
        var containerRAM = 256; // settings
        var containerInternalPort = 5060; // settings
        var containerExternalPort = 443; // settings
        var minNodes = 1; // settings
        var maxNodes = 1; // settings

        var registryId = "bd834fff-5eb0-4105-992a-e8bbb7cdac24"; // settings
        var prodContainerImageName = "dashboard-backend-prod";

        //#######################################################################
        //### GET LATEST PRODUCTION TAG
        //#######################################################################
        LatestProductionTag = GetLatestProductionTag();
        if (LatestProductionTag == String.Empty) { return new CreateBackendResponse { FoundProductionTag = false }; }

        //#######################################################################
        //### CHECK IF ENVIRONMENT EXISTS
        //#######################################################################

        try {
            var result = new APIClient(Runtime.CDCI.QoveryAPIUrl).QoveryApiCall(Method.GET, $"/project/{QoveryProjectId}/environment", null, Runtime.CDCI.QoveryAPIToken).Result;
            var environments = JsonConvert.DeserializeObject<QoveryEnvironmentsResult>(result.Content);

            foreach (var env in environments.Results) {
                if (env.Name == request.EnvironmentName) {
                    Log.Debug($"FOUND EXISTING ENVIRONMENT: {env.Name} - ENVIRONMENT ID: {env.Id}");
                    return new CreateBackendResponse { EnvironmentExists = true };
                }
            }
        }
        catch (Exception e) { Log.Debug(e.Message); if (e.InnerException != null) Log.Debug(e.InnerException.Message); }

        //#######################################################################
        //### IF IT DOES NOT EXIST, CREATE A NEW PROJECT ENVIRONMENT
        //#######################################################################
        Log.Debug($"COULD NOT FIND AN EXISTING ENVIRONMENT CALLED {request.EnvironmentName}... PROCEEDING");

        var newEnv = new QoveryEnvironmentRequest {
            name = request.EnvironmentName,
            cluster = ClusterId,
            mode = "PRODUCTION"
        };

        try {
            var createEnvResult = new APIClient(Runtime.CDCI.QoveryAPIUrl).QoveryApiCall(Method.POST, $"/project/{QoveryProjectId}/environment", JsonConvert.SerializeObject(newEnv), Runtime.CDCI.QoveryAPIToken).Result;
            NewEnvironment = JsonConvert.DeserializeObject<QoveryEnvironmentResult>(createEnvResult.Content);
            Log.Debug(JsonConvert.SerializeObject(NewEnvironment, Formatting.Indented));
        }
        catch(Exception e) { 
            Log.Debug(e.Message); if (e.InnerException != null) Log.Debug(e.InnerException.Message); }

        //#######################################################################
        //### CREATE A NEW BACKEND CONTAINER FROM LATEST DOCKER IMAGE IN AWS
        //#######################################################################

        Log.Debug($"CREATING A NEW BACKEND CONTAINER FROM LATEST DOCKER IMAGE IN AWS");

        var newContainer = new QoveryContainerRequest
        {
            name = QoveryProductionContainerName,
            registry_id = registryId,
            image_name = prodContainerImageName,
            tag = LatestProductionTag,
            cpu = containerCpu,
            memory = containerRAM,
            min_running_instances = minNodes,
            max_running_instances = maxNodes,
            ports = new List<QoveryContainerPort> { new QoveryContainerPort {
                    name = "api",
                    internal_port = containerInternalPort,
                    external_port = containerExternalPort,
                    publicly_accessible = true,
                    is_default = true,
                    protocol = "HTTP"
                }
            }
        };

        try {
            var createContainerResult = new APIClient(Runtime.CDCI.QoveryAPIUrl).QoveryApiCall(Method.POST, $"/environment/{NewEnvironment.Id}/container", JsonConvert.SerializeObject(newContainer), Runtime.CDCI.QoveryAPIToken).Result;
            NewContainer = JsonConvert.DeserializeObject<QoveryContainerResult>(createContainerResult.Content);
            Log.Debug($"CREATED NEW CONTAINER: {NewContainer.Name}");
            Log.Debug(JsonConvert.SerializeObject(NewContainer, Formatting.Indented));
        }
        catch (Exception e) { Log.Debug(e.Message); if (e.InnerException != null) Log.Debug(e.InnerException.Message); }



        //#########################################################################
        //### CHECK CONTAINER STATE, WAIT FOR IT TO BECOME AVAILABLE
        //#########################################################################

        Log.Debug($"CHECK CONTAINER STATE, WAIT FOR IT TO BECOME AVAILABLE");

        try {

            if (NewContainer != null) {
                string environmentState = "";
                while (environmentState == "DEPLOYING" || environmentState == "BUILDING" || environmentState == "CANCELING" || environmentState == "DELETING" || environmentState == "DELETE_QUEUED") {
                    try {
                        Log.Debug($"GET CONTAINER STATE for {NewContainer.Id}");
                        var createEnvResult = new APIClient(Runtime.CDCI.QoveryAPIUrl).QoveryApiCall(Method.GET, $"/environment/{NewEnvironment.Id}/status", null, Runtime.CDCI.QoveryAPIToken).Result;
                        environmentState = JsonConvert.DeserializeObject<QoveryEnvironmentStatusResult>(createEnvResult.Content).State;
                        Log.Debug($"CONTAINER STATE: {environmentState}");
                        if (environmentState == "DEPLOYING" || environmentState == "BUILDING" || environmentState == "CANCELING" || environmentState == "DELETING" || environmentState == "DELETE_QUEUED") {
                            Log.Debug($"CONTAINER STATE IS {environmentState}. LET'S WAIT 15 SECONDS AND TRY AGAIN");
                        }
                        Thread.Sleep(15000);
                    }
                    catch (Exception e) {
                        Log.Debug(e.Message);
                        if (e.InnerException != null) Log.Debug(e.InnerException.Message);
                    }
                }
            }
        }
        catch (Exception e) { Log.Debug(e.Message); if (e.InnerException != null) Log.Debug(e.InnerException.Message); }

        Log.Debug("CONTAINER CREATED OK");



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

        var deployment = new QoveryContainerRequest { image_tag = LatestProductionTag };

        try {
            var createEnvResult = new APIClient(Runtime.CDCI.QoveryAPIUrl).QoveryApiCall(Method.POST, $"/container/{NewEnvironment.Id}/container", JsonConvert.SerializeObject(deployment), Runtime.CDCI.QoveryAPIToken).Result;
            DeployedEnvironment = JsonConvert.DeserializeObject<QoveryEnvironmentResult>(createEnvResult.Content);
            Log.Debug(JsonConvert.SerializeObject(DeployedEnvironment, Formatting.Indented));
        }
        catch (Exception e) { Log.Debug(e.Message); if (e.InnerException != null) Log.Debug(e.InnerException.Message); return new CreateBackendResponse { BackendEnvironmentId = "deployment failed" }; }

        return new CreateBackendResponse {
            FoundProductionTag = true,
            EnvironmentExists = true,
            BackendEnvironmentId = DeployedEnvironment.Id
        };
    }

    public ToggleStateBackendResponse ToggleStateBackendInstance(ToggleStateBackendRequest request, CancellationToken cancellationToken) {
        ToggleStateBackendResponse response = new ToggleStateBackendResponse();

        if (request.Enabled) {
            try {
                // Get list of environments
                var environmentsResult = new APIClient(Runtime.CDCI.QoveryAPIUrl).QoveryApiCall(Method.GET, $"/project/{QoveryProjectId}/environment", null, Runtime.CDCI.QoveryAPIToken).Result;
                var environmentList = JsonConvert.DeserializeObject<QoveryEnvironmentsResult>(environmentsResult.Content);
                string environmentName = String.Empty;

                foreach (var environment in environmentList.Results) {
                    if (environment.Name == request.EnvironmentName) {
                        environmentName = environment.Name;

                        // Get list of containers
                        var environmentStopResult = new APIClient(Runtime.CDCI.QoveryAPIUrl).QoveryApiCall(Method.POST, $"/environment/{environment.Id}/start", null, Runtime.CDCI.QoveryAPIToken).Result;
                        var environmentStop = JsonConvert.DeserializeObject<QoveryContainerStopResult>(environmentStopResult.Content);
                        response.EnvironmentName = environmentName;
                        response.State = environmentStop.State;
                        response.Message = environmentStop.Message;
                    }
                }
            }
            catch (Exception e) {
                Log.Debug(e.Message); if (e.InnerException != null) Log.Debug(e.InnerException.Message);
                response.Message = $"{e.Message}";
                return response;
            }
        }
        else {
            try {
                // Get list of environments
                var environmentsResult = new APIClient(Runtime.CDCI.QoveryAPIUrl).QoveryApiCall(Method.GET, $"/project/{QoveryProjectId}/environment", null, Runtime.CDCI.QoveryAPIToken).Result;
                var environmentList = JsonConvert.DeserializeObject<QoveryEnvironmentsResult>(environmentsResult.Content);
                string environmentName = String.Empty;

                foreach (var environment in environmentList.Results) {
                    if (environment.Name == request.EnvironmentName) {
                        environmentName = environment.Name;

                        // Get list of containers
                        var environmentStopResult = new APIClient(Runtime.CDCI.QoveryAPIUrl).QoveryApiCall(Method.POST, $"/environment/{environment.Id}/stop", null, Runtime.CDCI.QoveryAPIToken).Result;
                        var environmentStop = JsonConvert.DeserializeObject<QoveryContainerStopResult>(environmentStopResult.Content);
                        response.EnvironmentName = environmentName;
                        response.State = environmentStop.State;
                        response.Message = environmentStop.Message;
                    }
                }
            }
            catch (Exception e) {
                Log.Debug(e.Message); if (e.InnerException != null) Log.Debug(e.InnerException.Message);
                response.Message = $"{e.Message}";
                return response;
            }
        }
        return response;
    }
    
    public string GetLatestProductionTag() {
        try {
            // Get list of environments
            var environmentsResult = new APIClient(Runtime.CDCI.QoveryAPIUrl).QoveryApiCall(Method.GET, $"/project/{QoveryProjectId}/environment", null,  Runtime.CDCI.QoveryAPIToken).Result;
            var environmentList = JsonConvert.DeserializeObject<QoveryEnvironmentsResult>(environmentsResult.Content);

            foreach (var environment in environmentList.Results) {
                if (environment.Name == "prod") {
                    // Get list of containers
                    var containersResult = new APIClient(Runtime.CDCI.QoveryAPIUrl).QoveryApiCall(Method.GET, $"/environment/{environment.Id}/container", null, Runtime.CDCI.QoveryAPIToken).Result;
                    var containersList = JsonConvert.DeserializeObject<QoveryContainersResult>(containersResult.Content);

                    foreach (var container in containersList.Results ) {
                        if (container.Name == QoveryProductionContainerName) { return container.Tag; }
                    }                    
                }
            }
        }
        catch (Exception e) { Log.Debug(e.Message); if (e.InnerException != null) Log.Debug(e.InnerException.Message); return String.Empty; }

        return String.Empty;
    }
}
