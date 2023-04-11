using Newtonsoft.Json;
using PipefyBackend;
using RestSharp;
using RestSharp.Authenticators;

namespace FSH.WebApi.Infrastructure.Helpers
{
    public class APIClient {
        private RestClient Client { get; set; } = new RestClient();
        private string BaseUrl { get; set; }
        private string Port { get; set; } = "";
        private string ContentType { get; set; } = "application/json";

        public APIClient(string baseUrl, string ?contentType = null) { 
            BaseUrl = baseUrl;
            if (contentType != null) ContentType = contentType;
        }

        private async Task<IRestResponse> ApiCall(string endpoint, Method method, string jsonBody, List<Tuple<string, string>> ?parameters, FormRequestBody ?formBody) {
            if (Port != "") BaseUrl = $"{BaseUrl}:{Port}/";
            Client = new RestSharp.RestClient(BaseUrl);
            RestRequest request = new RestRequest(BaseUrl+endpoint, method);
            request.AddHeader("Content-Type", ContentType);
            
            if (ContentType == "application/x-www-form-urlencoded" && formBody != null) { request.AddObject(formBody); }
            if (jsonBody != null) { request.AddJsonBody(jsonBody); }
            if (parameters != null) {
                foreach (var param in parameters) {
                    request.AddQueryParameter(param.Item1, param.Item2);
                }
            }
            IRestResponse? result = null;
            try {
                Client.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                result = await System.Threading.Tasks.Task.FromResult(Client.Execute(request));
                if (result.ErrorException != null) {
                    var errorMsg = "ERROR IN REST API CALL";
                    Log.Trace(errorMsg);
                    Log.Trace(JsonConvert.SerializeObject(result.ErrorException, Formatting.Indented));
                    }
            }
            catch (Exception e) {
                Log.Debug("ERROR IN HTTP REQUEST");
                Log.Debug(e.Message);
            }
            return result;
        }

        public async Task<IRestResponse> SimpleAuthApiCall(Method method, string endpoint, string? jsonBody, string username, string password) {
            if (Port != "") BaseUrl = $"{BaseUrl}:{Port}/";
            Client = new RestSharp.RestClient(BaseUrl);
            RestRequest request = new RestRequest(BaseUrl + endpoint, method);
            request.AddHeader("Content-Type", ContentType);

            Client.Authenticator = new HttpBasicAuthenticator(username, password);

            if (jsonBody != null) { request.AddJsonBody(jsonBody); }

            IRestResponse? result = null;
            try {
                Client.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                result = await System.Threading.Tasks.Task.FromResult(Client.Execute(request));
                if (result.ErrorException != null) {
                    var errorMsg = "ERROR IN REST API CALL";
                    Log.Trace(errorMsg);
                    Log.Trace(JsonConvert.SerializeObject(result.ErrorException, Formatting.Indented));
                }
            }
            catch (Exception e) {
                Log.Debug("ERROR IN HTTP REQUEST");
                Log.Debug(e.Message);
            }
            return result;
        }

        public async Task<IRestResponse> TogglOauth2ApiCall(Method method, string endpoint, string? jsonBody, string token)
        {
            if (Port != "") BaseUrl = $"{BaseUrl}:{Port}/";
            Client = new RestSharp.RestClient(BaseUrl);
            RestRequest request = new RestRequest(BaseUrl + endpoint, method);
            request.AddHeader("Content-Type", ContentType);

            request.AddHeader("Authorization", $"Bearer {token}");

            if (jsonBody != null) { request.AddJsonBody(jsonBody); }

            IRestResponse? result = null;
            try
            {
                Client.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                result = await System.Threading.Tasks.Task.FromResult(Client.Execute(request));
                if (result.ErrorException != null)
                {
                    var errorMsg = "ERROR IN REST API CALL";
                    Log.Trace(errorMsg);
                    Log.Trace(JsonConvert.SerializeObject(result.ErrorException, Formatting.Indented));
                }
            }
            catch (Exception e)
            {
                Log.Debug("ERROR IN HTTP REQUEST");
                Log.Debug(e.Message);
            }
            return result;
        }
        public async Task<IRestResponse> QoveryApiCall(Method method, string endpoint, string? jsonBody, string token)
        {
            if (Port != "") BaseUrl = $"{BaseUrl}:{Port}/";
            Client = new RestSharp.RestClient(BaseUrl);
            RestRequest request = new RestRequest(BaseUrl + endpoint, method);
            request.AddHeader("Content-Type", ContentType);

            request.AddHeader("Authorization", $"{token}");

            if (jsonBody != null) { request.AddJsonBody(jsonBody); }

            IRestResponse? result = null;
            try
            {
                Client.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                result = await System.Threading.Tasks.Task.FromResult(Client.Execute(request));
                if (result.ErrorException != null)
                {
                    var errorMsg = "ERROR IN REST API CALL";
                    Log.Trace(errorMsg);
                    Log.Trace(JsonConvert.SerializeObject(result.ErrorException, Formatting.Indented));
                }
            }
            catch (Exception e)
            {
                Log.Debug("ERROR IN HTTP REQUEST");
                Log.Debug(e.Message);
            }
            return result;
        }

        public async Task<IRestResponse> GraphQlOauth2ApiCall(Method method, string jsonBody, string token) {
            if (Port != "") BaseUrl = $"{BaseUrl}:{Port}/";
            Client = new RestSharp.RestClient(BaseUrl);
            RestRequest request = new RestRequest(BaseUrl, method);
            request.AddHeader("Content-Type", ContentType);

            request.AddHeader("Authorization", $"Bearer {token}");

            //Client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator("Bearer", token);

            if (jsonBody != null) { request.AddJsonBody(jsonBody); }

            IRestResponse ?result = null;
            try {
                Client.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                result = await System.Threading.Tasks.Task.FromResult(Client.Execute(request));
                if (result.ErrorException != null) {
                    var errorMsg = "ERROR IN REST API CALL";
                    Log.Trace(errorMsg);
                    Log.Trace(JsonConvert.SerializeObject(result.ErrorException, Formatting.Indented));
                }
            }
            catch (Exception e) {
                Log.Debug("ERROR IN HTTP REQUEST");
                Log.Debug(e.Message);
            }
            return result;
        }

        public async Task<IRestResponse> GET(string endpoint, List<Tuple<string, string>> ?parameters = null, FormRequestBody ?formBody = null) {
            return await ApiCall(endpoint, Method.GET, null, parameters, formBody);
        }

        public async Task<IRestResponse> POST(string endpoint, string jsonBody, List<Tuple<string, string>>? parameters = null, FormRequestBody? formBody = null) {
            return await ApiCall(endpoint, Method.POST, jsonBody, parameters, formBody);
        }

        public async Task<IRestResponse> PUT(string endpoint, string jsonBody, List<Tuple<string, string>> ?parameters = null, FormRequestBody ?formBody = null) {
            return await ApiCall(endpoint, Method.PUT, jsonBody, parameters, formBody);
        }

        public async Task<IRestResponse> PATCH(string endpoint, string jsonBody, List<Tuple<string, string>> ?parameters = null, FormRequestBody ?formBody = null) {
            return await ApiCall(endpoint, Method.PATCH, jsonBody, parameters, formBody);
        }

        public async Task<IRestResponse> DELETE(string endpoint, string jsonBody, List<Tuple<string, string>> ?parameters = null, FormRequestBody ?formBody = null) {
            return await ApiCall(endpoint, Method.DELETE, jsonBody, parameters, formBody);
        }
    }

    public class FormRequestBody {
        public string ?username { get; set; }
        public string ?password { get; set; }
        public string ?token { get; set; }
    }
}

