using Marilog.Domain.Entities.SystemEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.Interfaces.Services
{
    /// <summary>
    /// Domain service — generates unique document numbers per type.
    /// e.g. INV-2025-0042, QT-2025-0001
    /// </summary>
    public interface IDocumentNumberService
    {
        Task<string> GenerateAsync(DocumentType docType, CancellationToken ct = default);
    }
}

