using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.StoregFileDTOs;
using Microsoft.AspNetCore.Components;

namespace Marilog.Shared.UI.Components.Upload;

public partial class FileUpload
{
    [Parameter] public string? FileName { get; set; }
    [Parameter] public string? ContentType { get; set; }
    [Parameter] public long Size { get; set; }
    [Parameter] public Stream? FileStream { get; set; }

    [Parameter] public EventCallback OnSelectRequested { get; set; }
    [Parameter] public EventCallback<UploadFileRequest> OnFileChanged { get; set; }

    private bool _isDragging;

    private string DropZoneClass => _isDragging
        ? "pa-2 border-2 mud-border-primary"
        : "pa-2";

    private void OnDragEnter() => _isDragging = true;
    private void OnDragLeave() => _isDragging = false;

    private async Task Clear()
    {
        await OnFileChanged.InvokeAsync(new UploadFileRequest());
    }

    private string? GetFormattedSize()
    {
        if (Size == 0) return null;
        if (Size < 1024) return $"{Size} B";
        if (Size < 1_048_576) return $"{Size / 1024.0:F1} KB";
        return $"{Size / 1_048_576.0:F1} MB";
    }
}