using Marilog.Contracts.DTOs.Requests.CommentDTOs;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Kernel.Enums;
using Marilog.Shared.UI.Components.Comments;
using Marilog.Web.Client.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using System.Security.Claims;

namespace Marilog.Web.Client.Services.Implementation
{
    public class ManageComments : IManageComments
    {
        private readonly ICommentService _commentService;
        private readonly IDialogService _dialogService;
        private readonly AuthenticationStateProvider _authState;
        private readonly UnreadCommentsState _unread;

        public ManageComments(ICommentService commentService, IDialogService dialogService,
            AuthenticationStateProvider authState, UnreadCommentsState unread)
        {
            _commentService = commentService;
            _dialogService = dialogService;
            _authState = authState;
            _unread = unread;
        }

        public async Task OpenCommentsDialogAsync((int entityId, EntityType entityType) arg, string? contextLabel = null)
        {
            var title = string.IsNullOrWhiteSpace(contextLabel)
                ? $"{arg.entityType} #{arg.entityId}"
                : contextLabel.Trim();

            var state = await _authState.GetAuthenticationStateAsync();
            Guid.TryParse(state.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var currentUserId);

            var options = new DialogOptions
            {
                MaxWidth = MaxWidth.Small,
                FullWidth = true,
                CloseButton = true,
                Position = DialogPosition.Center
            };

            var parameters = new DialogParameters<CommentsDialog>
            {
                { x => x.LoadCommentsFunc, ct => _commentService.GetByEntityAsync(arg.entityType, arg.entityId, ct: ct) },
                { x => x.MarkReadFunc, ct => _commentService.MarkEntityReadAsync(arg.entityType, arg.entityId, ct) },
                { x => x.AddCommentFunc, (text, ct) => _commentService.CreateAsync(new CreateCommentRequest
                    {
                        Text = text,
                        EntityType = arg.entityType,
                        EntityId = arg.entityId
                    }, ct) },
                { x => x.UpdateCommentFunc, (id, text, ct) => _commentService.UpdateAsync(id, new UpdateCommentRequest { Text = text }, ct) },
                { x => x.DeleteCommentFunc, (id, ct) => _commentService.DeleteAsync(id, ct) },
                { x => x.CurrentUserId, currentUserId },
                { x => x.ContextLabel, title },
            };

            var dialog = await _dialogService.ShowAsync<CommentsDialog>(title, parameters, options);
            await dialog.Result;

            // The dialog marked this record read; update the badges right away.
            await _unread.RefreshAsync();
        }
    }
}