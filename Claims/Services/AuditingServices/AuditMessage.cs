namespace Claims.Services.AuditingServices;

public sealed record AuditMessage(
    string EntityId,
    string EntityType,
    string HttpRequestType);