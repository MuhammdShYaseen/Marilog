using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.Interfaces.DataManagment
{
    /// <summary>
    /// A backup built in the background on the server, then downloaded by the browser itself.
    /// </summary>
    public interface IBackupJobService
    {
        /// <summary>Starts a backup, or returns the one already running.</summary>
        Task<BackupJobResponse> StartAsync(CancellationToken ct = default);

        Task<BackupJobResponse> GetAsync(string token, CancellationToken ct = default);

        /// <summary>A one-time link the browser opens to download the ready file.</summary>
        string GetDownloadUrl(string token);
    }
}