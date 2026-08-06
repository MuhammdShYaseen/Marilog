
using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.DTOs.Requests.EmailDTOs
{
    public class EmailCreationResult
    {
       public EmailResponse? Email {  get; set; }
       public bool IsNewlyCreated { get; set; }
    }
}
