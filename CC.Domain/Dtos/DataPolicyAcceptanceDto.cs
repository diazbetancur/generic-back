using CC.Domain.Entities;

namespace CC.Domain.Dtos
{
    public class DataPolicyAcceptanceDto : BaseDto<Guid>
    {
        public string DocTypeCode { get; set; }
        public Guid DocTypeId { get; set; }
        public string DocNumber { get; set; }
        public string? IpAddress { get; set; }
        public string? PolicyVersion { get; set; }
    }
}