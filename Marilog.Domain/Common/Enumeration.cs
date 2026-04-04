using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Domain.Common
{
    public abstract class Enumeration : IComparable
    {
        public int Id { get; }
        public string Name { get; }

        protected Enumeration(int id, string name) => (Id, Name) = (id, name);

        public override string ToString() => Name;
        public override bool Equals(object? obj) =>
            obj is Enumeration other && GetType() == other.GetType() && Id == other.Id;
        public override int GetHashCode() => Id.GetHashCode();
        public int CompareTo(object? obj) => Id.CompareTo(((Enumeration)obj!).Id);
    }
}
