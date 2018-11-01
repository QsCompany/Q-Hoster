using System.Runtime.InteropServices;

namespace Server
{
    sealed public class LimitedResource
    {
        // Create a HandleCollector telling it that collections should
        // occur when two or more of these objects exist in the heap
        public static readonly HandleCollector s_hc = new HandleCollector("LimitedResource", 2);
        public LimitedResource()
        {
            // Tell the HandleCollector a LimitedResource has been added to the heap
            s_hc.Add();
            MyConsole.WriteLine("LimitedResource create. Count={0}", s_hc.Count);
        }
        ~LimitedResource()
        {
            // Tell the HandleCollector a LimitedResource has been removed from the heap
            s_hc.Remove();
            MyConsole.WriteLine("LimitedResource destroy. Count={0}", s_hc.Count);
        }
    }
}        