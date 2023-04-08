using FSH.Core.Dto.BankId;
using FSH.Core.Dto.CDCI;
namespace FSH.Core.Common;

public interface ICDCIService : ITransientService
{
    Task<CreateBackendResponse> CreateCustomerBackend(CreateBackendRequest request, CancellationToken cancellationToken);
    Task<DisableBackendResponse> DisableCustomerBackend(DisableBackendRequest request, CancellationToken cancellationToken);
    Task<DeleteBackendResponse> DeleteCustomerBackend(DeleteBackendRequest request, CancellationToken cancellationToken);
}
