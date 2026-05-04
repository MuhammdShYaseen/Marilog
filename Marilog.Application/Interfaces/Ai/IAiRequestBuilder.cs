using Marilog.Application.DTOs.Ai;
using Marilog.Domain.Entities.AiEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.Interfaces.Ai
{
    public interface IAiRequestBuilder
    {
        string Build(AiProvider provider, AiRequestContext context);
    }
}
