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
    public CreateBackendResponse CreateBackendInstance(CreateBackendRequest request, CancellationToken cancellationToken) =>
        CDCIService.CreateBackendInstance(request, cancellationToken);

    [AllowAnonymous]
    [HttpPost]
    public ToggleStateBackendResponse ToggleStateBackendInstance(ToggleStateBackendRequest request, CancellationToken cancellationToken) =>
        CDCIService.ToggleStateBackendInstance(request, cancellationToken);

}
