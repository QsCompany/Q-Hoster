using System;
using models;

namespace Server
{
    public class  DataTableParameter:DObjectParameter
    {
        public Type ForType => typeof(DataTable);
        public bool SerializeItemsAsId { get; set; }


        public bool StringifyType => true;

        public bool StringifyRef => true;

        public DataTableParameter(Type baseType):base(baseType)
        {

        }
    }
    public class  PriceParameter:DObjectParameter
    {
        public Type ForType => typeof( FakePrice);
        public Abonment Job { get; set; }

        public bool StringifyType => true;

        public bool StringifyRef => true;
        public bool IsAdmin { get; set; }
        public PriceParameter():base(typeof(FakePrice))
        {
            
        }
    }
    
}