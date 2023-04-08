using FSH.Core.Common;
using FSH.Core.Dto.BankId;
using FSH.Core.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


public class UserController : BaseController {

    private readonly IUserService UserService;

    public UserController(IUserService userService) {
        UserService = userService;
    }

    [AllowAnonymous]
    [HttpPost("switchuserbackend")]
    public async Task<TokenResponse> SwitchUserBackend(UserTokenRequest request, CancellationToken cancellationToken) =>
        await UserService.SwitchUserBackend(request, cancellationToken);
}
