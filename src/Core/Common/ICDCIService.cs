using FSH.Core.Dto.BankId;
using FSH.Core.Dto.CDCI;
namespace FSH.Core.Common;

public interface ICDCIService : ITransientService
{
    CreateBackendResponse CreateBackendInstance(CreateBackendRequest request, CancellationToken cancellationToken);
    ToggleStateBackendResponse ToggleStateBackendInstance(ToggleStateBackendRequest request, CancellationToken cancellationToken);
}
