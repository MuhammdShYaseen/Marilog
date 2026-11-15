using Marilog.Application.DTOs.Ai;
using Marilog.Domain.Entities.AiEntities;

namespace Marilog.Application.Interfaces.Ai
{
    public interface IAiRequestBuilder
    {
        string Build(AiProvider provider, AiRequestContext context, bool stream = false);
    }
}
