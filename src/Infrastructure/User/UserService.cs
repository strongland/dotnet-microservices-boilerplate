using FSH.Core.Common;
using FSH.Core.Dto.BankId;
using FSH.Core.Web;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace FSH.Infrastructure.Auth;

public class UserService : IUserService {

    private readonly ILogger Logger;

    public UserService(IConfiguration config, ILogger logger) {
        Logger = logger;
    }

    public Task<TokenResponse> GenerateAccessTokenInUserBackend(string personalNumber, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<BankIdResponses> GetFromPersonalNumberAsync(string personalNumber, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string> ResolveUsersBackendHostUrl(string personalNumber)
    {
        throw new NotImplementedException();
    }

    public Task<TokenResponse> SwitchUserBackend(UserTokenRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
