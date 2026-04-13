
namespace Marilog.Contracts.Enums
{
    //Email Enums
    public enum EmailDirection { Outbound, Inbound }
    public enum EmailStatus { Draft, Sent, Received, Failed }
    public enum ParticipantRole { From, To, Cc }
    public enum ParticipantType { Company, Vessel }


    //Payroll Enums
    public enum PayrollStatus { Draft, Approved, PartiallyPaid, FullyPaid, Cancelled }
    public enum PaymentMethod
    {
        CashOnBoard,   // نقداً على ظهر الباخرة   — VoyageId مطلوب
        CashAtOffice,  // نقداً في مكتب الشركة    — City + Country + بيانات المستلم
        BankTransfer   // حوالة بنكية              — SwiftTransferId مطلوب
    }
    public enum DisbursementStatus { Pending, Confirmed, Cancelled }

    //Rank Enums
    public enum Department { DECK, ENGINE, CATERING }

    //Voyage Enums
    public enum VoyageStatus { PLANNED, UNDERWAY, COMPLETED, CANCELLED }

    public class DomainEnums
    {
    }
}
