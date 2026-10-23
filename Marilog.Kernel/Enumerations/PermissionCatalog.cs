using Marilog.Kernel.Enums;

namespace Marilog.Kernel.Enumerations
{
    public static class PermissionCatalog
    {
        public const PermissionAction WriteActions =
            PermissionAction.Create | PermissionAction.Update | PermissionAction.Delete;

        public const PermissionAction AllActions = PermissionAction.Read | WriteActions;

        // Super admin only. Never shown in the roles UI.
        private static readonly HashSet<EntityType> SuperAdminOnly =
        [
            EntityType.NONE,
            EntityType.Email,
            EntityType.EmailAttachment,
            EntityType.StoredFile,
            EntityType.StoredFolder,
            EntityType.User,
            EntityType.AuditLog,
            EntityType.Role,
            EntityType.RolePermission,
            EntityType.UserRole
        ];

        // Readable by every authenticated user. Only Create/Update/Delete are permissions.
        private static readonly HashSet<EntityType> OpenRead =
        [
            EntityType.Country,
            EntityType.Currency,
            EntityType.Rank,
            EntityType.Tag,
            EntityType.DocumentType,
            EntityType.AppTheme,
            EntityType.NavItem,
            EntityType.Company,
            EntityType.Bank,
            EntityType.Vessel,
            EntityType.Port
        ];

        // Ticking Create or Update on one side auto-ticks Read on the other, in both directions.
        private static readonly (EntityType A, EntityType B)[] TwoWay =
        [
            (EntityType.CrewContract, EntityType.CrewPayroll),
            (EntityType.CrewPayroll, EntityType.CrewPayrollDisbursement),

            (EntityType.Document, EntityType.Voyage),
            (EntityType.Document, EntityType.Payment),
            (EntityType.Document, EntityType.SwiftTransfer),
            (EntityType.Document, EntityType.DocumentAdjustment),
            (EntityType.Payment, EntityType.SwiftTransfer),

            (EntityType.Voyage, EntityType.AContract),
            (EntityType.Voyage, EntityType.BillOfLading),
            (EntityType.Voyage, EntityType.CharterTerms),
            (EntityType.Voyage, EntityType.LaytimeCalculation),
            (EntityType.Voyage, EntityType.SofEvent),

            (EntityType.CharterTerms, EntityType.LaytimeCalculation),
            (EntityType.CharterTerms, EntityType.SofEvent),
            (EntityType.LaytimeCalculation, EntityType.LaytimeSegment),
            (EntityType.LaytimeCalculation, EntityType.LaytimeException),
            (EntityType.LaytimeCalculation, EntityType.SofEvent),
            (EntityType.LaytimeSegment, EntityType.SofEvent),
            (EntityType.LaytimeException, EntityType.SofEvent)
        ];

        // One direction only: writing on From auto-ticks Read on To, never the reverse.
        private static readonly (EntityType From, EntityType To)[] OneWay =
        [
            (EntityType.PersonCertificate, EntityType.Person),
            (EntityType.CrewContract, EntityType.Person),
            (EntityType.CrewPayroll, EntityType.Person),
            (EntityType.CrewPayrollDisbursement, EntityType.Person)
        ];

        private static readonly Dictionary<EntityType, HashSet<EntityType>> AutoRead = BuildAutoRead();

        private static readonly HashSet<EntityType> Empty = [];

        public static IReadOnlySet<EntityType> SuperAdminOnlyEntities => SuperAdminOnly;

        public static IReadOnlySet<EntityType> OpenReadEntities => OpenRead;

        // Entities shown in the roles UI.
        public static IReadOnlyList<EntityType> EntityTargets { get; } =
            Enum.GetValues<EntityType>().Where(e => !SuperAdminOnly.Contains(e)).ToList();

        public static IReadOnlyList<AppScreen> ScreenTargets { get; } = Enum.GetValues<AppScreen>();

        public static PermissionAction AllowedActions(EntityType entity)
            => SuperAdminOnly.Contains(entity) ? PermissionAction.None
             : OpenRead.Contains(entity) ? WriteActions
             : AllActions;

        public static PermissionAction AllowedActions(PermissionTargetKind kind, int value) => kind switch
        {
            PermissionTargetKind.Entity when Enum.IsDefined((EntityType)value) => AllowedActions((EntityType)value),
            PermissionTargetKind.Screen when Enum.IsDefined((AppScreen)value) => PermissionAction.Read,
            _ => PermissionAction.None
        };

        // Drops actions the target doesn't support, and forces Read on the same target
        // whenever any write action is granted (where Read is a permission at all).
        public static PermissionAction Normalize(PermissionTargetKind kind, int value, PermissionAction actions)
        {
            var allowed = AllowedActions(kind, value);
            var result = actions & allowed;

            if ((result & WriteActions) != 0 && allowed.HasFlag(PermissionAction.Read))
                result |= PermissionAction.Read;

            return result;
        }

        // Related entities whose Read the roles UI auto-ticks when Create or Update is ticked.
        public static IReadOnlySet<EntityType> GetAutoReadTargets(EntityType entity)
            => AutoRead.TryGetValue(entity, out var set) ? set : Empty;

        private static Dictionary<EntityType, HashSet<EntityType>> BuildAutoRead()
        {
            var map = new Dictionary<EntityType, HashSet<EntityType>>();

            void Add(EntityType from, EntityType to)
            {
                if (!map.TryGetValue(from, out var set))
                    map[from] = set = [];
                set.Add(to);
            }

            foreach (var (a, b) in TwoWay)
            {
                Add(a, b);
                Add(b, a);
            }

            foreach (var (from, to) in OneWay)
                Add(from, to);

            return map;
        }
    }
}
