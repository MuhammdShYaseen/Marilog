namespace Marilog.Kernel.Rules
{
    /// <summary>
    /// The single crew wage rule — shared by Marilog payroll (CrewPayroll, CrewContract,
    /// CrewContractService) and the ship program, so both always agree.
    ///
    ///   • Service days are inclusive: sign-on day and sign-off day both count.
    ///   • A full calendar month of service = full MonthlyWage (28, 29, 30 or 31 days).
    ///   • A part month = MonthlyWage × days ÷ 30.
    ///   • Rounded to 2 decimals (AwayFromZero), once per calendar month.
    ///
    /// A period that cuts a month (master handover, vessel sale) gets the month's wage earned
    /// up to its last day minus the wage earned before its first day, so the parts of a month
    /// always add up to exactly the month's wage.
    /// </summary>
    public static class CrewWageRules
    {
        public const int DaysPerMonth = 30;

        /// <summary>Days served inside [from..to], both ends inclusive. 0 when there is no overlap.</summary>
        public static int CountServiceDays(DateOnly signOn, DateOnly? signOff, DateOnly from, DateOnly to)
        {
            var start = signOn > from ? signOn : from;
            var end = signOff is { } off && off < to ? off : to;

            return end < start ? 0 : end.DayNumber - start.DayNumber + 1;
        }

        /// <summary>Wage for one calendar month (any day of the month may be passed).</summary>
        public static decimal MonthWage(decimal monthlyWage, DateOnly signOn, DateOnly? signOff, DateOnly month)
        {
            EnsureWage(monthlyWage);
            return WageUpTo(monthlyWage, signOn, signOff, FirstDay(month), LastDay(month));
        }

        /// <summary>Wage for any period [from..to], split by calendar month.</summary>
        public static decimal PeriodWage(decimal monthlyWage, DateOnly signOn, DateOnly? signOff,
                                         DateOnly from, DateOnly to)
        {
            EnsureWage(monthlyWage);
            if (to < from)
                throw new ArgumentException("Period end cannot be before period start.");

            var total = 0m;
            for (var month = FirstDay(from); month <= to; month = month.AddMonths(1))
            {
                var pieceStart = from > month ? from : month;
                var pieceEnd = to < LastDay(month) ? to : LastDay(month);

                total += WageUpTo(monthlyWage, signOn, signOff, month, pieceEnd)
                       - WageUpTo(monthlyWage, signOn, signOff, month, pieceStart.AddDays(-1));
            }
            return total;
        }

        // Wage earned from the 1st of 'firstDayOfMonth' up to 'upTo' (inclusive).
        private static decimal WageUpTo(decimal monthlyWage, DateOnly signOn, DateOnly? signOff,
                                        DateOnly firstDayOfMonth, DateOnly upTo)
        {
            if (upTo < firstDayOfMonth) return 0m;

            var days = CountServiceDays(signOn, signOff, firstDayOfMonth, upTo);
            if (days == 0) return 0m;

            var isFullMonth = days == DateTime.DaysInMonth(firstDayOfMonth.Year, firstDayOfMonth.Month);
            return Math.Round(
                isFullMonth ? monthlyWage : monthlyWage * days / DaysPerMonth,
                2, MidpointRounding.AwayFromZero);
        }

        private static DateOnly FirstDay(DateOnly date) => new(date.Year, date.Month, 1);
        private static DateOnly LastDay(DateOnly date) => FirstDay(date).AddMonths(1).AddDays(-1);

        private static void EnsureWage(decimal monthlyWage)
        {
            if (monthlyWage < 0)
                throw new ArgumentException("MonthlyWage cannot be negative.");
        }
    }
}