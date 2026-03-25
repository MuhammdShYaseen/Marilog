using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.DTOs
{
    public class DocumentTypeResponse
    {
        public string? Code { get;  set; }
        public string? Name { get;  set; }
        public int SortOrder { get;  set; }
    }
}
