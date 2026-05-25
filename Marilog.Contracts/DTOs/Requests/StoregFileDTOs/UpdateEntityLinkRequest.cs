using Marilog.Kernel.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Contracts.DTOs.Requests.StoregFileDTOs
{
    public class UpdateEntityLinkRequest
    {
        public EntityType EntityType { get; init; }
        public int EntityId { get; init; }
    }
}
