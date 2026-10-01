using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Enums;

namespace Marilog.Shared.UI.Pages.BillOfLading
{
    public static class BlDisplay
    {
        public static string Type(BlType type) => type switch
        {
            BlType.Straight => "Straight",
            BlType.OrderBl => "Order",
            BlType.BearerBl => "Bearer",
            _ => type.ToString()
        };

        public static string Freight(FreightTerms terms) => terms switch
        {
            FreightTerms.Prepaid => "Prepaid",
            FreightTerms.Collect => "Collect",
            FreightTerms.ThirdParty => "Third party",
            _ => terms.ToString()
        };

        public static string Issuance(BlIssuanceType issuance)
            => issuance == BlIssuanceType.House ? "House" : "Master";

        public static string Consignee(BillOfLadingResponse bl)
        {
            if (bl.BlType == BlType.BearerBl) return "Bearer";
            if (bl.ConsigneeCompany is not null) return bl.ConsigneeCompany.Name;
            return string.IsNullOrWhiteSpace(bl.ConsigneeToOrder) ? "—" : $"To order of {bl.ConsigneeToOrder}";
        }
    }
}