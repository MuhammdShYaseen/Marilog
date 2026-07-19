
using Marilog.Contracts.DTOs.Requests.StoregFileDTOs;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Kernel.Enums;
using Marilog.Shared.UI.Components.ManagingStoredFiles;
using Marilog.Web.Client.Services.Interfaces;
using Microsoft.AspNetCore.Components.Forms;
using Marilog.Shared.UI.Components.Upload;
using MudBlazor;
namespace Marilog.Web.Client.Services.Implementation
{
    public class ManageStoredFiles : IManageStoredFiles
    {
        private readonly IStoredFileService _storedFileService;
        private readonly HttpClient _http;
        private readonly IDialogService _dialogService;
        private readonly ISnackbar _snackbar;
        public ManageStoredFiles(IStoredFileService storedFileService, HttpClient http, IDialogService dialogService, ISnackbar snackbar)
        {
            _storedFileService = storedFileService;
            _http = http;
            _dialogService = dialogService;
            _snackbar = snackbar;
        }
        public async Task OpenManageStoredFilesDialogAsync((int entityId, EntityType entityType) arg)
        {
            var options = new DialogOptions
            {
                MaxWidth = MaxWidth.Medium,
                FullWidth = true,
                CloseButton = true,
                Position = DialogPosition.Center
            };

            var parameters = new DialogParameters<ManageStoredFilesDialog>
            {
                { x => x.LoadFilesFunc, ct => _storedFileService.GetByEntityIdAsync(arg.entityId, arg.entityType, ct) },
                { x => x.DeleteFileFunc, (id, ct) => _storedFileService.DeleteAsync(id, ct) },
                { x => x.AddTagFunc, (id, name, color, ct) => _storedFileService.AddTagAsync(id, name, color, ct) },
                { x => x.RemoveTagFunc, (id, tagId, ct) => _storedFileService.RemoveTagAsync(id, tagId, ct) },
                { x => x.BuildDownloadUrl, file => $"{_http.BaseAddress}api/StoredFiles/{file.Id}/stream" },
                { x => x.BuildThumbnailUrl, file => $"{_http.BaseAddress}api/StoredFiles/{file.Id}/thumbnailStream" },
                { x => x.SaveContentFunc,(id, content, ct) => UpdateContentAsync(id, content, ct) },
                { x => x.UploadFilesFunc, async (files, ct) =>
                    {
                        var requests = files.Select(f => new UploadFileRequest
                        {
                            ContentType = f.ContentType,
                            EntityId = arg.entityId,
                            EntityType = arg.entityType,
                            FileName = f.Name,
                            FileStream = f.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024),
                            Size = f.Size
                        });

                        var outcome = await _storedFileService.UploadAsync(requests, ct);
                        return outcome.Count > 0;
                    }
                }
            };

            await _dialogService.ShowAsync<ManageStoredFilesDialog>("Manage Documents", parameters, options);
        }



        //use it like this ::  await _dialogManager.OpenUploadFilesDialogAsync((Entity.Id, EntityType.Company), LoadAsync,() => RefreshSelected(Entity.Id));
        public async Task OpenUploadFilesDialogAsync((int entityId, EntityType entityType) arg, Func<Task>? loadAsync, Func<Task>? refreshSelectedAsync)
        {
            var options = new DialogOptions
            {
                MaxWidth = MaxWidth.Medium,      // أكبر من الافتراضي (Small)
                FullWidth = true,
                //DragMode = MudDialogDragMode.Simple,   // هذا يفعّل السحب فعليًا
                CloseButton = true,
                Position = DialogPosition.Center
            };
            var dialog = await _dialogService.ShowAsync<FileUploadDialog>("Attach Files", options);
            var result = await dialog.Result;
            if (result is null || result.Canceled) return;

            var data = (IReadOnlyList<IBrowserFile>)result.Data!;
            IEnumerable<UploadFileRequest> request()
            {
                List<UploadFileRequest> lines = new();
                foreach (var file in data)
                {
                    var uploadRequest = new UploadFileRequest
                    {
                        ContentType = file.ContentType,
                        EntityId = arg.entityId,
                        EntityType = arg.entityType,
                        FileName = file.Name,
                        FileStream = file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024),
                        Size = file.Size
                    };
                    lines.Add(uploadRequest);
                }
                return lines;
            }
            var outcome = await _storedFileService.UploadAsync(request());
            if (outcome.Count > 0)
            {
                _snackbar.Add("File attached.", Severity.Success);
                if(loadAsync != null)
                    await loadAsync();

                if(refreshSelectedAsync != null)
                    await refreshSelectedAsync();
            }
            else
                _snackbar.Add("Error while uploading files", Severity.Error);
        }

        private async Task<bool> UpdateContentAsync(int fileId, string content, CancellationToken ct = default)
        {
            await _storedFileService.UpdateContentFromUserAsync(fileId, content, ct);
            return true;
        }
    }
}
