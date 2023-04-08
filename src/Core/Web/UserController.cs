using FSH.Core.Common;
using FSH.Core.Dto.BankId;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


public class UserController : BaseController {

    private readonly IBankIdService BankIdService;

    public UserController(IBankIdService bankIdService) {
        BankIdService = bankIdService;
    }

    [AllowAnonymous]
    [HttpPost("switchuserbackend")]
    public async Task<TokenResponse> SwitchUserBackend(UserTokenRequest request, CancellationToken cancellationToken) =>
        await UserServicee.SwitchUserBackend(request, cancellationToken);


    private string GetIpAddress() =>
    Request.Headers.ContainsKey("X-Forwarded-For")
        ? Request.Headers["X-Forwarded-For"]
        : HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "N/A";

    private string GetOriginFromRequest() => $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";
}
