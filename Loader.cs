using AdvancedRoadTools.NewData;
using AdvancedRoadTools.Tools;
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
using UnityEngine;
using Object = UnityEngine.Object;

namespace AdvancedRoadTools
{
    public class Loader : LoadingExtensionBase
    {
        //public static bool DetourInited = false;
        public static UIView parentGuiView;
        public static LoadMode CurrentLoadMode;
        public static bool isGuiRunning = false;
        public static ThreeRoundButton threeRoundButton;
        public static OneRoundButton oneRoundButton;
        public static YRoadButton yRoadButton;
        public static SmoothButton smoothButton;
        public static FixButton fixButton;
        public static string m_atlasName = "AdvancedRoadTools";
        public static bool m_atlasLoaded;
        public static bool HarmonyDetourInited = false;
        public static bool HarmonyDetourFailed = true;
        public class Detour
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
        }

        public static List<Detour> Detours { get; set; }
        public static bool DetourInited = false;

        public override void OnCreated(ILoading loading)
        {
            Detours = new List<Detour>();
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
                    HarmonyInitDetour();
                    InitDetour();
                    DebugLog.LogToFileOnly("OnLevelLoaded");
                }
            }
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            if (CurrentLoadMode == LoadMode.LoadGame || CurrentLoadMode == LoadMode.NewGame || CurrentLoadMode == LoadMode.LoadMap || CurrentLoadMode == LoadMode.NewMap || CurrentLoadMode == LoadMode.LoadAsset || CurrentLoadMode == LoadMode.NewAsset)
            {
                if (AdvancedRoadTools.IsEnabled)
                {
                    HarmonyRevertDetour();
                    RevertDetour();
                    Threading.isFirstTime = true;
                    if (isGuiRunning)
                    {
                        RemoveGui();
                    }
                }
            }
        }

        public static void DataInit()
        {
            for (int i = 0; i < 262144; i++)
            {
                MainDataStore.segmentModifiedMinOffset[i] = 0f;
            }
            MainDataStore.SaveData = new byte[1048576];
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
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(194, 2), new Vector2(30, 30)), "Fix", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(226, 2), new Vector2(30, 30)), "Fix_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(258, 2), new Vector2(30, 30)), "YRoad", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(290, 2), new Vector2(30, 30)), "YRoad_S", m_atlasName);
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

        public void HarmonyInitDetour()
        {
            if (!HarmonyDetourInited)
            {
                DebugLog.LogToFileOnly("Init harmony detours");
                HarmonyDetours.Apply();
                HarmonyDetourInited = true;
            }
        }

        public void HarmonyRevertDetour()
        {
            if (HarmonyDetourInited)
            {
                DebugLog.LogToFileOnly("Revert harmony detours");
                HarmonyDetours.DeApply();
                HarmonyDetourInited = false;
                HarmonyDetourFailed = true;
            }
        }

        public void InitDetour()
        {
            if (!DetourInited)
            {
                DebugLog.LogToFileOnly("Init detours");
                bool detourFailed = false;

                //1
                //private void RefreshJunctionData(ushort nodeID, NetInfo info, uint instanceIndex)
                /*DebugLog.LogToFileOnly("Detour NetNode::RefreshJunctionData calls");
                try
                {
                    Detours.Add(new Detour(typeof(NetNode).GetMethod("RefreshJunctionData", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(NetInfo), typeof(uint) }, null),
                                           typeof(CustomNetNode).GetMethod("RefreshJunctionData", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(NetNode).MakeByRefType(), typeof(ushort), typeof(NetInfo), typeof(uint) }, null)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour NetNode::RefreshJunctionData");
                    //detourFailed = true;
                }*/
                //2
                //public static void CalculateCorner(ref NetSegment segment, ushort segmentID, bool heightOffset, bool start, bool leftSide, out Vector3 cornerPos, out Vector3 cornerDirection, out bool smooth)
                DebugLog.LogToFileOnly("Detour NetSegment::CalculateCorner calls");
                try
                {
                    Detours.Add(new Detour(typeof(NetSegment).GetMethod("CalculateCorner", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(bool), typeof(bool), typeof(bool), typeof(Vector3).MakeByRefType(), typeof(Vector3).MakeByRefType(), typeof(bool).MakeByRefType() }, null),
                                           typeof(CustomNetSegment).GetMethod("CalculateCorner", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(NetSegment).MakeByRefType(), typeof(ushort), typeof(bool), typeof(bool), typeof(bool), typeof(Vector3).MakeByRefType(), typeof(Vector3).MakeByRefType(), typeof(bool).MakeByRefType() }, null)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour NetSegment::CalculateCorner");
                    //detourFailed = true;
                }

                if (detourFailed)
                {
                    DebugLog.LogToFileOnly("Detours failed");
                }
                else
                {
                    DebugLog.LogToFileOnly("Detours successful");
                }
                DetourInited = true;
            }
        }

        public void RevertDetour()
        {
            if (DetourInited)
            {
                DebugLog.LogToFileOnly("Revert detours");
                Detours.Reverse();
                foreach (Detour d in Detours)
                {
                    RedirectionHelper.RevertRedirect(d.OriginalMethod, d.Redirect);
                }
                DetourInited = false;
                Detours.Clear();
                DebugLog.LogToFileOnly("Reverting detours finished.");
            }
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
                SetupFixButton();
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

        public static void SetupFixButton()
        {
            if (fixButton == null)
            {
                fixButton = (parentGuiView.AddUIComponent(typeof(FixButton)) as FixButton);
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
                UnityEngine.Object.Destroy(fixButton);
                threeRoundButton = null;
                oneRoundButton = null;
                yRoadButton = null;
                smoothButton = null;
                fixButton = null;
            }
        }
    }
}
