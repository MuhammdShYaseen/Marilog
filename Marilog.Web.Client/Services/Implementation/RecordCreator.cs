
using Marilog.Contracts.DTOs.Requests.DocumentDTOs;
using Marilog.Contracts.DTOs.Requests.SwiftTransferDTOs;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Shared.UI.Pages.Documents;
using Marilog.Shared.UI.Pages.SwiftTransfer;
using Marilog.Web.Client.Services.Interfaces;
using MudBlazor;

namespace Marilog.Web.Client.Services.Implementation
{
    public class RecordCreator(
    IDialogService dialogService,
    ISnackbar snackbar,
    IDocumentService documentService,
    IVoyageService voyageService,
    IVesselService vesselService,
    ICompanyService companyService,
    IDocumentTypeService docTypeService,
    ICurrencyService currencyService,
    IPortService portService,
    ISwiftTransferService swiftTransferService,
    IBankService bankService) : IRecordCreator
    {
        public Task<int?> CreateAdjustmentAsync(int documentId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<int?> CreateDocumentAsync(CreateDocumentRequest? prefill = null, CancellationToken ct = default)
        {
            try
            {
                var vessels = vesselService.GetAllAsLookUpAsync();
                var companies = companyService.GetActiveAsync();
                var docTypes = docTypeService.GetActiveAsync();
                var currencies = currencyService.GetActiveAsync();
                var ports = portService.GetActiveAsync();

                await Task.WhenAll(vessels, companies, docTypes, currencies, ports);

                var parameters = new DialogParameters
                {
                    ["DocTypes"] = docTypes.Result,
                    ["Companies"] = companies.Result,
                    ["Currencies"] = currencies.Result,
                    ["Vessels"] = vessels.Result,
                    ["Ports"] = ports.Result,
                    ["GetDocsByVessel"] = async (int vesselId) => await documentService.GetByVesselAsync(vesselId),
                    ["GetVoyagesByVessel"] = async (int vesselId) => await voyageService.GetByVesselAsync(vesselId),
                    ["GetDocByNumber"] = async (string docNumber) => await documentService.GetByNumberAsync(docNumber)
                };

                if (prefill is not null)
                    parameters["Prefill"] = prefill;

                var dialog = await dialogService.ShowAsync<DocumentCreateWizard>("New Document", parameters, new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true });
                var result = await dialog.Result;
                if (result is null || result.Canceled || result.Data is null) return null;

                var created = await documentService.CreateAsync((CreateDocumentRequest)result.Data, ct);
                return created.Id;
            }
            catch (Exception ex)
            {
                snackbar.Add(ex.Message, Severity.Error);
                return null;
            }
        }

        public Task<int?> CreatePaymentAsync(int documentId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<int?> CreateSwiftTransferAsync(CreateSwiftTransferRequest? prefill = null, CancellationToken ct = default)
        {
            try
            {
                var companies = companyService.GetActiveAsync();
                var banks = bankService.GetAllActiveAsync();
                var currencies = currencyService.GetActiveAsync();

                await Task.WhenAll(companies, banks, currencies);

                var parameters = new DialogParameters
                {
                    ["Companies"] = companies.Result,
                    ["Banks"] = banks.Result,
                    ["Currencies"] = currencies.Result,
                    ["GetByReference"] = async (string reference) => await swiftTransferService.GetByReferenceAsync(reference)
                };

                if (prefill is not null)
                    parameters["Prefill"] = prefill;

                var dialog = await dialogService.ShowAsync<SwiftCreateWizard>("New SWIFT Transfer", parameters,
                    new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true });
                var result = await dialog.Result;
                if (result is null || result.Canceled || result.Data is null) return null;

                var r = (CreateSwiftTransferRequest)result.Data;
                var created = await swiftTransferService.CreateAsync(
                    r.SwiftReference, r.TransactionDate, r.CurrencyId, r.Amount,
                    r.SenderCompanyId, r.ReceiverCompanyId, r.SenderBankId, r.ReceiverBankId,
                    r.PaymentReference, r.RawMessage, ct);

                snackbar.Add($"SWIFT transfer {created.SwiftReference} created.", Severity.Success);
                return created.Id;
            }
            catch (Exception ex)
            {
                snackbar.Add(ex.Message, Severity.Error);
                return null;
            }
        }
    }
}
