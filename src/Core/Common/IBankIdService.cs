using FSH.Core.Dto.BankId;

namespace FSH.Core.Common;

public interface IBankIdService : ITransientService
{
    Task<BankIdResponse> Auth(BankIdAuth auth, CancellationToken cancellationToken);
    Task<BankIdResponse> Sign(BankIdSign sign, CancellationToken cancellationToken);
    Task<BankIdResponse> CollectQR(BankIdCollect collectQr, CancellationToken cancellationToken);
    Task<BankIdResponse> CollectStatus(BankIdCollect collectStatus, string ipAddress, string requestOrigin, CancellationToken cancellationToken);
}
