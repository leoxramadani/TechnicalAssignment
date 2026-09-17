using Claims.Domain.Entities;

namespace Claims.Services.ClaimsServices
{
    public interface IClaimsService
    {
        Task<IEnumerable<Claim>> GetClaimsAsync(CancellationToken cancellationToken = default);

        Task<Claim?> GetClaimByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Claim> CreateClaimAsync(Claim claim, CancellationToken cancellationToken = default);
        Task<bool> DeleteClaimAsync(string id, CancellationToken cancellationToken = default);
    }
}
