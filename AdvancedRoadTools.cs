using ICities;
using System.IO;
using ColossalFramework.UI;
using ColossalFramework;
using System;
using AdvancedRoadTools.Util;
using AdvancedRoadTools.UI;
using AdvancedRoadTools.NewData;
using System.Reflection;
using System.Collections.Generic;

namespace AdvancedRoadTools
{
    public class AdvancedRoadTools : IUserMod
    {
        public static bool IsEnabled = false;
        public string Name
        {
            get { return "AdvancedRoadTools"; }
        }
        public string Description
        {
            get { return "Can build more complex curve"; }
        }
        public void OnEnabled()
        {
            IsEnabled = true;
            FileStream fs = File.Create("AdvancedRoadTools.txt");
            fs.Close();
        }
        public void OnDisabled()
        {
            IsEnabled = false;
        }
        public AdvancedRoadTools()
        {
            try
            {
                if (GameSettings.FindSettingsFileByName("AdvancedRoadTools_SETTING") == null)
                {
                    // Creating setting file 
                    GameSettings.AddSettingsFile(new SettingsFile { fileName = "AdvancedRoadTools_SETTING" });
                }
            }
            catch (Exception)
            {
                DebugLog.LogToFileOnly("Could not load/create the setting file.");
            }
        }
        public void OnSettingsUI(UIHelperBase helper)
        {
            OptionUI.makeSettings(helper);
        }
    }
}
