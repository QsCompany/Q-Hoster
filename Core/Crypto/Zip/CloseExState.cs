using System;

namespace Server.Zip
{
    [Flags]
    public enum CloseExState
    {
        Normal = 0,
        Abort = 1,
        Silent = 2
    }
}