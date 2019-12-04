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
        public static bool DetourInited = false;
        public static UIView parentGuiView;
        public static LoadMode CurrentLoadMode;
        public static bool isGuiRunning = false;
        public static ThreeRoundButton threeRoundButton;
        public static OneRoundButton oneRoundButton;
        public static YRoadButton yRoadButton;
        public static SmoothButton smoothButton;
        public static string m_atlasName = "AdvancedRoadTools";
        public static bool m_atlasLoaded;

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
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
                    DebugLog.LogToFileOnly("OnLevelLoaded");
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
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(130, 2), new Vector2(30, 30)), "Smooth", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(162, 2), new Vector2(30, 30)), "Smooth_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(194, 2), new Vector2(30, 30)), "YRoad", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(226, 2), new Vector2(30, 30)), "YRoad_S", m_atlasName);
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
                SetupSmoothButton();
                isGuiRunning = true;
            }
        }

        public static void SetupThreeRoundButton()
        {
            if (threeRoundButton == null)
            {
                threeRoundButton = (parentGuiView.AddUIComponent(typeof(ThreeRoundButton)) as ThreeRoundButton);
            }
        }

        public static void SetupOneRoundButton()
        {
            if (oneRoundButton == null)
            {
                oneRoundButton = (parentGuiView.AddUIComponent(typeof(OneRoundButton)) as OneRoundButton);
            }
        }

        public static void SetupYRoadButton()
        {
            if (yRoadButton == null)
            {
                yRoadButton = (parentGuiView.AddUIComponent(typeof(YRoadButton)) as YRoadButton);
            }
        }

        public static void SetupSmoothButton()
        {
            if (smoothButton == null)
            {
                smoothButton = (parentGuiView.AddUIComponent(typeof(SmoothButton)) as SmoothButton);
            }
        }

        public static void RemoveGui()
        {
            isGuiRunning = false;
            if (parentGuiView != null)
            {
                parentGuiView = null;
                UnityEngine.Object.Destroy(threeRoundButton);
                UnityEngine.Object.Destroy(oneRoundButton);
                UnityEngine.Object.Destroy(yRoadButton);
                UnityEngine.Object.Destroy(smoothButton);
                threeRoundButton = null;
                oneRoundButton = null;
                yRoadButton = null;
                smoothButton = null;
            }
        }
    }
}
