namespace Claims.Application.Auditing;

public sealed record AuditMessage(
    string EntityId,
    string EntityType,
    string HttpRequestType,
    DateTime? Timestamp = null);
