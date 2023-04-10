using FSH.Core.Common;
using FSH.Core.Dto.CDCI;
using FSH.WebApi.Infrastructure.Common.Settings;
using FSH.WebApi.Infrastructure.Helpers;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;

namespace FSH.Infrastructure.Auth;

public class CDCIService : ICDCIService {

    private readonly ILogger Logger;

    public CDCIService(IConfiguration config, ILogger logger) {
        Logger = logger;
        Runtime.CDCI = config.GetSection("CDCI").Get<CDCISettings>();
    }

    public async Task<CreateBackendResponse> CreateCustomerBackend(CreateBackendRequest request, CancellationToken cancellationToken)
    {
        
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
}
