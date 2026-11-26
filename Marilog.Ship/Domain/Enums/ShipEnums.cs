namespace Marilog.Ship.Domain.Enums
{
    // Money the owner sent or paid on the master's behalf (MGA page 4).
    public enum OwnerRemittanceType
    {
        CashToMaster = 0,     // cash received on board
        Cigarettes = 1,       // cigarettes received on board (value only)
        CrewPaidAshore = 2,   // wages / allotment / sign-off paid ashore for one crew member
        OtherPaidAshore = 3   // anything else the owner paid for the master
    }

    // What a crew member took on board (MGA page 5).
    public enum CrewWithdrawalType
    {
        Cash = 0,         // cash from the ship's cash
        Cigarettes = 1,   // cigarettes from the ship's stock (value only)
        Other = 2         // deduction without cash (needs a DeductionType + reason)
    }

    // Extra payable on top of wages (MGA page 2).
    public enum CrewExtraType
    {
        Bonus = 0,
        TravelTicket = 1,
        Other = 2
    }

    public enum MgaStatus
    {
        Draft = 0,
        Closed = 1
    }
}