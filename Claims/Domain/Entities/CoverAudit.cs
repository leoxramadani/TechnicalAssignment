namespace Claims.Domain.Entities
{
    /// <summary>
    /// Represents an audit record for a cover, capturing relevant information such as the cover ID, creation timestamp, and HTTP request type.
    /// </summary>
    public class CoverAudit
    {
        public int Id { get; set; }

        public string? CoverId { get; set; }

        public DateTime Created { get; set; }

        public string? HttpRequestType { get; set; }
    }
}
