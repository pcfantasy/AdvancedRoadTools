using AdvancedRoadTools.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedRoadTools.NewManager
{
    public class CustomNetManager
    {
        public static void NetManagerReleaseNodeImplementationPrefix(ushort node)
        {
            for (int i = 0; i <8; i++)
            {
                MainDataStore.segmentModifiedMinOffset[node * 8 + i] = 0f;
            }
        }
    }
}
