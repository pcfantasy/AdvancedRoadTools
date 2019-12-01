using AdvancedRoadTools.NewData;
using AdvancedRoadTools.UI;
using AdvancedRoadTools.Util;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AdvancedRoadTools
{
    public class Loader : LoadingExtensionBase
    {
        /*public class Detour
        {
            public MethodInfo OriginalMethod;
            public MethodInfo CustomMethod;
            public RedirectCallsState Redirect;

            public Detour(MethodInfo originalMethod, MethodInfo customMethod)
            {
                this.OriginalMethod = originalMethod;
                this.CustomMethod = customMethod;
                this.Redirect = RedirectionHelper.RedirectCalls(originalMethod, customMethod);
            }
        }*/

        //public static List<Detour> Detours { get; set; }
        public static bool DetourInited = false;
        public static UIView parentGuiView;
        public static LoadMode CurrentLoadMode;
        public static bool isGuiRunning = false;
        public static ThreeRoundButton threeRoundButton;
        public static OneRoundButton oneRoundButton;
        public static YRoadButton yRoadButton;
        public static string m_atlasName = "AdvancedRoadTools";
        public static bool m_atlasLoaded;
        public static bool Done { get; private set; } // Only one Assets installation throughout the application

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            //Detours = new List<Detour>();
        }
        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            CurrentLoadMode = mode;
            if (AdvancedRoadTools.IsEnabled)
            {
                if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame || mode == LoadMode.NewMap || mode == LoadMode.LoadMap || mode == LoadMode.NewAsset || mode == LoadMode.LoadAsset)
                {
                    OptionUI.LoadSetting();
                    SetupGui();
                    SetupTools();
                    //InitDetour();
                    DebugLog.LogToFileOnly("OnLevelLoaded");
                    //if (mode == LoadMode.NewGame)
                    //{
                        //InitData();
                        //DebugLog.LogToFileOnly("InitData");
                    //}
                }
            }
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            if (CurrentLoadMode == LoadMode.LoadGame || CurrentLoadMode == LoadMode.NewGame || CurrentLoadMode == LoadMode.LoadMap || CurrentLoadMode == LoadMode.NewMap || CurrentLoadMode == LoadMode.LoadAsset || CurrentLoadMode == LoadMode.NewAsset)
            {
                if (AdvancedRoadTools.IsEnabled && isGuiRunning)
                {
                    Threading.isFirstTime = true;
                    RemoveGui();
                }
            }
        }

        private static void LoadSprites()
        {
            if (SpriteUtilities.GetAtlas(m_atlasName) != null) return;
            var modPath = PluginManager.instance.FindPluginInfo(Assembly.GetExecutingAssembly()).modPath;
            m_atlasLoaded = SpriteUtilities.InitialiseAtlas(Path.Combine(modPath, "Resources/Icon.png"), m_atlasName);
            if (m_atlasLoaded)
            {
                var spriteSuccess = true;
                spriteSuccess = SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(2, 2), new Vector2(30, 30)), "3Round", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(34, 2), new Vector2(30, 30)), "3Round_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(66, 2), new Vector2(30, 30)), "1Round", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(98, 2), new Vector2(30, 30)), "1Round_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(130, 2), new Vector2(30, 30)), "YRoad", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(162, 2), new Vector2(30, 30)), "YRoad_S", m_atlasName);
                if (!spriteSuccess) DebugLog.LogToFileOnly("Error: Some sprites haven't been loaded. This is abnormal; you should probably report this to the mod creator.");
            }
            else DebugLog.LogToFileOnly("Error: The texture atlas (provides custom icons) has not loaded. All icons have reverted to text prompts.");
        }

        public static void SetupTools()
        {
            if (AdvancedTools.instance == null)
            {
                ToolController toolController = GameObject.FindObjectOfType<ToolController>();
                AdvancedTools.instance = toolController.gameObject.AddComponent<AdvancedTools>();
                AdvancedTools.instance.enabled = false;
            }
            AdvancedTools.InitData();
        }
        public static void SetupGui()
        {
            LoadSprites();
            if (m_atlasLoaded)
            {
                parentGuiView = null;
                parentGuiView = UIView.GetAView();
                SetupThreeRoundButton();
                SetupOneRoundButton();
                SetupYRoadButton();
                isGuiRunning = true;
            }
        }

        public static void SetupThreeRoundButton()
        {
            if (threeRoundButton == null)
            {
                threeRoundButton = (parentGuiView.AddUIComponent(typeof(ThreeRoundButton)) as ThreeRoundButton);
            }
            //mainButton.Show();
        }

        public static void SetupOneRoundButton()
        {
            if (oneRoundButton == null)
            {
                oneRoundButton = (parentGuiView.AddUIComponent(typeof(OneRoundButton)) as OneRoundButton);
            }
            //mainButton.Show();
        }

        public static void SetupYRoadButton()
        {
            if (yRoadButton == null)
            {
                yRoadButton = (parentGuiView.AddUIComponent(typeof(YRoadButton)) as YRoadButton);
            }
            //mainButton.Show();
        }

        /*public static void SetupButton()
        {
            RoadToolsWindowGameObject = new GameObject("RoadToolsWindowGameObject");
            RoadToolsWindowGameObject1 = new GameObject("RoadToolsWindowGameObject1");
            threeRoundButton = (ThreeRoundButton)RoadToolsWindowGameObject.AddComponent(typeof(ThreeRoundButton));
            oneRoundButton = (OneRoundButton)RoadToolsWindowGameObject1.AddComponent(typeof(OneRoundButton));
            RoadToolsInfo = Find<UIPanel>("RoadsOptionPanel(RoadsPanel)");
            if (RoadToolsInfo == null)
            {
                DebugLog.LogToFileOnly("UIPanel not found (update broke the mod!): RoadsOptionPanel(RoadsPanel)\nAvailable panels are:\n");
                UITabstrip uITabstrip = Find<UITabstrip>("MainToolstrip");
                if (uITabstrip == null)
                {
                    DebugLog.LogToFileOnly("Could not find MainToolstrip");
                    return;
                }
                DebugLog.LogToFileOnly("eventSelectedIndexChanged Init");
                uITabstrip.eventSelectedIndexChanged += mainToolStrip_eventSelectedIndexChanged;
            }
            else
            {
                threeRoundButton.transform.parent = RoadToolsInfo.transform;
                threeRoundButton.size = new Vector3(30, 30);
                threeRoundButton.baseBuildingWindow = RoadToolsInfo.gameObject.transform.GetComponentInChildren<RoadsPanel>();
                threeRoundButton.position = new Vector3(-40, RoadToolsInfo.size.y);

                oneRoundButton.transform.parent = RoadToolsInfo.transform;
                oneRoundButton.size = new Vector3(30, 30);
                oneRoundButton.baseBuildingWindow = RoadToolsInfo.gameObject.transform.GetComponentInChildren<RoadsPanel>();
                oneRoundButton.position = new Vector3(-75, RoadToolsInfo.size.y);
                RoadToolsInfo.eventVisibilityChanged += RoadToolsInfo_eventVisibilityChanged;
            }
        }

        public static T Find<T>(string name) where T : Object
        {
            T[] array = Object.FindObjectsOfType<T>();
            foreach (T val in array)
            {
                if (val.name.Equals(name))
                {
                    return val;
                }
            }
            return null;
        }
        private static void mainToolStrip_eventSelectedIndexChanged(UIComponent component, int value)
        {
            DebugLog.LogToFileOnly("eventSelectedIndexChanged");
            if (value == 0)
            {
                DebugLog.LogToFileOnly("eventSelectedIndexChanged right tab selected");
                component.StartCoroutine(GetRoadOptionsPanel((UITabstrip)component));
            }
        }

        private static IEnumerator<object> GetRoadOptionsPanel(UITabstrip tabstrip)
        {
            yield return null;
            if (Loader.RoadToolsInfo == null)
            {
                RoadToolsInfo = getRoadsOptionsPanel();
                if (RoadToolsInfo != null)
                {
                    tabstrip.eventSelectedIndexChanged -= mainToolStrip_eventSelectedIndexChanged;
                    onWaitComplete();
                }
            }
            else
            {
                tabstrip.eventSelectedIndexChanged -= mainToolStrip_eventSelectedIndexChanged;
            }
        }
        private static UIPanel getRoadsOptionsPanel()
        {
            return Find<UIPanel>("RoadsOptionPanel(RoadsPanel)");
        }

        private static void onWaitComplete()
        {
            threeRoundButton.transform.parent = RoadToolsInfo.transform;
            threeRoundButton.size = new Vector3(30, 30);
            threeRoundButton.baseBuildingWindow = RoadToolsInfo.gameObject.transform.GetComponentInChildren<RoadsPanel>();
            threeRoundButton.position = new Vector3(-40, RoadToolsInfo.size.y);

            oneRoundButton.transform.parent = RoadToolsInfo.transform;
            oneRoundButton.size = new Vector3(30, 30);
            oneRoundButton.baseBuildingWindow = RoadToolsInfo.gameObject.transform.GetComponentInChildren<RoadsPanel>();
            oneRoundButton.position = new Vector3(-75, RoadToolsInfo.size.y);
            RoadToolsInfo.eventVisibilityChanged += RoadToolsInfo_eventVisibilityChanged;
        }
        public static void RoadToolsInfo_eventVisibilityChanged(UIComponent component, bool value)
        {
            threeRoundButton.isEnabled = value;
            oneRoundButton.isEnabled = value;
            if (value)
            {
                //initialize again
                threeRoundButton.transform.parent = RoadToolsInfo.transform;
                threeRoundButton.size = new Vector3(30, 30);
                threeRoundButton.baseBuildingWindow = RoadToolsInfo.gameObject.transform.GetComponentInChildren<RoadsPanel>();
                threeRoundButton.position = new Vector3(-40, RoadToolsInfo.size.y);
                threeRoundButton.Show();
                oneRoundButton.transform.parent = RoadToolsInfo.transform;
                oneRoundButton.size = new Vector3(30, 30);
                oneRoundButton.baseBuildingWindow = RoadToolsInfo.gameObject.transform.GetComponentInChildren<RoadsPanel>();
                oneRoundButton.position = new Vector3(-75, RoadToolsInfo.size.y);
                oneRoundButton.Show();
            }
            else
            {
                threeRoundButton.Hide();
                oneRoundButton.Hide();
            }
        }*/

        public static void RemoveGui()
        {
            isGuiRunning = false;
            if (parentGuiView != null)
            {
                parentGuiView = null;
                UnityEngine.Object.Destroy(threeRoundButton);
                UnityEngine.Object.Destroy(oneRoundButton);
                UnityEngine.Object.Destroy(yRoadButton);
                threeRoundButton = null;
                oneRoundButton = null;
                yRoadButton = null;
            }
        }
    }
}
