using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.Interfaces.DataManagment
{
    public interface ISchemaVersionProvider
    {
        Task<string> GetCurrentVersionAsync(CancellationToken ct = default);
    }
}
