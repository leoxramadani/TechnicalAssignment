using Claims.Domain.Entities;
using Claims.Domain.Enums;

namespace Claims.Application.Covers;

public interface ICoversService
{
    Task<IEnumerable<Cover>> GetCoversAsync(CancellationToken cancellationToken = default);
    Task<Cover?> GetCoverByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Cover> CreateCoverAsync(Cover cover, CancellationToken cancellationToken = default);
    Task<bool> DeleteCoverAsync(string id, CancellationToken cancellationToken = default);
    decimal ComputePremium(DateTime startDate, DateTime endDate, CoverTypeEnum coverType);
}
