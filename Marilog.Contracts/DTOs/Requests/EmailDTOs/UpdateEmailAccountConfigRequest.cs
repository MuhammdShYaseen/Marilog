using System;


namespace Marilog.Contracts.DTOs.Requests.EmailDTOs
{
    public class UpdateEmailAccountConfigRequest
    {
        public Dictionary<string, string>? Config { get; set; }
    }
}
