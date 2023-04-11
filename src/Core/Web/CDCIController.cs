using FSH.Core.Common;
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
    [HttpPost]
    public CreateBackendResponse CreateCustomerBackend(CreateBackendRequest request, CancellationToken cancellationToken) =>
        CDCIService.CreateCustomerBackend(request, cancellationToken);

    [AllowAnonymous]
    [HttpPost("createcustomerbackend")]
    public DisableBackendResponse DisableCustomerBackend(DisableBackendRequest request, CancellationToken cancellationToken) =>
        CDCIService.DisableCustomerBackend(request, cancellationToken);

}
