namespace Marilog.Ship.Domain
{
    /// <summary>Codes the ship program relies on. They are seeded on the first run — keep them.</summary>
    public static class ShipCodes
    {
        /// <summary>
        /// Document type for cigarettes bought with the ship's cash: in the MGA it is not an expense,
        /// the amount moves from cash on board to cigarettes on board.
        /// </summary>
        public const string CigarettesDocType = "CIGARETTES";
    }
}