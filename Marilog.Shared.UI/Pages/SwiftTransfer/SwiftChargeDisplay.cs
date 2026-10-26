using Marilog.Kernel.Enums;

namespace Marilog.Shared.UI.Pages.SwiftTransfer
{
    public static class SwiftChargeDisplay
    {
        public static string Bearer(SwiftChargeBearer bearer) => bearer switch
        {
            SwiftChargeBearer.Our => "OUR — sender pays all",
            SwiftChargeBearer.Sha => "SHA — shared",
            SwiftChargeBearer.Ben => "BEN — beneficiary pays all",
            _ => "Not recorded"
        };

        public static string Stage(SwiftFeeStage stage) => stage switch
        {
            SwiftFeeStage.SenderBank => "Sender bank",
            SwiftFeeStage.IntermediaryBank => "Intermediary bank",
            SwiftFeeStage.ReceiverBank => "Receiver bank",
            _ => stage.ToString()
        };
    }
}