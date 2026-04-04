using Marilog.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Domain.Enumerations
{
    public class ContractStatus : Enumeration
    {
        public static readonly ContractStatus Draft = new(1, "Draft", canBeActivated: true);
        public static readonly ContractStatus Active = new(2, "Active", canBeSuspended: true, canBeTerminated: true);
        public static readonly ContractStatus Expired = new(3, "Expired", isClosed: true);
        public static readonly ContractStatus Terminated = new(4, "Terminated", isClosed: true);
        public static readonly ContractStatus Suspended = new(5, "Suspended", canBeTerminated: true);

        // ─── الصلاحيات مدمجة داخل الحالة نفسها (بدل if/switch في كل مكان)
        public bool CanBeActivated { get; }
        public bool CanBeSuspended { get; }
        public bool CanBeTerminated { get; }
        public bool IsClosed { get; }

        private ContractStatus(
            int id, string name,
            bool canBeActivated = false,
            bool canBeSuspended = false,
            bool canBeTerminated = false,
            bool isClosed = false) : base(id, name)
        {
            CanBeActivated = canBeActivated;
            CanBeSuspended = canBeSuspended;
            CanBeTerminated = canBeTerminated;
            IsClosed = isClosed;
        }


        public static bool TryFromName(string name, out ContractStatus? result)
        {
            result = name?.ToLower() switch
            { 
                "draft" => Draft,
                "active" => Active,
                "Expired" => Expired,
                "Terminated" => Terminated,
                "Suspended" => Suspended,
                _ => null
            };

            return result is not null;
        }
        public static ContractStatus FromId(int id) => id switch
        {
            1 => Draft,
            2 => Active,
            3 => Expired,
            4 => Terminated,
            5 => Suspended,
            _ => throw new ArgumentException($"Invalid ContractStatus id: {id}")
        };
    }
}
