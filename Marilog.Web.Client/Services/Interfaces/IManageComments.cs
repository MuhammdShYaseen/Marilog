using Marilog.Kernel.Enums;

namespace Marilog.Web.Client.Services.Interfaces
{
    public interface IManageComments
    {
        Task OpenCommentsDialogAsync((int entityId, EntityType entityType) arg, string? contextLabel = null);
    }
}