using Marilog.Domain.Entities.LaytimeEntities;
using System.ComponentModel.DataAnnotations.Schema;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.ValueObjects.Laytime
{
    /// <summary>
    /// شروط عملية شحن واحدة (تحميل أو تفريغ) — مستخرجة من Charter Party.
    /// Value Object غير قابل للتعديل.
    /// </summary>
    public class CargoOperationTerms
    {
        public OperationType OperationType { get; private set; }

        /// <summary>معدل التحميل/التفريغ بالطن/يوم</summary>
        public decimal RateMtPerDay { get; private set; }

        /// <summary>نوع التقويم المحدد في Charter Party (SHINC / SHEX / SSHINC / WWD)</summary>
        public LaytimeCalendarType CalendarType { get; private set; }

        /// <summary>فترة الإشعار بالساعات قبل بدء احتساب الـ Laytime</summary>
        public int NoticeHours { get; private set; }

        /// <summary>
        /// هل يمكن جمع وقت Loading و Discharging في حساب واحد؟
        /// (Reversible Laytime في بعض Charter Parties)
        /// </summary>
        public bool IsReversible { get; private set; }

        /// <summary>
        /// مشتق من CalendarType — لا يحتاج تخزيناً مستقلاً.
        /// true إذا كان CalendarType = WeatherWorkingDay.
        /// </summary>
        /// 
        [NotMapped]
        public bool IsWeatherWorkingDay =>
            CalendarType == LaytimeCalendarType.WeatherWorkingDay;

        private CargoOperationTerms() { } // للـ EF Core


        public static CargoOperationTerms Create(
            OperationType operationType,
            decimal rateMtPerDay,
            LaytimeCalendarType calendarType,
            int noticeHours,
            bool isReversible = false)
        {
            if (rateMtPerDay <= 0)
                throw new ArgumentException(
                    "Rate must be greater than zero.", nameof(rateMtPerDay));

            if (noticeHours < 0)
                throw new ArgumentException(
                    "Notice hours cannot be negative.", nameof(noticeHours));

            return new CargoOperationTerms
            {
                OperationType = operationType,
                RateMtPerDay = rateMtPerDay,
                CalendarType = calendarType,
                NoticeHours = noticeHours,
                IsReversible = isReversible
            };
        }

        // ─── Equality ───

        public override bool Equals(object? obj) =>
            obj is CargoOperationTerms other &&
            OperationType == other.OperationType &&
            RateMtPerDay == other.RateMtPerDay &&
            CalendarType == other.CalendarType &&
            NoticeHours == other.NoticeHours &&
            IsReversible == other.IsReversible;

        public override int GetHashCode() =>
            HashCode.Combine(OperationType, RateMtPerDay, CalendarType, NoticeHours, IsReversible);
    }
}