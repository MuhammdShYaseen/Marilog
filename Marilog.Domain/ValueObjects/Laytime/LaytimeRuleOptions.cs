using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Domain.ValueObjects.Laytime
{
    public class LaytimeRuleOptions
    {
        public bool DraftSurveyCounts { get; private set; }

        public bool HolidaysIncluded { get; private set; }

        public bool SundaysIncluded { get; private set; }

        public bool TimeReversible { get; private set; }

        public bool AllowShiftingTime { get; private set; }

        private LaytimeRuleOptions()
        {
            
        }
        public static LaytimeRuleOptions Create(bool draftSurveyCounts, bool holidaysIncluded, bool sundaysIncluded, bool timeReversible, bool allowShiftingTime)
        {
            return new LaytimeRuleOptions
            {
                DraftSurveyCounts = draftSurveyCounts,
                HolidaysIncluded = holidaysIncluded,
                SundaysIncluded = sundaysIncluded,
                TimeReversible = timeReversible,
                AllowShiftingTime = allowShiftingTime
            };
        }

        public static LaytimeRuleOptions CreateDefault() =>
    new()
    {
        DraftSurveyCounts = false,
        HolidaysIncluded = false,
        SundaysIncluded = true,
        TimeReversible = false,
        AllowShiftingTime = false
    };
    }
}
