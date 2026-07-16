
namespace Marilog.Application.Interfaces.DataManagment
{
    public interface IFullBackupService
    {
        string FileExtension { get; } // ".zip"
        Task CreateBackupAsync(Stream destination, CancellationToken ct = default);
        Task RestoreBackupAsync(Stream source, CancellationToken ct = default);
    }
}
