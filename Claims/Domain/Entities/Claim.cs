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
        public string Id { get; set; }
        public string CoverId { get; set; }
        public DateTime Created { get; set; }
        public string Name { get; set; }
        public ClaimTypeEnum Type { get; set; }
        public decimal DamageCost { get; set; }
    }


}
