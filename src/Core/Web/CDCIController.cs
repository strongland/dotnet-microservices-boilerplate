using FSH.Core.Common;
using FSH.Core.Dto.BankId;
using FSH.Core.Dto.CDCI;
using FSH.Core.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class CDCIController : BaseController {

    private readonly ICDCIService CDCIService;

    public CDCIController(ICDCIService cdciService) {
        CDCIService = cdciService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<string> Hello() => "hello you";

    [AllowAnonymous]
    [HttpPost]
    public async Task<CreateBackendResponse> CreateCustomerBackend(CreateBackendRequest request, CancellationToken cancellationToken) =>
        await CDCIService.CreateCustomerBackend(request, cancellationToken);

    [AllowAnonymous]
    [HttpPost("createcustomerbackend")]
    public async Task<DisableBackendResponse> DisableCustomerBackend(DisableBackendRequest request, CancellationToken cancellationToken) =>
    await CDCIService.DisableCustomerBackend(request, cancellationToken);

    [AllowAnonymous]
    [HttpPost("deletecustomerbackend")]
    public async Task<DeleteBackendResponse> DeleteCustomerBackend(DeleteBackendRequest request, CancellationToken cancellationToken) =>
    await CDCIService.DeleteCustomerBackend(request, cancellationToken);
}
