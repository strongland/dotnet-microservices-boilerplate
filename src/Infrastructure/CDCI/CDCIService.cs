using FSH.Core.Common;
using FSH.Core.Dto.BankId;
using FSH.Core.Dto.CDCI;
using FSH.WebApi.Infrastructure.Common.Settings;
using FSH.WebApi.Infrastructure.Helpers;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;

namespace FSH.Infrastructure.Auth;

public class CDCIService : ICDCIService {

    private readonly ILogger Logger;

    public CDCIService(IConfiguration config, ILogger logger, IUserService userService) {
        Logger = logger;
        Runtime.SecuritySettings = config.GetSection("SecuritySettings").Get<SecuritySettings>();
    }

    public Task<CreateBackendResponse> CreateCustomerBackend(CreateBackendRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
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
