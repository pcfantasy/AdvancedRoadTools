using System;
using ICities;

namespace AdvancedRoadTools.Util
{
    public class SaveAndRestore : SerializableDataExtensionBase
    {
        private static ISerializableData _serializableData;
        public static void save_floats(ref int idex, float[] item, ref byte[] container)
        {
            int i; int j;
            byte[] temp_data;
            for (j = 0; j < item.Length; j++)
            {
                temp_data = BitConverter.GetBytes(item[j]);
                for (i = 0; i < temp_data.Length; i++)
                {
                    container[idex + i] = temp_data[i];
                }
                idex = idex + temp_data.Length;
            }
        }

        public static float[] load_floats(ref int idex, byte[] container, int length)
        {
            float[] tmp = new float[length];
            int i;
            if (idex < container.Length)
            {
                for (i = 0; i < length; i++)
                {
                    tmp[i] = BitConverter.ToSingle(container, idex);
                    idex = idex + 4;
                }
            }
            else
            {
                for (i = 0; i < length; i++)
                {
                    idex = idex + 4;
                }
                DebugLog.LogToFileOnly("load data is too short, please check" + container.Length.ToString());
            }
            return tmp;
        }


        public static void gather_saveData()
        {
            MainDataStore.Save();
        }

        public override void OnCreated(ISerializableData serializableData)
        {
            _serializableData = serializableData;
        }

        public override void OnReleased()
        {
        }

        public override void OnSaveData()
        {
#if DEBUG
            if (Loader.CurrentLoadMode == LoadMode.LoadGame || Loader.CurrentLoadMode == LoadMode.NewGame)
            {
                DebugLog.LogToFileOnly("startsave");
                MainDataStore.SaveData = new byte[4194304];
                gather_saveData();
                _serializableData.SaveData("AdvancedRoadTools MainDataStore", MainDataStore.SaveData);
            }
#endif
        }

        public override void OnLoadData()
        {
#if DEBUG
            Loader.DataInit();
            DebugLog.LogToFileOnly("OnLoadData");
            if (true)
            {
                DebugLog.LogToFileOnly("startload");

                MainDataStore.SaveData = _serializableData.LoadData("AdvancedRoadTools MainDataStore");
                if (MainDataStore.SaveData == null)
                {
                    DebugLog.LogToFileOnly("no MainDataStore save data, please check");
                }
                else
                {
                    MainDataStore.Load();
                }
            }
#endif

        }
    }
}
