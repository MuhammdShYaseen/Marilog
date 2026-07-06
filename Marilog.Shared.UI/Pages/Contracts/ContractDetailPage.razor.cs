using Marilog.Contracts.DTOs.Requests.ContractDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Shared.UI.Pages.Contracts.Dialogs;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Marilog.Shared.UI.Pages.Contracts;

public partial class ContractDetailPage
{
    [Inject] private IContractService ContractService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private ContractDetailResponse? _detail;
    private bool _loading = true;

    protected override async Task OnParametersSetAsync() => await LoadAsync();

    // ─── Load ──────────────────────────────────────────────────────────────

    private async Task LoadAsync()
    {
        _loading = true;
        _detail = await ContractService.GetByIdAsync(Id);
        _loading = false;
    }

    // ─── Info Actions ──────────────────────────────────────────────────────

    private async Task Activate()
    {
        var result = await ContractService.ActivateAsync(Id);
        if (result.IsSuccess)
        {
            Snackbar.Add("Contract activated.", Severity.Success);
            await LoadAsync();
        }
        else
            Snackbar.Add(result.Error!, Severity.Error);
    }

    private async Task Suspend()
    {
        var result = await ContractService.SuspendAsync(Id, "Suspended by user");
        if (result.IsSuccess)
        {
            Snackbar.Add("Contract suspended.", Severity.Warning);
            await LoadAsync();
        }
        else
            Snackbar.Add(result.Error!, Severity.Error);
    }

    private async Task Terminate()
    {
        var result = await ContractService.TerminateAsync(Id, "Terminated by user");
        if (result.IsSuccess)
        {
            Snackbar.Add("Contract terminated.", Severity.Error);
            await LoadAsync();
        }
        else
            Snackbar.Add(result.Error!, Severity.Error);
    }

    private async Task MarkExpired()
    {
        var result = await ContractService.MarkExpiredAsync(Id);
        if (result.IsSuccess)
        {
            Snackbar.Add("Contract marked as expired.", Severity.Info);
            await LoadAsync();
        }
        else
            Snackbar.Add(result.Error!, Severity.Error);
    }

    // ─── Parties ───────────────────────────────────────────────────────────

    private async Task OpenAddPartyDialog()
    {
        var dialog = await DialogService.ShowAsync<AddPartyDialog>("Add Party");
        var result = await dialog.Result;
        if (result is null || result.Canceled) return;

        var data = (PartyRequest)result.Data!;
        var outcome = await ContractService.AddPartyAsync(Id, data.CompanyId, data.Role);
        if (outcome.IsSuccess)
        {
            Snackbar.Add("Party added.", Severity.Success);
            await LoadAsync();
        }
        else
            Snackbar.Add(outcome.Error!, Severity.Error);
    }

    private async Task OpenRemovePartyDialog(ContractPartyResponse party)
    {
        var parameters = new DialogParameters
        {
            ["CompanyName"] = party.CompanyName,
            ["Role"]        = party.Role
        };

        var dialog = await DialogService.ShowAsync<RemovePartyDialog>("Remove Party", parameters);
        var result = await dialog.Result;
        if (result is null || result.Canceled) return;

        var outcome = await ContractService.RemovePartyAsync(Id, party.CompanyId, party.Role);
        if (outcome.IsSuccess)
        {
            Snackbar.Add("Party removed.", Severity.Success);
            await LoadAsync();
        }
        else
            Snackbar.Add(outcome.Error!, Severity.Error);
    }

    // ─── Amendments ────────────────────────────────────────────────────────

    private async Task OpenRecordAmendmentDialog()
    {
        var dialog = await DialogService.ShowAsync<RecordAmendmentDialog>("Record Amendment");
        var result = await dialog.Result;
        if (result is null || result.Canceled) return;

        var data = (AmendmentRequest)result.Data!;
        var outcome = await ContractService.RecordAmendmentAsync(
            Id,
            data.Description,
            data.EffectiveDate,
            data.ChangedBy
        );

        if (outcome.IsSuccess)
        {
            Snackbar.Add("Amendment recorded.", Severity.Success);
            await LoadAsync();
        }
        else
            Snackbar.Add(outcome.Error!, Severity.Error);
    }

    private async Task OpenChangeExpiryDialog()
    {
        var dialog = await DialogService.ShowAsync<ChangeExpiryDialog>("Change Expiry");
        var result = await dialog.Result;
        if (result is null || result.Canceled) return;

        var data = (ChangeExpiryRequest)result.Data!;
        var outcome = await ContractService.ChangeExpiryAsync(Id, data.NewExpiryDate, data.ChangedBy);
        if (outcome.IsSuccess)
        {
            Snackbar.Add("Expiry changed.", Severity.Success);
            await LoadAsync();
        }
        else
            Snackbar.Add(outcome.Error!, Severity.Error);
    }

    // ─── Documents ─────────────────────────────────────────────────────────

    private async Task OpenAttachFileDialog()
    {
        var dialog = await DialogService.ShowAsync<AttachFileDialog>("Attach File");
        var result = await dialog.Result;
        if (result is null || result.Canceled) return;

        var data = (AttachFileRequest)result.Data!;
        var outcome = await ContractService.AttachFileAsync(Id, data.FileUrl, data.FileName);
        if (outcome.IsSuccess)
        {
            Snackbar.Add("File attached.", Severity.Success);
            await LoadAsync();
        }
        else
            Snackbar.Add(outcome.Error!, Severity.Error);
    }
}
