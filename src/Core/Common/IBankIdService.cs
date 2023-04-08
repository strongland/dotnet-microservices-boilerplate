using FSH.Core.Dto.BankId;
using FSH.Core.Dto.CDCI;

namespace FSH.Core.Common;

public interface IBankIdService : ITransientService
{
    Task<BankIdResponses> Auth(BankIdAuth auth, CancellationToken cancellationToken);
    Task<BankIdResponses> Sign(BankIdSign sign, CancellationToken cancellationToken);
    Task<BankIdResponses> CollectQR(BankIdCollect collectQr, CancellationToken cancellationToken);
    Task<BankIdResponses> CollectStatus(BankIdCollect collectStatus, string ipAddress, string requestOrigin, CancellationToken cancellationToken);
}
