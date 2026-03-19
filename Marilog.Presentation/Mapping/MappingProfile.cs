using AutoMapper;
using Marilog.Domain.Entities;

namespace Marilog.Presentation.Mapping
{
    public class MappingProfile : Profile
    {
        /*public MappingProfile()
        {
            // ── Lookups ───────────────────────────────────────────────────────────
            CreateMap<Country, CountryResponse>()
                .ConstructUsing(x => new CountryResponse(
                    x.CountryID, x.CountryCode, x.CountryName, x.IsActive));

            CreateMap<Currency, CurrencyResponse>()
                .ConstructUsing(x => new CurrencyResponse(
                    x.Id, x.CurrencyCode, x.CurrencyName, x.Symbol,
                    x.ExchangeRate, x.IsBaseCurrency, x.IsActive));

            CreateMap<Port, PortResponse>()
                .ConstructUsing(x => new PortResponse(
                    x.PortID, x.PortCode, x.PortName,
                    x.CountryID, x.Country != null ? x.Country.CountryName : null,
                    x.IsActive));

            CreateMap<Rank, RankResponse>()
                .ConstructUsing(x => new RankResponse(
                    x.RankID, x.RankCode, x.RankName,
                    x.Department.ToString(), x.IsActive));

            CreateMap<Office, OfficeResponse>()
                .ConstructUsing(x => new OfficeResponse(
                    x.Id, x.OfficeName, x.City,
                    x.CountryId, x.Country != null ? x.Country.CountryName : null,
                    x.Address, x.Phone, x.ContactName, x.IsActive));

            // ── Core ──────────────────────────────────────────────────────────────
            CreateMap<Company, CompanyResponse>()
                .ConstructUsing(x => new CompanyResponse(
                    x.CompanyID, x.CompanyName, null,
                    x.ContactName, x.Email, x.Phone, x.Address, x.IsActive));

            CreateMap<Person, PersonResponse>()
                .ConstructUsing(x => new PersonResponse(
                    x.PersonID, x.FullName,
                    x.Nationality,
                    x.NationalityCountry != null ? x.NationalityCountry.CountryName : null,
                    x.PassportNo, x.PassportExpiry, x.IsPassportExpired(),
                    x.SeamanBookNo, x.DateOfBirth,
                    x.Phone, x.Email,
                    x.BankName, x.IBAN, x.BankSwiftCode,
                    x.IsActive));

            CreateMap<Vessel, VesselResponse>()
                .ConstructUsing(x => new VesselResponse(
                    x.VesselID, x.VesselName, x.IMONumber, x.GrossTonnage,
                    x.CompanyID,
                    x.Company != null ? x.Company.CompanyName : null,
                    x.FlagCountryID,
                    x.FlagCountry != null ? x.FlagCountry.CountryName : null,
                    x.IsActive));

            // ── Operations ────────────────────────────────────────────────────────
            CreateMap<CrewContract, CrewContractResponse>()
                .ConstructUsing(x => new CrewContractResponse(
                    x.ContractID,
                    x.PersonID, x.Person != null ? x.Person.FullName : string.Empty,
                    x.VesselID, x.Vessel != null ? x.Vessel.VesselName : string.Empty,
                    x.RankID,
                    x.Rank != null ? x.Rank.RankName : string.Empty,
                    x.Rank != null ? x.Rank.Department.ToString() : string.Empty,
                    x.MonthlyWage,
                    x.SignOnDate, x.SignOffDate,
                    x.SignOnPort, x.SignOnPortNav != null ? x.SignOnPortNav.PortName : null,
                    x.SignOffPort, x.SignOffPortNav != null ? x.SignOffPortNav.PortName : null,
                    x.IsActive,
                    x.ContractDurationDays(),
                    x.TotalWageEarned()));

            CreateMap<VoyageStop, VoyageStopResponse>()
                .ConstructUsing(x => new VoyageStopResponse(
                    x.StopOrder,
                    x.PortID,
                    x.Port != null ? x.Port.PortName : string.Empty,
                    x.ArrivalDate, x.DepartureDate,
                    x.PurposeOfCall, x.Notes));

            CreateMap<Voyage, VoyageResponse>()
                .ConstructUsing((x, ctx) => new VoyageResponse(
                    x.VoyageID, x.VoyageNumber,
                    x.VesselID,
                    x.Vessel != null ? x.Vessel.VesselName : string.Empty,
                    x.VoyageMonth,
                    x.Status.ToString(),
                    x.MasterContractID,
                    x.MasterContract?.Person != null ? x.MasterContract.Person.FullName : null,
                    x.DeparturePortID,
                    x.DeparturePort != null ? x.DeparturePort.PortName : null,
                    x.ArrivalPortID,
                    x.ArrivalPort != null ? x.ArrivalPort.PortName : null,
                    x.DepartureDate, x.ArrivalDate,
                    x.CargoType, x.CargoQuantityMT,
                    x.CashOnBoard, x.CigarettesOnBoard, x.PreviousMasterBalance,
                    x.Notes,
                    ctx.Mapper.Map<IReadOnlyList<VoyageStopResponse>>(x.Stops)));

            // ── Financial ─────────────────────────────────────────────────────────
            CreateMap<DocumentItem, DocumentItemResponse>()
                .ConstructUsing(x => new DocumentItemResponse(
                    x.Id, x.ProductName, x.Quantity,
                    x.Unit, x.UnitPrice, x.LineTotal));

            CreateMap<Document, DocumentResponse>()
                .ConstructUsing((x, ctx) => new DocumentResponse(
                    x.Id, x.DocNumber,
                    x.DocTypeId,
                    x.DocType != null ? x.DocType.Name : string.Empty,
                    x.DocDate,
                    x.SupplierId, x.Supplier != null ? x.Supplier.CompanyName : null,
                    x.BuyerId, x.Buyer != null ? x.Buyer.CompanyName : null,
                    x.VesselId, x.Vessel != null ? x.Vessel.VesselName : null,
                    x.PortId, x.Port != null ? x.Port.PortName : null,
                    x.CurrencyId,
                    x.Currency != null ? x.Currency.CurrencyCode : string.Empty,
                    x.TotalAmount, x.TotalPaid, x.RemainingBalance, x.IsFullyPaid,
                    x.Reference, x.FilePath,
                    x.ParentDocumentId, x.IsActive,
                    ctx.Mapper.Map<IReadOnlyList<DocumentItemResponse>>(x.Items)));

            CreateMap<SwiftTransfer, SwiftTransferResponse>()
                .ConstructUsing(x => new SwiftTransferResponse(
                    x.Id, x.SwiftReference, x.TransactionDate,
                    x.CurrencyId,
                    x.Currency != null ? x.Currency.CurrencyCode : string.Empty,
                    x.Amount, x.AllocatedAmount, x.UnallocatedAmount, x.IsFullyAllocated,
                    x.SenderCompanyId,
                    x.SenderCompany != null ? x.SenderCompany.CompanyName : null,
                    x.ReceiverCompanyId,
                    x.ReceiverCompany != null ? x.ReceiverCompany.CompanyName : null,
                    x.SenderBank, x.ReceiverBank, x.PaymentReference, x.IsActive));

            // ── Payroll ───────────────────────────────────────────────────────────
            CreateMap<CrewPayrollDisbursement, DisbursementResponse>()
                .ConstructUsing(x => new DisbursementResponse(
                    x.Id,
                    x.Method.ToString(),
                    x.Amount, x.PaidOn,
                    x.Status.ToString(),
                    x.VoyageId,
                    x.Voyage != null ? x.Voyage.VoyageNumber : null,
                    x.OfficeId,
                    x.Office != null ? x.Office.OfficeName : null,
                    x.RecipientName, x.RecipientIdNumber,
                    x.SwiftTransferId,
                    x.SwiftTransfer != null ? x.SwiftTransfer.SwiftReference : null,
                    x.Notes, x.CancelReason));

            CreateMap<CrewPayroll, CrewPayrollResponse>()
                .ConstructUsing((x, ctx) => new CrewPayrollResponse(
                    x.Id,
                    x.ContractId,
                    x.Contract?.PersonID ?? 0,
                    x.Contract?.Person?.FullName ?? string.Empty,
                    x.Contract?.VesselID ?? 0,
                    x.Contract?.Vessel?.VesselName ?? string.Empty,
                    x.PayrollMonth,
                    x.WorkingDays,
                    x.BasicWage, x.Allowances, x.Deductions, x.GrossAmount,
                    x.TotalDisbursed, x.RemainingBalance, x.IsFullyPaid,
                    x.Status.ToString(),
                    x.Notes,
                    ctx.Mapper.Map<IReadOnlyList<DisbursementResponse>>(x.Disbursements)));
        }
    */}
}