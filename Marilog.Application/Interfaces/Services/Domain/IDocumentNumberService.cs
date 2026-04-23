using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.Interfaces.Services.Domain
{
    public interface IDocumentNumberService
    {
        Task<string> GenerateAsync(DocumentType docType,
              CancellationToken ct = default);
    }
}
