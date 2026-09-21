using Claims.Application.Auditing;
using Claims.Domain.Entities;
using Claims.Persistance;

namespace Claims.Infrastructure.Auditing;

public class Auditer(AuditContext auditContext) : IAuditService
{
    private readonly AuditContext _auditContext = auditContext;

    public Task AuditAsync(
        AuditMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        return AuditBatchAsync([message], cancellationToken);
    }

    public async Task AuditBatchAsync(
        IReadOnlyList<AuditMessage> messages,
        CancellationToken cancellationToken = default)
    {
        if (messages is null || messages.Count == 0)
        {
            return;
        }

        var claimAudits = new List<ClaimAudit>();
        var coverAudits = new List<CoverAudit>();

        foreach (var message in messages)
        {
            var created = message.Timestamp ?? DateTime.UtcNow;

            switch (message.EntityType)
            {
                case "Claim":
                    claimAudits.Add(new ClaimAudit
                    {
                        Created = created,
                        HttpRequestType = message.HttpRequestType,
                        ClaimId = message.EntityId
                    });
                    break;

                case "Cover":
                    coverAudits.Add(new CoverAudit
                    {
                        Created = created,
                        HttpRequestType = message.HttpRequestType,
                        CoverId = message.EntityId
                    });
                    break;

                default:
                    throw new ArgumentException(
                        $"Unsupported entity type: {message.EntityType}");
            }
        }

        if (claimAudits.Count > 0)
        {
            _auditContext.ClaimAudits.AddRange(claimAudits);
        }

        if (coverAudits.Count > 0)
        {
            _auditContext.CoverAudits.AddRange(coverAudits);
        }

        await _auditContext.SaveChangesAsync(cancellationToken);
    }
}
