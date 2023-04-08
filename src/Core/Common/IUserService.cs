using FSH.Core.Dto.BankId;
using FSH.Core.Web;

namespace FSH.Core.Common;

public interface IUserService : ITransientService
{
    Task<TokenResponse> SwitchUserBackend(UserTokenRequest request, CancellationToken cancellationToken);
    Task<string> ResolveUsersBackendHostUrl(string personalNumber);
    Task<TokenResponse> GenerateAccessTokenInUserBackend(string personalNumber, CancellationToken cancellationToken);
    Task<BankIdResponse> GetFromPersonalNumberAsync(string personalNumber, CancellationToken cancellationToken);
}
