namespace CC.Domain.Entities
{
    public class DataPolicyAcceptance : EntityBase<Guid>
    {
        public Guid DoctTypeId { get; set; }
        public virtual DocType DocumentType { get; set; }
        public string DocNumber { get; set; }
        public DateTime AcceptanceDate { get; set; } = DateTime.UtcNow;
        public bool IsAccepted { get; set; } = true;
        public string? IpAddress { get; set; }
        public string? PolicyVersion { get; set; }
    }
}