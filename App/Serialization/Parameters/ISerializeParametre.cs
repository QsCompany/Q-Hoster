using System;
using models;

namespace Server
{
    public interface ISerializeParametre
    {
        Type FromType { get; set; }
        Type ToType { get; set; }
        bool StringifyType { get; }
        bool StringifyRef { get; }
    }

    public class  DObjectParameter : ISerializeParametre
    {
        public DObjectParameter Super { get; set; }

        private PropertyAttribute?[] attributes;


        public PropertyAttribute? this[int property]
        {
            get => property < attributes.Length ? attributes[property] : null;
            set => attributes[property] = value;
        }

        public bool? DIsFrozen;
        public virtual bool? IsFrozen(object p)
        {
            return DIsFrozen;
        }

        public Type FromType { get; set; }
        public Type ToType { get; set; }

        public Type BaseType { get; private set; }

        public bool StringifyType => true;

        public bool StringifyRef => true;

        public bool FullyStringify { get; set; }

        public DObjectParameter(Type baseType, bool fullyStringify = false)
        {
            BaseType = baseType;
            FullyStringify = fullyStringify;
            FromType = typeof(DObject);
            attributes = new PropertyAttribute?[DObject.GetPropertyCount(baseType)];
        }
    }

    public class  DataRowParameter : DObjectParameter
    {
        public bool SerializeAsId { get; set; }
        public DataRowParameter(Type baseType, bool fullyStringify = false, bool SerializeAsId=false):base(baseType,fullyStringify)
        {
            this.SerializeAsId = SerializeAsId;
        }

    }
    public class  FactureParameter : DataRowParameter
    {
        private readonly User Requester;
        private readonly bool? _isFrozen;
        public override bool? IsFrozen(object p)
        {
            if (_isFrozen.HasValue) return _isFrozen.Value;
            var f = p as Facture;
            if (f == null) return DIsFrozen;
            var lb = f.LockedBy;
            if (lb != null)
                if (lb != Requester.Client) return true;
                else return false;
            else
            {
                return true;
                //if (f.IsValidated) return true;
                //else return Requester.IsAdmin ? false : false;
            }
        }
        public FactureParameter(User requester, Type baseType, bool fullyStringify = false, bool SerializeAsId = false, bool? isFrozen=null)
            : base(baseType, fullyStringify)
        {
            this.Requester = requester;
            this._isFrozen = isFrozen;
        }

    }

    public class  ArticlesParameter : DataTableParameter
    {
        public override bool? IsFrozen(object p)
        {
            var f = p as Articles;
            if (f == null) return DIsFrozen;
            var fct = ((Facture)f.Owner);
            return fct != null && fct.IsValidated;
        }

        public ArticlesParameter(Type baseType)
            : base(baseType)
        {
            
        }
    }

    public class  ArticleParameter : DataRowParameter
    {
        public override bool? IsFrozen(object p)
        {
            var f = p as Article;
            if (f == null) return DIsFrozen;
            return f.Facture.IsValidated;
        }

        public ArticleParameter(Type baseType, bool fullyStringify = false, bool SerializeAsId = false)
            : base(baseType, fullyStringify)
        {
        }
    }
}