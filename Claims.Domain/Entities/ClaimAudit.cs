namespace Claims.Domain.Entities;

/// <summary>
/// Represents an audit record for a claim, capturing relevant information such as the claim ID, creation timestamp, and HTTP request type.
/// </summary>
public class ClaimAudit
{
    public int Id { get; set; }

    public string? ClaimId { get; set; }

    public DateTime Created { get; set; }

    public string? HttpRequestType { get; set; }
}
