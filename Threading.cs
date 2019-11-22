using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedRoadTools
{
    public class Threading : ThreadingExtensionBase
    {
        public static bool isFirstTime = true;
        public override void OnAfterSimulationFrame()
        {
            base.OnAfterSimulationFrame();
            if (Loader.CurrentLoadMode == LoadMode.LoadGame || Loader.CurrentLoadMode == LoadMode.NewGame)
            {
                if (AdvancedRoadTools.IsEnabled)
                {
                    if (isFirstTime)
                    {
                        //Loader.SetupGui();
                        isFirstTime = false;
                    }
                }
            }
        }
    }
}
