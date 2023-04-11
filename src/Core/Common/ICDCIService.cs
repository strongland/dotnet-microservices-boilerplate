using FSH.Core.Dto.BankId;
using FSH.Core.Dto.CDCI;
namespace FSH.Core.Common;

public interface ICDCIService : ITransientService
{
    CreateBackendResponse CreateCustomerBackend(CreateBackendRequest request, CancellationToken cancellationToken);
    DisableBackendResponse DisableCustomerBackend(DisableBackendRequest request, CancellationToken cancellationToken);
}
