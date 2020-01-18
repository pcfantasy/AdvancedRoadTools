using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedRoadTools.Util
{
    public class MainDataStore
    {
        public static float[] segmentModifiedMinOffset = new float[262144];
        public static byte[] SaveData = new byte[1048576];
        public static void Save()
        {
            int i = 0;
            SaveAndRestore.save_floats(ref i, segmentModifiedMinOffset, ref SaveData);
        }

        public static void Load()
        {
            int i = 0;
            segmentModifiedMinOffset = SaveAndRestore.load_floats(ref i, SaveData, segmentModifiedMinOffset.Length);
        }
    }
}
