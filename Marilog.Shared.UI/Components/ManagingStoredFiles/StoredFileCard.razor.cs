using Marilog.Contracts.DTOs.Responses;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Marilog.Shared.UI.Components.ManagingStoredFiles;

public partial class StoredFileCard
{
    [Parameter, EditorRequired]
    public StoredFileResponse File { get; set; } = default!;

    [Parameter, EditorRequired]
    public bool IsSelected { get; set; }

    [Parameter, EditorRequired]
    public string DownloadUrl { get; set; } = default!;

    [Parameter, EditorRequired]
    public string ThumbnailUrl { get; set; } = default!;

    [Parameter, EditorRequired]
    public EventCallback<StoredFileResponse> OnSelect { get; set; }

    [Parameter, EditorRequired]
    public EventCallback<StoredFileResponse> OnDownload { get; set; }

    [Parameter, EditorRequired]
    public EventCallback<StoredFileResponse> OnDelete { get; set; }

    [Parameter, EditorRequired]
    public EventCallback<(StoredFileResponse File, string Name, string Color)> OnAddTag { get; set; }

    [Parameter, EditorRequired]
    public EventCallback<(StoredFileResponse File, TagResponse Tag)> OnRemoveTag { get; set; }

    [Parameter, EditorRequired]
    public (string Name, Color Value)[] PresetColors { get; set; } = default!;

    private string _newTagName = string.Empty;
    private string _newTagColor = nameof(Color.Primary);

    private string CardClass =>
        IsSelected ? "file-card pa-0 file-card-selected" : "file-card pa-0 file-card-wrapper";

    private bool IsImage =>
        !string.IsNullOrWhiteSpace(File.ContentType)
        && File.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

    private string FileIcon
    {
        get
        {
            var ext = System.IO.Path.GetExtension(File.OriginalFileName).ToLowerInvariant();

            return ext switch
            {
                ".pdf" => Icons.Material.Filled.PictureAsPdf,
                ".doc" or ".docx" => Icons.Material.Filled.Description,
                ".xls" or ".xlsx" or ".csv" => Icons.Material.Filled.TableChart,
                ".ppt" or ".pptx" => Icons.Material.Filled.Slideshow,
                ".zip" or ".rar" or ".7z" => Icons.Material.Filled.FolderZip,
                ".txt" => Icons.Material.Filled.Article,
                _ => Icons.Material.Filled.InsertDriveFile
            };
        }
    }

    private string FormattedSize
    {
        get
        {
            var bytes = File.Size;

            if (bytes < 1024)
                return $"{bytes} B";

            if (bytes < 1024 * 1024)
                return $"{bytes / 1024d:F1} KB";

            return $"{bytes / 1024d / 1024d:F1} MB";
        }
    }

    private static Color ParseColor(string? colorName)
    {
        return Enum.TryParse<Color>(colorName, ignoreCase: true, out var parsed)
            ? parsed
            : Color.Default;
    }

    private Task HandleCardClick()
    {
        return OnSelect.InvokeAsync(File);
    }

    private Task HandleDownloadClick()
    {
        return OnDownload.InvokeAsync(File);
    }

    private Task HandleDeleteClick()
    {
        return OnDelete.InvokeAsync(File);
    }

    private async Task HandleAddTagClick()
    {
        if (string.IsNullOrWhiteSpace(_newTagName))
            return;

        await OnAddTag.InvokeAsync((File, _newTagName.Trim(), _newTagColor));
        _newTagName = string.Empty;
    }
}
