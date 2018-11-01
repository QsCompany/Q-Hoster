using Json;
using Server;

namespace models
{
    [QServer.Core.HosteableObject(typeof(Api.Picture), typeof(PictureSerializer))]
    public class  Picture : DataRow
    {
        //public new static int __LOAD__(int dp) => DpRegion;

        //public static int DpImageUrl = Register<Picture, string>("ImageUrl", PropertyAttribute.None, null, null, "nvarchar(75)");
        //public string ImageUrl { get => get<string>(DpImageUrl);
        //    set => set(DpImageUrl, value);
        //}
        //public static int DpRegion = Register<Picture, Rectangle>("Region");         public Rectangle Region { get => get<Rectangle>(DpRegion);
        //    set => set(DpRegion, value);
        //}

        //public Picture()
        //{
            
        //}
      
        //public Picture(Context c,JValue jv):base(c,jv)
        //{
         
        //}
        
        //public override JValue Parse(JValue json)
        //{
        //    return json;
        //}       
    }
}