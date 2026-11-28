using Marilog.Contracts.DTOs.Requests.StoregFileDTOs;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Kernel.Enums;
using Marilog.Shared.UI.Components.ManagingStoredFiles;
using MudBlazor;
using System.IO;

namespace Marilog.Ship.Services.Files
{
    /// <summary>
    /// Opens Marilog's ManageStoredFilesDialog for one record — the ship's version of
    /// Marilog.Web.Client's ManageStoredFiles, calling IStoredFileService in-process.
    /// The page passes its own IDialogService (the one the dialog provider uses) and its scoped IStoredFileService.
    /// </summary>
    public static class ShipStoredFiles
    {
        private const long MaxFileSize = 50 * 1024 * 1024;   // same limit as Marilog.Web.Client

        public static async Task OpenAsync(IDialogService dialogService, IStoredFileService files,
            (int entityId, EntityType entityType) arg, string? contextLabel = null)
        {
            var title = string.IsNullOrWhiteSpace(contextLabel)
                ? $"{arg.entityType} #{arg.entityId}"
                : contextLabel.Trim();

            var options = new DialogOptions
            {
                MaxWidth = MaxWidth.Medium,
                FullWidth = true,
                CloseButton = true,
                Position = DialogPosition.Center
            };

            var parameters = new DialogParameters<ManageStoredFilesDialog>
            {
                { x => x.LoadFilesFunc, ct => files.GetByEntityIdAsync(arg.entityId, arg.entityType, ct) },
                { x => x.DeleteFileFunc, (id, ct) => files.DeleteAsync(id, ct) },
                { x => x.AddTagFunc, (id, name, color, ct) => files.AddTagAsync(id, name, color, ct) },
                { x => x.RemoveTagFunc, (id, tagId, ct) => files.RemoveTagAsync(id, tagId, ct) },
                { x => x.BuildDownloadUrl, file => ShipFileHost.DownloadUrl(file.Id) },
                { x => x.BuildThumbnailUrl, file => ShipFileHost.ThumbnailUrl(file.Id) },
                { x => x.SaveContentFunc, async (id, content, ct) => { await files.UpdateContentFromUserAsync(id, content, ct); return true; } },
                { x => x.RenameFunc, async (id, newName, ct) => { await files.RenameAsync(id, newName, ct); return true; } },
                { x => x.UploadFilesFunc, async (browserFiles, folderId, ct) =>
                    {
                        // The browser stream can't seek, and StoredFileService reads it twice
                        // (checksum, then save) — so each file is copied to memory first.
                        var requests = new List<UploadFileRequest>();
                        try
                        {
                            foreach (var f in browserFiles)
                            {
                                var buffer = new MemoryStream();
                                await using (var stream = f.OpenReadStream(MaxFileSize, ct))
                                    await stream.CopyToAsync(buffer, ct);
                                buffer.Position = 0;

                                requests.Add(new UploadFileRequest
                                {
                                    ContentType = f.ContentType,
                                    EntityId = arg.entityId,
                                    EntityType = arg.entityType,
                                    FolderId = folderId,
                                    FileName = f.Name,
                                    FileStream = buffer,
                                    Size = f.Size
                                });
                            }

                            var outcome = await files.UploadAsync(requests, ct);
                            return outcome.Count > 0;
                        }
                        finally
                        {
                            foreach (var r in requests)
                                await r.FileStream.DisposeAsync();
                        }
                    }
                },
                { x => x.LoadFoldersFunc, ct => files.GetFoldersByEntityIdAsync(arg.entityId, arg.entityType, ct) },
                { x => x.CreateFolderFunc, (name, parentFolderId, ct) => files.CreateFolderAsync(new CreateStoredFolderRequest
                    {
                        Name = name,
                        ParentFolderId = parentFolderId,
                        EntityType = arg.entityType,
                        EntityId = arg.entityId
                    }, ct) },
                { x => x.RenameFolderFunc, (id, name, ct) => files.RenameFolderAsync(id, name, ct) },
                { x => x.MoveFolderFunc, (id, targetParentFolderId, ct) => files.MoveFolderAsync(id, targetParentFolderId, ct) },
                { x => x.DeleteFolderFunc, (id, ct) => files.DeleteFolderAsync(id, ct) },
                { x => x.MoveFilesFunc, (fileIds, targetFolderId, ct) => files.MoveFilesAsync(new MoveStoredFilesRequest
                    {
                        FileIds = fileIds.ToList(),
                        TargetFolderId = targetFolderId
                    }, ct) },
                { x => x.ContextLabel, title },
            };

            await dialogService.ShowAsync<ManageStoredFilesDialog>(title, parameters, options);
        }
    }
}