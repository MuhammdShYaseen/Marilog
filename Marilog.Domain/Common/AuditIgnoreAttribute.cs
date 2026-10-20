

namespace Marilog.Domain.Common
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class AuditIgnoreAttribute : Attribute { }
}
