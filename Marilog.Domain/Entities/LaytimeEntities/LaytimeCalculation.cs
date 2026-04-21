using Marilog.Domain.Common;
using Marilog.Domain.ValueObjects.Laytime;
using Marilog.Kernel.Enums;
using Marilog.Kernel.Primitives;

namespace Marilog.Domain.Entities.LaytimeEntities
{
    /// <summary>
    /// Aggregate Root لحساب الـ Laytime لعملية شحن في ميناء محدد.
    /// يملك: SOF Events → Segments → Exceptions → Result.
    /// دورة الحياة: Draft → Computed → Finalized.
    /// </summary>
    public class LaytimeCalculation : Entity
    {
        public int VoyageId { get; private set; }
        public int ContractId { get; private set; }
        public int PortId { get; private set; }
        public OperationType OperationType { get; private set; }
        public decimal CargoQuantityMt { get; private set; }
        public LaytimeStatus Status { get; private set; }

        /// <summary>اللحظة الفعلية لبدء احتساب الـ Laytime (بعد NOR + Notice Period)</summary>
        public DateTime? LaytimeCommencedAt { get; private set; }

        /// <summary>اللحظة الفعلية لانتهاء احتساب الـ Laytime</summary>
        public DateTime? LaytimeCompletedAt { get; private set; }

        private readonly List<SofEvent> _sofEvents = [];
        private readonly List<LaytimeSegment> _segments = [];
        private readonly List<LaytimeException> _exceptions = [];

        public IReadOnlyCollection<SofEvent> SofEvents => _sofEvents;
        public IReadOnlyCollection<LaytimeSegment> Segments => _segments;
        public IReadOnlyCollection<LaytimeException> Exceptions => _exceptions;

        public LaytimeResult? Result { get; private set; }

        private LaytimeCalculation() { }

        // ─────────────────────────────
        // Factory
        // ─────────────────────────────

        public static LaytimeCalculation Create(
            int voyageId,
            int contractId,
            int portId,
            OperationType operationType,
            decimal cargoQuantityMt)
        {
            if (cargoQuantityMt <= 0)
                throw new ArgumentException(
                    "Cargo quantity must be greater than zero.", nameof(cargoQuantityMt));

            return new LaytimeCalculation
            {
                VoyageId = voyageId,
                ContractId = contractId,
                PortId = portId,
                OperationType = operationType,
                CargoQuantityMt = cargoQuantityMt,
                Status = LaytimeStatus.Draft
            };
        }

        // ─────────────────────────────
        // SOF Events
        // ─────────────────────────────

        public Result AddSofEvent(SofEvent sofEvent)
        {
            ArgumentNullException.ThrowIfNull(sofEvent);
            EnsureNotFinalized();

            if (_sofEvents.Any(e => e.EventTime == sofEvent.EventTime))
                return Kernel.Primitives.Result.Fail(
                    $"A SOF event already exists at {sofEvent.EventTime:yyyy-MM-dd HH:mm}.");

            _sofEvents.Add(sofEvent);
            return Kernel.Primitives.Result.Ok();
        }

        public Result RemoveSofEvent(int sofEventId)
        {
            EnsureNotFinalized();

            var ev = _sofEvents.FirstOrDefault(e => e.Id == sofEventId);
            if (ev is null)
                return Kernel.Primitives.Result.Fail("SOF event not found.");

            _sofEvents.Remove(ev);
            return Kernel.Primitives.Result.Ok();
        }

        // ─────────────────────────────
        // Segments (يبنيها Engine — ليست يدوية)
        // ─────────────────────────────

        public void AddSegment(LaytimeSegment segment)
        {
            ArgumentNullException.ThrowIfNull(segment);
            EnsureNotFinalized();
            _segments.Add(segment);
        }

        public void ClearSegments()
        {
            EnsureNotFinalized();
            _segments.Clear();
        }

        // ─────────────────────────────
        // Exceptions
        // ─────────────────────────────

        public Result AddException(LaytimeException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            EnsureNotFinalized();

            if (_exceptions.Any(e =>
                    e.ExceptionType == exception.ExceptionType &&
                    e.From == exception.From &&
                    e.To == exception.To))
                return Kernel.Primitives.Result.Fail("An identical exception already exists.");

            _exceptions.Add(exception);
            return Kernel.Primitives.Result.Ok();
        }

        public Result RemoveException(int exceptionId)
        {
            EnsureNotFinalized();

            var exception = _exceptions.FirstOrDefault(e => e.Id == exceptionId);
            if (exception is null)
                return Kernel.Primitives.Result.Fail("Exception not found.");

            _exceptions.Remove(exception);
            return Kernel.Primitives.Result.Ok();
        }

        // ─────────────────────────────
        // Laytime Period
        // ─────────────────────────────

        public void SetLaytimePeriod(DateTime commenced, DateTime completed)
        {
            EnsureNotFinalized();

            if (commenced == default)
                throw new ArgumentException("Commenced date is required.", nameof(commenced));

            if (completed <= commenced)
                throw new ArgumentException(
                    "Completed must be after commenced.", nameof(completed));

            LaytimeCommencedAt = commenced;
            LaytimeCompletedAt = completed;
        }

        // ─────────────────────────────
        // Result & Status
        // ─────────────────────────────

        public Result SetResult(LaytimeResult result)
        {
            EnsureNotFinalized();

            if (_sofEvents.Count < 2)
                return Kernel.Primitives.Result.Fail(
                    "At least 2 SOF events are required before setting result.");

            if (_segments.Count == 0)
                return Kernel.Primitives.Result.Fail(
                    "Segments must be built before setting result.");

            Result = result;
            Status = LaytimeStatus.Computed;
            return Kernel.Primitives.Result.Ok();
        }

        public Result FinalizeCalculation()
        {
            if (Status != LaytimeStatus.Computed)
                return Kernel.Primitives.Result.Fail("Calculation must be computed before finalizing.");

            if (Result is null)
                return Kernel.Primitives.Result.Fail("Cannot finalize without a computed result.");

            Status = LaytimeStatus.Finalized;
            return Kernel.Primitives.Result.Ok();
        }

        public Result Recompute()
        {
            if (Status == LaytimeStatus.Finalized)
                return Kernel.Primitives.Result.Fail("Cannot recompute a finalized calculation.");

            _segments.Clear();
            Result = null;
            Status = LaytimeStatus.Draft;
            LaytimeCommencedAt = null;
            LaytimeCompletedAt = null;

            return Kernel.Primitives.Result.Ok();
        }

        // ─────────────────────────────
        // Updates
        // ─────────────────────────────

        public void UpdateCargoQuantity(decimal quantity)
        {
            EnsureNotFinalized();

            if (quantity <= 0)
                throw new ArgumentException(
                    "Cargo quantity must be greater than zero.", nameof(quantity));

            CargoQuantityMt = quantity;
        }

        public void UpdateOperationType(OperationType operationType)
        {
            EnsureNotFinalized();
            OperationType = operationType;
        }

        // ─────────────────────────────
        // Private Guards
        // ─────────────────────────────

        private void EnsureNotFinalized()
        {
            if (Status == LaytimeStatus.Finalized)
                throw new InvalidOperationException(
                    "Cannot modify a finalized calculation.");
        }
    }
}