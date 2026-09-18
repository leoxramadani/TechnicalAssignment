using Claims.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace Claims.Domain.Entities;

/// <summary>
/// Represents a cover entity with properties such as ID, start date, end date, type, and premium.
/// </summary>
public class Cover
{
    [BsonId]
    public string Id { get; set; } = String.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public CoverTypeEnum Type { get; set; }
    public decimal Premium { get; set; }
}


