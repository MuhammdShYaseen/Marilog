

namespace Marilog.Application.Interfaces.DataManagment
{
    public interface IDatabaseBackupService
    {
        string ProviderName { get; }
        string FileExtension { get; }
        Task CreateBackupAsync(Stream destination, CancellationToken ct = default);
        Task RestoreBackupAsync(Stream source, CancellationToken ct = default);
    }
}
