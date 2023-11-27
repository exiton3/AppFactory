using System;
using System.Runtime.Serialization;

namespace AppFactory.Framework.Domain.Infrastructure
{
    [DataContract]
    [Serializable]
    public class IdValueModel
    {
        public IdValueModel()
        { }

        public IdValueModel(object id, string value)
        {
            Id = id;
            Value = value;
        }

        [DataMember]
        public object Id { get; set; }

        [DataMember]
        public string Value { get; set; }

        #region Overrides of Object

        public override bool Equals(object obj)
        {
            if (obj is null) return false;

            if (ReferenceEquals(this, obj)) return true;

            if (obj is IdValueModel compareModel)
            {
                return Id.GetHashCode() == compareModel.Id.GetHashCode() &&
                    Value == compareModel.Value;
            }

            return false;
        }

        public override int GetHashCode()
        {
            var hashCodeId = Id?.GetHashCode() ?? 0;
            var hasCodeValue = Value?.GetHashCode() ?? 0;

            return hashCodeId ^ hasCodeValue;
        }

        #endregion
    }


    [DataContract]
    [Serializable]
    public class IdValueModel<TId> : IdValueModel
    {
        public IdValueModel()
        { }

        public IdValueModel(TId id, string value)
            : base(id, value)
        { }

        [DataMember]
        public new TId Id
        {
            get { return (TId)base.Id; }
            set { base.Id = value; }
        }
    }
}
