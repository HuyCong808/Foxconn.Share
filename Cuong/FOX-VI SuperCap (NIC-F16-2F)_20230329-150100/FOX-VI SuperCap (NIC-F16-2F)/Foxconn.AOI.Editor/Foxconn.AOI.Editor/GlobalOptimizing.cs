using System;

namespace Foxconn.AOI.Editor
{
    public static class GlobalOptimizing
    {
        public static void FreeMemory()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
        }
    }
}
