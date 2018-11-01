using System;
using Json;
using Server;

namespace models
{
    [QServer.Core.HosteableObject(null, typeof(QDataSerializer))]
    public class  QData:DataRow
    {
        public static int DpProducts = Register<QData, Products>("Products");         public Products Products { get => get<Products>(DpProducts);
            set => set(DpProducts, value);
        }
        public static int DpFactures = Register<QData, Factures>("Factures");         public Factures Factures { get => get<Factures>(DpFactures);
            set => set(DpFactures, value);
        }

        public QData(Context c,JValue jv):base(c,jv)
        {
        }

        public QData()
        {

        }

        public override JValue Parse(JValue json)
        {
            throw new NotImplementedException();
        }
    }
}