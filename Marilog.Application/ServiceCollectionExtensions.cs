using Marilog.Application.Helpers;
using Marilog.Application.Interfaces.Ai;
using Marilog.Application.Interfaces.Services.Domain;
using Marilog.Application.Interfaces.Services.Laytime;
using Marilog.Application.Services.ApplicationServices.AiServices;
using Marilog.Application.Services.ApplicationServices.AuditLog;
using Marilog.Application.Services.ApplicationServices.DocumentExtraction;
using Marilog.Application.Services.ApplicationServices.LaytimeServices.LaytimeHelppers;
using Marilog.Application.Services.ApplicationServices.LaytimeServices.LaytimeServices;
using Marilog.Application.Services.ApplicationServices.NotifiServices;
using Marilog.Application.Services.ApplicationServices.SystemServices;
using Marilog.Application.Services.ApplicationServices.UserIdenty;
using Marilog.Application.Services.FrontendServices;
using Marilog.Application.Services.MapService;
using Marilog.Contracts.Interfaces.FrontendServices;
using Marilog.Contracts.Interfaces.MapService;
using Marilog.Contracts.Interfaces.NotifiServices;
using Marilog.Contracts.Interfaces.Services.AiProviderServices;
using Marilog.Contracts.Interfaces.Services.AuditLog;
using Marilog.Contracts.Interfaces.Services.CharterLaytimeServices;
using Marilog.Contracts.Interfaces.Services.DocumentExtraction;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Contracts.Interfaces.Services.UserIdenty;
using Microsoft.Extensions.DependencyInjection;


namespace Marilog.Application
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            //----OperatorDashBoard--------------------------------------------------
            services.AddScoped<IOperationsDashboardService, OperationsDashboardService>();

            // ── Lookups ───────────────────────────────────────────────────────────
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<IPortService, PortService>();
            services.AddScoped<IRankService, RankService>();
            services.AddScoped<IDocumentTypeService, DocumentTypeService>();
            services.AddScoped<IContractTypeService, ContractTypeService>();
            services.AddScoped<IContractRoleService, ContractRoleService>();
            services.AddScoped<IOfficeService, OfficeService>();
            services.AddScoped<IBankService, BankService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<ICargoTypeService, CargoTypeService>();
            services.AddScoped<IVesselTypeService, VesselTypeService>();
            // ── Core ──────────────────────────────────────────────────────────────
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IPersonService, PersonService>();
            services.AddScoped<IVesselService, VesselService>();

            // ── Operations ────────────────────────────────────────────────────────
            services.AddScoped<ICrewContractService, CrewContractService>();
            services.AddScoped<IVoyageService, VoyageService>();
            services.AddScoped<IBillOfLadingService, BillOfLadingService>();

            // ── Financial ─────────────────────────────────────────────────────────
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<ISwiftTransferService, SwiftTransferService>();

            // ── Payroll ───────────────────────────────────────────────────────────
            services.AddScoped<ICrewPayrollService, CrewPayrollService>();



            // ── Domain Services ───────────────────────────────────────────────────
            services.AddScoped<IPayrollCalculatorService, PayrollCalculatorService>();
            services.AddScoped<IInboxService, InboxService>();
            services.AddScoped<InboxLinkResolver>();
            services.AddScoped<IRecordLinkService, RecordLinkService>();
            //---Contracts-----------------------
            services.AddScoped<IContractService, ContractService>();

            //----FrontendServices-------------------
            services.AddScoped<INavigationService, NavigationService>();
            services.AddScoped<IAppThemeService, AppThemeService>();

            //----Laytime Services---------------------------
            services.AddScoped<ILaytimeCalculationService, LaytimeCalculationService>();
            services.AddScoped<ILaytimeExceptionService, LaytimeExceptionService>();
            services.AddScoped<ILaytimeQueryService, LaytimeQueryService>();
            services.AddScoped<ILaytimeReportService, LaytimeReportService>();
            services.AddScoped<ICharterTermsService, CharterTermsService>();
            services.AddScoped<ISofEventService, SofEventService>();

            //---AiServices----------------------------------
            services.AddScoped<IAiProviderService, AiProviderService>();
            services.AddScoped<IAiRequestBuilder, GenericAiRequestBuilder>();
            services.AddScoped<IAiCompletionService, AiCompletionService>();
            //----Laytime Helpper----------------------------
            services.AddScoped<ILaytimeHelpper,LaytimeHelpper>();
            services.AddScoped<ILaytimeEngine,LaytimeEngine>();
            services.AddScoped<ILaytimeReportCreator,LaytimeReportCreator>();

            //----Encryption----------------------------------
            //services.AddScoped<ISecretEncryptionService, SecretEncryptionService>();

            //----------------------------------------------------------------------------------------

            //----HttpClient-----------------------------------------------------
            services.AddScoped<HttpClient>();


            //---StorgService----------------------------------------------------
            services.AddScoped<IStoredFileService, StoredFileService>();

            //---FunctionaltyServices--------------------------------------------



            //-- certificate-----------------------------------------------
            services.AddScoped<IVesselCertificateService, VesselCertificateService>();
            services.AddScoped<IPersonCertificateService, PersonCertificateService>();


            //---Weather------------------------------------------------------------
            services.AddScoped<IWeatherService, WeatherService>();


            //----Map---------------------------------------------------------------
            services.AddSingleton<IMapTileService, MapTileService>();


            //-----Audit-----------------------------------------
            services.AddScoped<IAuditLogService, AuditLogService>();

            //-----Identy----------------------------------------
            services.AddScoped<IStoredFileOwnerReader, StoredFileOwnerReader>();


            //----Exchange Rate Refresh Gate---------------------------
            services.AddSingleton<ExchangeRateRefreshGate>();


            //---Inbox Entry----------------------------------
            services.AddScoped<IInboxService, InboxService>();

            //----DocumentExtraction--------------------------
            services.AddScoped<IDocumentExtractionService, DocumentExtractionService>();

            //---Notifi------------------------------------
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<AuditNotificationService>();
            services.AddScoped<DueNotificationService>();
            //----Comments------------------------------------
            services.AddScoped<ICommentService, CommentService>();
            return services;
        }
    }
}
