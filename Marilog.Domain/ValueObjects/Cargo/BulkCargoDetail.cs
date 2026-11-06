namespace Marilog.Domain.ValueObjects.Cargo
{
    /// <summary>
    /// Dry bulk extension of a cargo line. Both figures are optional —
    /// they are the standard figures used in shortage claims.
    /// </summary>
    public sealed class BulkCargoDetail
    {
        public decimal? DraftSurveyMt { get; private set; }
        public decimal? ShoreFigureMt { get; private set; }

        private BulkCargoDetail() { }   // EF Core

        /// <summary>Returns null when both figures are empty — no empty extension rows.</summary>
        public static BulkCargoDetail? Create(decimal? draftSurveyMt, decimal? shoreFigureMt)
        {
            if (draftSurveyMt < 0 || shoreFigureMt < 0)
                throw new ArgumentException("Bulk figures cannot be negative.");

            if (draftSurveyMt is null && shoreFigureMt is null)
                return null;

            return new BulkCargoDetail
            {
                DraftSurveyMt = draftSurveyMt,
                ShoreFigureMt = shoreFigureMt
            };
        }
    }
}