using Claims.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace Claims.Domain.Entities
{
    /// <summary>
    /// Represents a claim entity with properties such as ID, cover ID, creation timestamp, name, type, and damage cost.
    /// </summary>
    public class Claim
    {
        [BsonId]
        public string Id { get; set; } = String.Empty;
        public string CoverId { get; set; } = String.Empty;
        public DateTime Created { get; set; }
        public string Name { get; set; } = String.Empty;
        public ClaimTypeEnum Type { get; set; }
        public decimal DamageCost { get; set; }
    }


}
