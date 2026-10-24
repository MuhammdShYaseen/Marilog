using Marilog.Kernel.Enums;

namespace Marilog.Infrastructure.Helpers.UserIdentity
{
    // Marks a controller's entity. PermissionConvention maps each action's HTTP verb to an action on it.
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class PermissionEntityAttribute(EntityType entity) : Attribute
    {
        public EntityType Entity { get; } = entity;
    }
}