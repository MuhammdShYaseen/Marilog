using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Contracts.DTOs.Responses
{
    public class TagResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Color { get; set; } = default!;
    }
}
