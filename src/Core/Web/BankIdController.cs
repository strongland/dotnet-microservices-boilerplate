using FSH.Core.Common;
using FSH.Core.Dto.BankId;
using FSH.Core.Dto.CDCI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FSH.Core.Web;

public class BankIdController : BaseController {

    private readonly IBankIdService BankIdService;

    public BankIdController(IBankIdService bankIdService) {
        BankIdService = bankIdService;
    }

    [AllowAnonymous]
    [HttpPost("auth")]
    public async Task<BankIdResponses> Auth(BankIdAuth auth, CancellationToken cancellationToken) =>
        await BankIdService.Auth(auth, cancellationToken);

    [AllowAnonymous]
    [HttpPost("sign")]
    public async Task<BankIdResponses> Sign(BankIdSign sign, CancellationToken cancellationToken) =>
        await BankIdService.Sign(sign, cancellationToken);

    [AllowAnonymous]
    [HttpPost("collectqr")]
    public async Task<BankIdResponses> CollectQr(BankIdCollect collectQr, CancellationToken cancellationToken) =>
        await BankIdService.CollectQR(collectQr, cancellationToken);

    [AllowAnonymous]
    [HttpPost("collectstatus")]
    public async Task<BankIdResponses> CollectStatus(BankIdCollect collectStatus, CancellationToken cancellationToken) =>
        await BankIdService.CollectStatus(collectStatus, GetIpAddress(), GetOriginFromRequest(), cancellationToken);

    private string GetIpAddress() =>
    Request.Headers.ContainsKey("X-Forwarded-For")
        ? Request.Headers["X-Forwarded-For"]
        : HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "N/A";

    private string GetOriginFromRequest() => $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";
}
