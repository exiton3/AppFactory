using System.Text.Json.Serialization;

namespace AppFactory.Framework.Domain.Entities
{
    public abstract class EntityWithTypedId<TId>
    {
        [JsonPropertyOrder(-1)]
        public virtual TId Id { get; set; }

        protected bool Equals(EntityWithTypedId<TId> other)
        {
            return EqualityComparer<TId>.Default.Equals(Id, other.Id);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((EntityWithTypedId<TId>)obj);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<TId>.Default.GetHashCode(Id);
        }

        public static bool operator ==(EntityWithTypedId<TId> left, EntityWithTypedId<TId> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EntityWithTypedId<TId> left, EntityWithTypedId<TId> right)
        {
            return !Equals(left, right);
        }
    }

}