using Marilog.Kernel.Enums;

namespace Marilog.Web.Client.Services.Interfaces
{
    public interface IManageStoredFiles
    {
        Task OpenUploadFilesDialogAsync((int entityId, EntityType entityType) arg, Func<Task> loadAsync, Func<Task> refreshSelectedAsync);
        Task OpenManageStoredFilesDialogAsync((int entityId, EntityType entityType) arg);
        Task UpdateContentAsync(int fileId, string content);

    }
}
