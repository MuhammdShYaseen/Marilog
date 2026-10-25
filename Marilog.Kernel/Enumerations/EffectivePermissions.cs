using Marilog.Kernel.Enums;

namespace Marilog.Kernel.Enumerations
{
    public sealed class EffectivePermissions
    {
        private readonly bool _isAuthenticated;
        private readonly bool _isSuperAdmin;
        private readonly Dictionary<(PermissionTargetKind Kind, int Value), PermissionAction> _granted = new();

        // Anonymous: nothing is allowed, not even open-read entities.
        public static EffectivePermissions None { get; } =
            new(isAuthenticated: false, isSuperAdmin: false, Array.Empty<(PermissionTargetKind, int, PermissionAction)>());

        public static EffectivePermissions SuperAdmin { get; } =
            new(isAuthenticated: true, isSuperAdmin: true, Array.Empty<(PermissionTargetKind, int, PermissionAction)>());

        // A regular, authenticated user. Grants from several roles are merged (union).
        public EffectivePermissions(IEnumerable<(PermissionTargetKind Kind, int Value, PermissionAction Actions)> grants)
            : this(isAuthenticated: true, isSuperAdmin: false, grants) { }

        private EffectivePermissions(
            bool isAuthenticated,
            bool isSuperAdmin,
            IEnumerable<(PermissionTargetKind Kind, int Value, PermissionAction Actions)> grants)
        {
            _isAuthenticated = isAuthenticated;
            _isSuperAdmin = isSuperAdmin;

            foreach (var (kind, value, actions) in grants)
            {
                var key = (kind, value);
                _granted[key] = _granted.TryGetValue(key, out var existing) ? existing | actions : actions;
            }
        }

        public bool IsSuperAdmin => _isSuperAdmin;

        public IReadOnlyList<(PermissionTargetKind Kind, int Value, PermissionAction Actions)> Grants
            => _granted.Select(g => (g.Key.Kind, g.Key.Value, g.Value)).ToList();

        public bool Can(EntityType entity, PermissionAction action)
            => Can(PermissionTargetKind.Entity, (int)entity, action);

        public bool Can(AppScreen screen)
            => Can(PermissionTargetKind.Screen, (int)screen, PermissionAction.Read);

        public bool Can(PermissionTargetKind kind, int value, PermissionAction action)
        {
            if (!_isAuthenticated || action == PermissionAction.None) return false;
            if (_isSuperAdmin) return true;

            // Read on an open-read entity: every authenticated user.
            if (kind == PermissionTargetKind.Entity
                && action == PermissionAction.Read
                && PermissionCatalog.OpenReadEntities.Contains((EntityType)value))
                return true;

            // Actions the catalog doesn't offer for this target (e.g. super-admin-only entities) are never granted.
            var allowed = PermissionCatalog.AllowedActions(kind, value);
            if ((allowed & action) != action) return false;

            return _granted.TryGetValue((kind, value), out var actions) && (actions & action) == action;
        }
    }
}