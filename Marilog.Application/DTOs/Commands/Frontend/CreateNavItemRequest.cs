using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.DTOs.Commands.Frontend
{
    public class CreateNavItemRequest
    {
        public string Title { get;  init; } = null!;
        public int? ParentId { get; init; }
        public string? Route { get;  init; }
        public string? Icon { get;  init; }
        public int Order { get;  init; }
    }
}
