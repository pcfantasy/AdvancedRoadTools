using ColossalFramework;
using ColossalFramework.UI;
using ColossalFramework.Plugins;
using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using AdvancedRoadTools.Util;
using ColossalFramework.Math;
using AdvancedRoadTools.NewData;
using Object = UnityEngine.Object;
using AdvancedRoadTools.UI;

namespace AdvancedRoadTools.Tools
{
    public class AdvancedTools : ToolBase
    {
        //private static MethodInfo RenderSegment = typeof(NetTool).GetMethod("RenderSegment", BindingFlags.NonPublic | BindingFlags.Static);
        const float RAD = Mathf.PI / 180;
        public static AdvancedTools instance;
        public static ushort m_hover;
        public static ushort m_hoverSegment;
        public static int preFixSegmentIndex;
        public static ushort m_step;
        private static InfoManager.InfoMode m_prevInfoMode;
        Vector3 pos0;
        Vector3 pos1;
        Vector3 pos2;
        Vector3 pos;
        ushort node0;
        ushort node1;
        ushort node2;
        ushort segment1;
        public static float radius;
        public static byte rampMode;
        public static float height;
        public static int currentMoney;

        public static byte leftAddWidth;
        public static byte rightAddWidth;
        public static byte mainRoadWidth;
        public static byte roadSpace;
        public static byte roadLength;

        //load&restore
        public static Vector3[] storedPos0 = new Vector3[8];
        public static Vector3[] storedPos1 = new Vector3[8];
        public static Vector3[] storedPos2 = new Vector3[8];
        public static float[] storedElevation = new float[8];
        public static float[] storedRadius = new float[8];
        public static byte[] storedRampMode = new byte[8];
        public static ushort[] storedNode0 = new ushort[8];
        public static ushort[] storedNode2 = new ushort[8];
        public static NetInfo[] storedNetInfo = new NetInfo[8];
        public static byte storedNum;
        public static byte hoveredRoundIndex;
        public static bool updateRoundMode;

        public static NetInfo m_netInfo;

        Color hcolor = new Color32(0, 181, 255, 255);
        Color scolor = new Color32(95, 166, 0, 244);
        Color m_errorColorInfo = new Color(1f, 0.25f, 0.1875f, 0.75f);
        Color m_validColorInfo = new Color(0f, 0f, 0f, 0.5f);

        public NetManager Manager
        {
            get { return Singleton<NetManager>.instance; }
        }
        public NetNode GetNode(ushort id)
        {
            return Manager.m_nodes.m_buffer[id];
        }
        public NetSegment GetSegment(ushort id)
        {
            return Manager.m_segments.m_buffer[id];
        }

        protected override void Awake()
        {
            base.Awake();
            InitData();
        }

        public static void InitData()
        {
            radius = 20;
            rampMode = 0;
            height = 0;
            m_step = 0;
            storedPos0 = new Vector3[8];
            storedPos1 = new Vector3[8];
            storedPos2 = new Vector3[8];
            storedElevation = new float[8];
            storedRadius = new float[8];
            storedRampMode = new byte[8];
            storedNode0 = new ushort[8];
            storedNode2 = new ushort[8];
            storedNetInfo = new NetInfo[8];
            storedNum = 255;
            hoveredRoundIndex = 255;
            updateRoundMode = false;
            currentMoney = 0;
            leftAddWidth = 0;
            rightAddWidth = 0;
            mainRoadWidth = 16;
            roadSpace = 0;
            roadLength = 24;
        }
        protected override void OnToolUpdate()
        {
            base.OnToolUpdate();
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastInput input = new RaycastInput(ray, Camera.main.farClipPlane);
            input.m_ignoreTerrain = false;
            input.m_ignoreNodeFlags = NetNode.Flags.None;
            RayCast(input, out RaycastOutput output);
            pos = output.m_hitPos;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                enabled = false;
                ToolsModifierControl.SetTool<DefaultTool>();
                m_step = 0;
            }
            //revert
            if (Input.GetMouseButtonUp(1))
            {
                if (m_step != 0)
                {
                    m_step--;
                }
            }
            //hoveredRound
            if (m_step == 0)
            {
                if (updateRoundMode)
                {
                    storedPos1[hoveredRoundIndex] = output.m_hitPos;
                    storedElevation[hoveredRoundIndex] = height;
                    storedRadius[hoveredRoundIndex] = radius;

                    if (hoveredRoundIndex != 255)
                    {
                        if (storedRampMode[hoveredRoundIndex] == 0)
                        {
                            if (Input.GetMouseButtonUp(0))
                            {
                                storedPos1[hoveredRoundIndex] = output.m_hitPos;
                                storedElevation[hoveredRoundIndex] = height;
                                storedRadius[hoveredRoundIndex] = radius;
                                updateRoundMode = updateRoundMode ? false : true;
                            }
                            CustomShowToolInfo(true, Localization.Get("ChangeRoundCenter") + "\n" + Localization.Get("Radius") + radius.ToString() + "\n" + Localization.Get("Height") + height.ToString() + "\n" + Localization.Get("ConstructionFee") + (currentMoney / 100f).ToString(), output.m_hitPos);
                        }
                    }
                }
                else
                {
                    hoveredRoundIndex = 255;
                    if (storedNum != 255)
                    {
                        for (int i = 0; i <= storedNum; i++)
                        {
                            if (storedRampMode[i] == 0)
                            {
                                if (Vector2.Distance(VectorUtils.XZ(output.m_hitPos), VectorUtils.XZ(storedPos1[i])) < 4f)
                                {
                                    hoveredRoundIndex = (byte)i;
                                }
                            }
                        }
                    }
                    if (hoveredRoundIndex != 255)
                    {
                        if (storedRampMode[hoveredRoundIndex] == 0)
                        {
                            if (Input.GetMouseButtonUp(0))
                            {
                                storedPos1[hoveredRoundIndex] = output.m_hitPos;
                                storedElevation[hoveredRoundIndex] = height;
                                storedRadius[hoveredRoundIndex] = radius;
                                updateRoundMode = updateRoundMode ? false : true;
                            }
                            CustomShowToolInfo(true, Localization.Get("ChangeRoundCenter") + "\n" + Localization.Get("Radius") + radius.ToString() + "\n" + Localization.Get("Height") + height.ToString() + "\n" + Localization.Get("ConstructionFee") + (currentMoney / 100f).ToString(), output.m_hitPos);
                        }
                    }
                }
            }

            if (hoveredRoundIndex == 255)
            {
                //Step 0 & 1
                if (m_step != 2)
                {
                    switch (m_step)
                    {
                        case 0:
                            determineHoveredElements(true); 
                            pos0 = GetNode(m_hover).m_position; 
                            node0 = m_hover;
                            CustomShowToolInfo(true, Localization.Get("STEP0"), output.m_hitPos);
                            if (rampMode == 1)
                            {
                                if (CheckYRoadVaild(node0) == "False")
                                {
                                    CustomShowExtraInfo(true, Localization.Get("STEP0ERROR"), pos);
                                }
                                else
                                {
                                    CustomShowExtraInfo(false, null, Vector3.zero);
                                }
                            }
                            break;
                        case 1:
                            if (rampMode == 0)
                            {
                                pos1 = output.m_hitPos;
                                CustomShowToolInfo(true, Localization.Get("3ROUNDSTEP1") + radius.ToString() , output.m_hitPos);
                            }
                            else if (rampMode == 1)
                            {
                                if (CheckYRoadVaild(node0) == "Dual")
                                {
                                    rightAddWidth = leftAddWidth;
                                    leftAddWidth = rightAddWidth;
                                }
                                CustomShowToolInfo(true, Localization.Get("YROADSTEP1") + leftAddWidth.ToString() + "\n" + Localization.Get("RightAddedWidth") + rightAddWidth.ToString() + "\n" + Localization.Get("CurrentRoadWidth") + ((int)(Manager.m_nodes.m_buffer[node0].Info.m_halfWidth * 2)).ToString(), output.m_hitPos);
                            }
                            else if (rampMode == 2)
                            {
                                determineHoveredElements(true);
                                pos1 = GetNode(m_hover).m_position;
                                node1 = m_hover;
                                CustomShowToolInfo(true, Localization.Get("1ROUNDSTEP1") + "\n" + Localization.Get("Radius") + radius.ToString(), output.m_hitPos);
                            }
                            else if (rampMode == 3)
                            {
                                determineHoveredElements(false);
                                segment1 = m_hoverSegment;
                                for (int i = 0; i < 8; i++)
                                {
                                    if (Manager.m_nodes.m_buffer[node0].GetSegment(i) != 0 && Manager.m_nodes.m_buffer[node0].GetSegment(i) == segment1)
                                    {
                                        preFixSegmentIndex = i;
                                    }
                                }
                            }
                            break;
                        default: break;
                    }
                    //Step 0 & 1 click mouse
                    if (Input.GetMouseButtonUp(0))
                    {
                        if (m_step < 2)
                        {
                            if (m_step == 0)
                            {
                                if (m_hover != 0)
                                {
                                    m_step++;
                                }
                            }
                            else
                            {
                                if (m_hover != 0 || rampMode == 1 || ((rampMode == 3) && (m_hoverSegment!=0)))
                                {
                                    m_step++;
                                }
                            }
                        }
                    }
                }
                else
                {
                    //Step 2, 1Round and 3Round Tools need to select node.
                    if (rampMode != 1 && rampMode != 3)
                    determineHoveredElements(true);

                    if (m_hover != 0 || rampMode == 1)
                    {
                        if (rampMode != 1)
                        {
                            pos2 = GetNode(m_hover).m_position;
                            node2 = m_hover;
                        }

                        if (Input.GetMouseButtonUp(0))
                        {
                            FieldInfo cashAmount;
                            cashAmount = typeof(EconomyManager).GetField("m_cashAmount", BindingFlags.NonPublic | BindingFlags.Instance);
                            long _cashAmount = (long)cashAmount.GetValue(Singleton<EconomyManager>.instance);

                            if ((_cashAmount < currentMoney) && OptionUI.isMoneyNeeded && ((Loader.CurrentLoadMode == ICities.LoadMode.LoadGame) || (Loader.CurrentLoadMode == ICities.LoadMode.NewGame)))
                            {
                                //CustomShowExtraInfo(true, Localization.Get("NOMONEY"), pos);
                            }
                            else
                            {
                                if (OptionUI.isMoneyNeeded && ((Loader.CurrentLoadMode == ICities.LoadMode.LoadGame) || (Loader.CurrentLoadMode == ICities.LoadMode.NewGame)))
                                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Construction, currentMoney, m_netInfo.m_class);

                                bool noNeedUpdate = false;
                                if (rampMode == 0)
                                {
                                    Build3RoundRoad(false, false, false, false, 0, 0, null, out noNeedUpdate);
                                }
                                else if (rampMode == 1)
                                {
                                    BuildYRoad(false, false);
                                }
                                else if (rampMode == 2)
                                {
                                    Build1RoundRoad(false, false, false, false, 0, 0, null, out noNeedUpdate);
                                }
                                if (!noNeedUpdate)
                                {
                                    storedNum = 255;
                                    m_step = 0;
                                    CustomShowToolInfo(show: false, null, Vector3.zero);
                                    CustomShowExtraInfo(show: false, null, Vector3.zero);
                                    updateRoundMode = false;
                                    currentMoney = 0;
                                    preFixSegmentIndex = 0;
                                }
                            }
                        }
                        else
                        {
                            FieldInfo cashAmount;
                            cashAmount = typeof(EconomyManager).GetField("m_cashAmount", BindingFlags.NonPublic | BindingFlags.Instance);
                            long _cashAmount = (long)cashAmount.GetValue(Singleton<EconomyManager>.instance);

                            if ((_cashAmount < currentMoney) && OptionUI.isMoneyNeeded && ((Loader.CurrentLoadMode == ICities.LoadMode.LoadGame) || (Loader.CurrentLoadMode == ICities.LoadMode.NewGame)))
                            {
                                CustomShowExtraInfo(true, Localization.Get("NOMONEY") + "\n" + Localization.Get("TRYOPTION"), pos);
                            }
                        }
                    }
                    else
                    {
                        pos2 = Vector3.zero;
                        node2 = 0;
                    }

                    if (rampMode == 0 || rampMode == 2)
                    {
                        if (OptionUI.isMoneyNeeded && ((Loader.CurrentLoadMode == ICities.LoadMode.LoadGame) || (Loader.CurrentLoadMode == ICities.LoadMode.NewGame)))
                        {
                            if (!OptionUI.isSmoothMode)
                                CustomShowToolInfo(true, Localization.Get("3ROUNDSTEP2") + "\n" + Localization.Get("Radius") + radius.ToString() + "\n" + Localization.Get("Height") + height.ToString() + "\n" + Localization.Get("ConstructionFee") + (currentMoney / 100f).ToString(), output.m_hitPos);
                            else
                                CustomShowToolInfo(true, Localization.Get("3ROUNDSTEP2") + "\n" + Localization.Get("Radius") + radius.ToString() + "\n" + Localization.Get("Height") + Localization.Get("SMOOTHMODE") + "\n" + Localization.Get("ConstructionFee") + (currentMoney / 100f).ToString(), output.m_hitPos);
                        }
                        else
                        {
                            if (!OptionUI.isSmoothMode)
                                CustomShowToolInfo(true, Localization.Get("3ROUNDSTEP2") + "\n" + Localization.Get("Radius") + radius.ToString() + "\n" + Localization.Get("Height") + height.ToString(), output.m_hitPos);
                            else
                                CustomShowToolInfo(true, Localization.Get("3ROUNDSTEP2") + "\n" + Localization.Get("Radius") + radius.ToString() + "\n" + Localization.Get("Height") + Localization.Get("SMOOTHMODE"), output.m_hitPos);
                        }
                    }
                    else if (rampMode == 1)
                    {
                        if (OptionUI.isMoneyNeeded && ((Loader.CurrentLoadMode == ICities.LoadMode.LoadGame) || (Loader.CurrentLoadMode == ICities.LoadMode.NewGame)))
                        {
                            if (CheckYRoadVaild(node0) == "Dual")
                            {
                                float totalWidth = Singleton<NetManager>.instance.m_nodes.m_buffer[node0].Info.m_halfWidth * 2 + leftAddWidth + rightAddWidth + 2 * roadSpace;
                                CustomShowToolInfo(true, Localization.Get("YROADSTEP2Dual") + "\n" + Localization.Get("MainRoadWidth") + mainRoadWidth.ToString() + "\n" + Localization.Get("RightLeftRoadWidth") + ((totalWidth - mainRoadWidth - 2 * roadSpace) / 2f).ToString() + "\n" + Localization.Get("RoadGaps") + roadSpace.ToString() + "\n" + Localization.Get("ConstructionFee") + (currentMoney / 100f).ToString(), output.m_hitPos);
                            }
                            else
                            {
                                float totalWidth = Singleton<NetManager>.instance.m_nodes.m_buffer[node0].Info.m_halfWidth * 2 + leftAddWidth + rightAddWidth + roadSpace;
                                CustomShowToolInfo(true, Localization.Get("YROADSTEP2Single") + "\n" + Localization.Get("LeftRoadWidth") + mainRoadWidth.ToString() + "\n" + Localization.Get("RightRoadWidth") + (totalWidth - mainRoadWidth - roadSpace).ToString() + "\n" + Localization.Get("RoadGaps") + roadSpace.ToString() + "\n" + Localization.Get("ConstructionFee") + (currentMoney / 100f).ToString(), output.m_hitPos);
                            }
                        }
                        else
                        {
                            if (CheckYRoadVaild(node0) == "Dual")
                            {
                                float totalWidth = Singleton<NetManager>.instance.m_nodes.m_buffer[node0].Info.m_halfWidth * 2 + leftAddWidth + rightAddWidth + 2 * roadSpace;
                                CustomShowToolInfo(true, Localization.Get("YROADSTEP2Dual") + "\n" + Localization.Get("MainRoadWidth") + mainRoadWidth.ToString() + "\n" + Localization.Get("RightLeftRoadWidth") + ((totalWidth - mainRoadWidth - 2 * roadSpace) / 2f).ToString() + "\n" + Localization.Get("RoadGaps") + roadSpace.ToString() + "\n" + Localization.Get("RoadLength") + roadLength.ToString(), output.m_hitPos);
                            }
                            else
                            {
                                float totalWidth = Singleton<NetManager>.instance.m_nodes.m_buffer[node0].Info.m_halfWidth * 2 + leftAddWidth + rightAddWidth + roadSpace;
                                CustomShowToolInfo(true, Localization.Get("YROADSTEP2Single") + "\n" + Localization.Get("LeftRoadWidth") + mainRoadWidth.ToString() + "\n" + Localization.Get("RightRoadWidth") + (totalWidth - mainRoadWidth - roadSpace).ToString() + "\n" + Localization.Get("RoadGaps") + roadSpace.ToString() + "\n" + Localization.Get("RoadLength") + roadLength.ToString(), output.m_hitPos);
                            }
                        }
                    }
                }
            }
        }

        public float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        protected override void OnToolGUI(Event e)
        {
            if (enabled == true)
            {
                if (OptionsKeymappingRoadTool.m_add.IsPressed(e))
                {
                    if (rampMode == 0 || rampMode == 2)
                        radius = Clamp(radius + 0.5f, 4, 250);
                    else if (m_step == 1)
                        leftAddWidth = (byte)COMath.Clamp(leftAddWidth + 2, 0, 32);
                    else if (m_step == 2)
                        mainRoadWidth = (byte)COMath.Clamp(mainRoadWidth + 2, 0, 32);
                }
                if (OptionsKeymappingRoadTool.m_minus.IsPressed(e))
                {
                    if (rampMode == 0 || rampMode == 2)
                        radius = Clamp(radius - 0.5f, 4, 250);
                    else if (m_step == 1)
                        leftAddWidth = (byte)COMath.Clamp(leftAddWidth - 2, 0, 32);
                    else if (m_step == 2)
                        mainRoadWidth = (byte)COMath.Clamp(mainRoadWidth - 2, 0, 32);
                }
                if (OptionsKeymappingRoadTool.m_rise.IsPressed(e))
                {
                    if (rampMode == 0 || rampMode == 2)
                        height = COMath.Clamp((int)height + 1, -32, 32);
                    else if (m_step == 1)
                        rightAddWidth = (byte)COMath.Clamp(rightAddWidth + 2, 0, 32);
                    else if (m_step == 2)
                        roadSpace = (byte)COMath.Clamp(roadSpace + 1, 0, 4);

                    if (height < -8f)
                    {
                        if (InfoManager.instance.CurrentMode != InfoManager.InfoMode.Underground)
                        {
                            m_prevInfoMode = InfoManager.instance.CurrentMode;
                            InfoManager.instance.SetCurrentMode(InfoManager.InfoMode.Underground, InfoManager.instance.CurrentSubMode);
                        }
                    }
                    else
                    {
                        InfoManager.instance.SetCurrentMode(m_prevInfoMode, InfoManager.instance.CurrentSubMode);
                    }
                }
                if (OptionsKeymappingRoadTool.m_lower.IsPressed(e))
                {
                    if (rampMode == 0 || rampMode == 2)
                        height = (float)COMath.Clamp((int)height - 1, -32, 32);
                    else if (m_step == 1)
                        rightAddWidth = (byte)COMath.Clamp(rightAddWidth - 2, 0, 32);
                    else if (m_step == 2)
                        roadSpace = (byte)COMath.Clamp(roadSpace - 1, 0, 4);

                    if (height < -8f)
                    {
                        if (InfoManager.instance.CurrentMode != InfoManager.InfoMode.Underground)
                        {
                            m_prevInfoMode = InfoManager.instance.CurrentMode;
                            InfoManager.instance.SetCurrentMode(InfoManager.InfoMode.Underground, InfoManager.instance.CurrentSubMode);
                        }
                    }
                    else
                    {
                        InfoManager.instance.SetCurrentMode(m_prevInfoMode, InfoManager.instance.CurrentSubMode);
                    }
                }
                if (OptionsKeymappingRoadTool.m_laterBuild.IsPressed(e))
                {
                    if (!updateRoundMode && (rampMode != 1) && (rampMode != 3))
                    {
                        if ((storedNum < 8) || (storedNum == 255))
                        {
                            if (storedNum == 255) storedNum = 0;
                            else storedNum++;
                            //DebugLog.LogToFileOnly("storedNum = " + storedNum.ToString());
                            bool isUpdated = false;
                            if (rampMode == 0)
                            {
                                Build3RoundRoad(false, false, true, false, storedNum, storedNum, null, out isUpdated);
                            }
                            else if (rampMode == 2)
                            {
                                Build1RoundRoad(false, false, true, false, storedNum, storedNum, null, out isUpdated);
                            }
                            if (isUpdated)
                            {
                                storedNum--;
                            }
                        }
                        m_step = 0;
                        CustomShowToolInfo(show: false, null, Vector3.zero);
                        currentMoney = 0;
                    }
                    else
                    {
                        roadLength = (byte)COMath.Clamp(roadLength + 8, 8, 64);
                    }
                    //ToolsModifierControl.SetTool<DefaultTool>();
                    //enabled = false;
                }

                if (OptionsKeymappingRoadTool.m_build.IsPressed(e))
                {
                    if (!updateRoundMode && (rampMode != 1) && (rampMode != 3))
                    {
                        FieldInfo cashAmount;
                        cashAmount = typeof(EconomyManager).GetField("m_cashAmount", BindingFlags.NonPublic | BindingFlags.Instance);
                        long _cashAmount = (long)cashAmount.GetValue(Singleton<EconomyManager>.instance);

                        if ((_cashAmount < currentMoney) && OptionUI.isMoneyNeeded && ((Loader.CurrentLoadMode == ICities.LoadMode.LoadGame) || (Loader.CurrentLoadMode == ICities.LoadMode.NewGame)))
                        {
                            //CustomShowExtraInfo(true, Localization.Get("NOMONEY"), pos);
                        }
                        else
                        {
                            if (OptionUI.isMoneyNeeded && ((Loader.CurrentLoadMode == ICities.LoadMode.LoadGame) || (Loader.CurrentLoadMode == ICities.LoadMode.NewGame)))
                                Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Construction, currentMoney, m_netInfo.m_class);
                            if (storedNum != 255)
                            {
                                for (int i = 0; i <= storedNum; i++)
                                {
                                    if (storedRampMode[i] == 0)
                                    {
                                        Build3RoundRoad(false, false, false, true, (byte)i, (byte)i, null, out bool _);
                                    }
                                    else if (storedRampMode[i] == 2)
                                    {
                                        Build1RoundRoad(false, false, false, true, (byte)i, (byte)i, null, out bool _);
                                    }
                                }
                            }
                            storedNum = 255;
                            m_step = 0;
                            CustomShowToolInfo(show: false, null, Vector3.zero);
                            ToolsModifierControl.SetTool<DefaultTool>();
                            enabled = false;
                            updateRoundMode = false;
                            currentMoney = 0;
                        }
                    }
                    else
                    {
                        roadLength = (byte)COMath.Clamp(roadLength - 8, 8, 64);
                    }
                }

                if (OptionsKeymappingRoadTool.m_clear.IsPressed(e))
                {
                    storedNum = 255;
                    m_step = 0;
                    CustomShowToolInfo(show: false, null, Vector3.zero);
                    ToolsModifierControl.SetTool<DefaultTool>();
                    enabled = false;
                    updateRoundMode = false;
                }
            }
        }

        private bool determineHoveredElements(bool isNode)
        {
            bool flag = !UIView.IsInsideUI() && Cursor.visible;
            if (flag)
            {
                m_hover = 0;
                m_hoverSegment = 0;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastInput input = new RaycastInput(ray, Camera.main.farClipPlane);
                input.m_netService.m_itemLayers = (ItemClass.Layer.Default | ItemClass.Layer.MetroTunnels);
                input.m_netService.m_service = ItemClass.Service.Road;
                input.m_ignoreTerrain = true;
                input.m_ignoreNodeFlags = NetNode.Flags.None;
                if (ToolBase.RayCast(input, out RaycastOutput output) && isNode)
                {
                    m_hover = output.m_netNode;
                }

                ushort HoveredSegmentId = 0;
                RaycastInput input2 = new RaycastInput(ray, Camera.main.farClipPlane);
                input2.m_netService.m_itemLayers = (ItemClass.Layer.Default | ItemClass.Layer.MetroTunnels);
                input2.m_netService.m_service = ItemClass.Service.Road;
                input2.m_ignoreTerrain = true;
                input2.m_ignoreSegmentFlags = NetSegment.Flags.None;
                if (ToolBase.RayCast(input2, out RaycastOutput output2))
                {
                    HoveredSegmentId = output2.m_netSegment;
                }
                
                if (m_hover <= 0 && HoveredSegmentId > 0)
                {
                    ushort startNode = Singleton<NetManager>.instance.m_segments.m_buffer[HoveredSegmentId].m_startNode;
                    ushort endNode = Singleton<NetManager>.instance.m_segments.m_buffer[HoveredSegmentId].m_endNode;
                    float magnitude = (output2.m_hitPos - Singleton<NetManager>.instance.m_nodes.m_buffer[startNode].m_position).magnitude;
                    float magnitude2 = (output2.m_hitPos - Singleton<NetManager>.instance.m_nodes.m_buffer[endNode].m_position).magnitude;
                    if (magnitude < magnitude2 && magnitude < 75f && isNode)
                    {
                        m_hover = startNode;
                    }
                    else if (magnitude2 < magnitude && magnitude2 < 75f && isNode)
                    {
                        m_hover = endNode;
                    }
                    m_hoverSegment = HoveredSegmentId;
                }
                if (m_hover == 0)
                {
                    return HoveredSegmentId != 0;
                }
                return true;
            }
            return flag;
        }

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            if (enabled == true)
            {
                currentMoney = 0;
                if (m_hover != 0 && (m_step != 2))
                {
                    ushort netNode = m_hover;
                    var Instance = Singleton<NetManager>.instance;
                    NetInfo info = Instance.m_nodes.m_buffer[netNode].Info;
                    Vector3 position = Instance.m_nodes.m_buffer[netNode].m_position;
                    float alpha = 1f;
                    var toolColor = new Color32(95, 166, 0, 244);
                    NetTool.CheckOverlayAlpha(info, ref alpha);
                    toolColor.a = (byte)(toolColor.a * alpha);
                    Singleton<RenderManager>.instance.OverlayEffect.DrawCircle(cameraInfo, toolColor, position, Mathf.Max(6f, info.m_halfWidth * 2f), -1f, 1280f, renderLimits: false, alphaBlend: true);
                }

                if (storedNum != 255)
                {
                    for (int i = 0; i <= storedNum; i++)
                    {
                        if (storedRampMode[i] == 0)
                        {
                            Build3RoundRoad(false, true, false, true, (byte)i, (byte)i, cameraInfo, out bool _);
                        }
                        else if (storedRampMode[i] == 2)
                        {
                            Build1RoundRoad(false, true, false, true, (byte)i, (byte)i, cameraInfo, out bool _);
                        }
                    }
                }

                if (m_step == 2)
                {
                    if (rampMode == 0)
                    {
                        Build3RoundRoad(false, true, false, false, 0, 0, cameraInfo, out bool _);
                    }
                    else if (rampMode == 2)
                    {
                        Build1RoundRoad(false, true, false, false, 0, 0, cameraInfo, out bool _);
                    }
                    else if (rampMode == 1)
                    {
                        BuildYRoad(true, false);
                    }
                }

                if (m_step ==1 || m_step == 2)
                {
                    if (rampMode == 3)
                    {
                        if (Manager.m_nodes.m_buffer[node0].GetSegment(preFixSegmentIndex) != 0)
                        {
                            float alpha = 1f;
                            var toolColor = new Color32(95, 166, 0, 244);
                            NetTool.CheckOverlayAlpha(Manager.m_segments.m_buffer[Manager.m_nodes.m_buffer[node0].GetSegment(preFixSegmentIndex)].Info, ref alpha);
                            toolColor.a = (byte)(toolColor.a * alpha);
                            NetTool.RenderOverlay(cameraInfo, ref Manager.m_segments.m_buffer[Manager.m_nodes.m_buffer[node0].GetSegment(preFixSegmentIndex)], toolColor, toolColor);
                        }
                    }
                }

                if (storedNum != 255)
                {
                    for (int i = 0; i <= storedNum; i++)
                    {
                        if (storedRampMode[i] == 0)
                        {
                            if (hoveredRoundIndex == (byte)i)
                            {
                                Singleton<RenderManager>.instance.OverlayEffect.DrawCircle(cameraInfo, new Color32(10, 70, 0, 90), storedPos1[i], 16f, -1f, 1280f, renderLimits: false, alphaBlend: true);
                            }
                            else
                            {
                                Singleton<RenderManager>.instance.OverlayEffect.DrawCircle(cameraInfo, new Color32(45, 126, 0, 214), storedPos1[i], 16f, -1f, 1280f, renderLimits: false, alphaBlend: true);
                            }
                        }
                    }
                }
            }
        }

        public override void RenderGeometry(RenderManager.CameraInfo cameraInfo)
        {
            if (enabled == true)
            {
                if (storedNum != 255)
                {
                    for (int i = 0; i <= storedNum; i++)
                    {
                        if (storedRampMode[i] == 0)
                        {
                            Build3RoundRoad(true, true, false, true, (byte)i, (byte)i, cameraInfo, out bool _);
                        }
                        else if (storedRampMode[i] == 2)
                        {
                            Build1RoundRoad(true, true, false, true, (byte)i, (byte)i, cameraInfo, out bool _);
                        }
                    }
                }

                if (m_step == 2)
                {
                    if (rampMode == 0)
                    {
                        Build3RoundRoad(true, true, false, false, 0, 0, cameraInfo, out bool _);
                    }
                    else if (rampMode == 1)
                    {
                        BuildYRoad(true, true);
                    }
                    else if (rampMode == 2)
                    {
                        Build1RoundRoad(true, true, false, false, 0, 0, cameraInfo, out bool _);
                    }
                }
            }
        }

        private void AdjustElevation(ushort startNode, float elevation)
        {
            var nm = Singleton<NetManager>.instance;
            var node = nm.m_nodes.m_buffer[startNode];
            var ele = (float)Mathf.Clamp(Mathf.RoundToInt(Math.Max(node.m_elevation, elevation)), 0, 255);
            if (elevation < 0)
            {
                ele = (float)Mathf.Clamp(Mathf.RoundToInt(Math.Min(node.m_elevation, elevation)), -255, 0);
            }
            var terrain = Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(node.m_position, false, 0f);
            if (ele > 0) node.m_elevation = (byte)ele;
            else node.m_elevation = (byte)(-ele);
            node.m_position = new Vector3(node.m_position.x, ele + terrain, node.m_position.z);
            if (elevation < -0f)
            {
                node.m_flags |= NetNode.Flags.Underground;
                node.m_flags &= ~NetNode.Flags.OnGround;
            }
            else if (elevation < 1f)
            {
                node.m_flags |= NetNode.Flags.OnGround;
            }
            else
            {
                node.m_flags &= ~NetNode.Flags.OnGround;
                UpdateSegment(node.m_segment0, elevation);
                UpdateSegment(node.m_segment1, elevation);
                UpdateSegment(node.m_segment2, elevation);
                UpdateSegment(node.m_segment3, elevation);
                UpdateSegment(node.m_segment4, elevation);
                UpdateSegment(node.m_segment5, elevation);
                UpdateSegment(node.m_segment6, elevation);
                UpdateSegment(node.m_segment7, elevation);
            }
            nm.m_nodes.m_buffer[startNode] = node;
            //Singleton<NetManager>.instance.UpdateNode(startNode);
        }

        private void AdjustElevationDontFollowTerrain(ushort startNode, float elevation)
        {
            var nm = Singleton<NetManager>.instance;
            var node = nm.m_nodes.m_buffer[startNode];
            var ele = (float)Mathf.Clamp(Mathf.RoundToInt(Math.Max(node.m_elevation, elevation)), 0, 255);
            if (elevation < 0)
            {
                ele = (float)Mathf.Clamp(Mathf.RoundToInt(Math.Min(node.m_elevation, elevation)), -255, 0);
            }
            //var terrain = Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(node.m_position, false, 0f);
            if (ele > 0) node.m_elevation = (byte)ele;
            else node.m_elevation = (byte)(-ele);
            //node.m_position = new Vector3(node.m_position.x, ele + terrain, node.m_position.z);
            if (elevation < -0f)
            {
                node.m_flags |= NetNode.Flags.Underground;
                node.m_flags &= ~NetNode.Flags.OnGround;
            }
            else if (elevation < 1f)
            {
                node.m_flags |= NetNode.Flags.OnGround;
            }
            else
            {
                node.m_flags &= ~NetNode.Flags.OnGround;
                UpdateSegment(node.m_segment0, elevation);
                UpdateSegment(node.m_segment1, elevation);
                UpdateSegment(node.m_segment2, elevation);
                UpdateSegment(node.m_segment3, elevation);
                UpdateSegment(node.m_segment4, elevation);
                UpdateSegment(node.m_segment5, elevation);
                UpdateSegment(node.m_segment6, elevation);
                UpdateSegment(node.m_segment7, elevation);
            }
            nm.m_nodes.m_buffer[startNode] = node;
            //Singleton<NetManager>.instance.UpdateNode(startNode);
        }

        private void UpdateSegment(ushort segmentId, float elevation)
        {
            if (segmentId == 0)
            {
                return;
            }
            var netManager = Singleton<NetManager>.instance;
            if (elevation > 4 || elevation < -8)
            {
                var errors = default(ToolBase.ToolErrors);
                netManager.m_segments.m_buffer[segmentId].Info =
                    netManager.m_segments.m_buffer[segmentId].Info.m_netAI.GetInfo(elevation, elevation, 5, false, false, false, false, ref errors);
            }
        }

        private void CreateNode(out ushort startNode, ref Randomizer rand, NetInfo netInfo, Vector3 oldPos)
        {
            var pos = new Vector3(oldPos.x, 0, oldPos.z);
            pos.y = Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(pos, false, 0f);
            var nm = Singleton<NetManager>.instance;
            nm.CreateNode(out startNode, ref rand, netInfo, pos,
                Singleton<SimulationManager>.instance.m_currentBuildIndex);
            Singleton<SimulationManager>.instance.m_currentBuildIndex += 1u;
        }

        private void CreateNodeDontFollowTerrain(out ushort startNode, ref Randomizer rand, NetInfo netInfo, Vector3 oldPos)
        {
            var pos = new Vector3(oldPos.x, oldPos.y, oldPos.z);
            //pos.y = Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(pos, false, 0f);
            var nm = Singleton<NetManager>.instance;
            nm.CreateNode(out startNode, ref rand, netInfo, pos,
                Singleton<SimulationManager>.instance.m_currentBuildIndex);
            Singleton<SimulationManager>.instance.m_currentBuildIndex += 1u;
        }

        private Vector3 FollowTerrain(Vector3 oldPos, float elevation)
        {
            oldPos.y = Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(oldPos, false, 0f) + elevation;
            return oldPos;
        }

        public Vector3 GetNodeDir(ushort node)
        {
            var nm = Singleton<NetManager>.instance;
            var m_node = nm.m_nodes.m_buffer[node];
            for (int i =0; i < 8; i ++)
            {
                if(m_node.GetSegment(i) !=0)
                {
                    if (Singleton<NetManager>.instance.m_segments.m_buffer[m_node.GetSegment(i)].m_startNode == node)
                    {
                        return Singleton<NetManager>.instance.m_segments.m_buffer[m_node.GetSegment(i)].m_startDirection;
                    }
                    else if (Singleton<NetManager>.instance.m_segments.m_buffer[m_node.GetSegment(i)].m_endNode == node)
                    {
                        return Singleton<NetManager>.instance.m_segments.m_buffer[m_node.GetSegment(i)].m_endDirection;
                    }
                }
            }

            return Vector3.zero;
        }

        public float BezierDistance(Bezier3 bz, float startP, float endP)
        {
            float distance = 0;
            int startPosP = (int)(100 * startP);
            int endPosP = (int)(100 * endP);
            for (int i = startPosP; i < endPosP; i ++)
            {
                if (i > 0)
                {
                    var startPosFloat = (float)(i - 1) / 100f;
                    var endPosFloat = (float)(i) / 100f;
                    distance += Vector2.Distance(VectorUtils.XZ(bz.Position(startPosFloat)), VectorUtils.XZ(bz.Position(endPosFloat)));
                }
            }
            return distance;
        }

        public void Build3RoundRoad(bool onlyShowMesh, bool onlyShow, bool store, bool load, byte storeIndex, byte loadIndex, RenderManager.CameraInfo cameraInfo, out bool isUpdate)
        {
            Bezier3 partA = default(Bezier3);
            Bezier3 partB = default(Bezier3);
            Bezier3 partC = default(Bezier3);
            Bezier3 partD = default(Bezier3);
            Bezier3 partE = default(Bezier3);
            Vector3 m_pos0 = pos0;
            Vector3 m_pos1 = pos1;
            Vector3 m_pos2 = pos2;
            float m_elevation = height;
            float m_radius = radius;
            ushort m_node0 = node0;
            ushort m_node2 = node2;
            NetInfo m_loacalNetInfo = m_netInfo;
            isUpdate = false;

            if (load)
            {
                if (loadIndex >= 8)
                {
                    return;
                }

                if (storedRampMode[loadIndex] == 2)
                {
                    return;
                }

                m_pos0 = storedPos0[loadIndex];
                m_pos1 = storedPos1[loadIndex];
                m_pos2 = storedPos2[loadIndex];
                m_elevation = storedElevation[loadIndex];
                m_radius = storedRadius[loadIndex];
                m_node0 = storedNode0[loadIndex];
                m_node2 = storedNode2[loadIndex];
                m_loacalNetInfo = storedNetInfo[loadIndex];
            }

            var rand = new Randomizer(0u);
            var m_prefab = m_loacalNetInfo;
            ToolErrors errors = default(ToolErrors);
            var netInfo = m_prefab.m_netAI.GetInfo(m_elevation, m_elevation, 5, false, false, false, false, ref errors);
            if (OptionUI.isSmoothMode)
            {
                netInfo = m_prefab.m_netAI.GetInfo(5, 5, 5, false, false, false, false, ref errors);
            }

            if (m_node2 == 0)
            {
                return;
            }

            FindNodeA(true, m_pos0, GetNodeDir(m_node0), m_pos1, m_radius * 8f, out Vector3 NodeA1, out Vector3 NodeA1Dir, out Vector3 startDir);
            FindNodeA(false, m_pos2, GetNodeDir(m_node2), m_pos1, m_radius * 8f, out Vector3 NodeA2, out Vector3 NodeA2Dir, out Vector3 endDir);
            endDir = -endDir;

            partA.a = m_pos0;
            partA.d = NodeA1;
            CustomNetSegment.CalculateMiddlePoints(m_pos0, startDir, NodeA1, -VectorUtils.NormalizeXZ(NodeA1Dir), true, true, out partA.b, out partA.c);
            partB.a = NodeA1;
            int angle;
            GetRoundCurve(NodeA1, m_pos1, NodeA2, 85, out Vector3 NodeB1, out Vector3 NodeB1Dir, false, out angle);
            partB.d = NodeB1;
            CustomNetSegment.CalculateMiddlePoints(NodeA1, VectorUtils.NormalizeXZ(NodeA1Dir), NodeB1, -NodeB1Dir, true, true, out partB.b, out partB.c);
            partC.a = NodeB1;
            GetRoundCurve(NodeA1, m_pos1, NodeA2, 170, out Vector3 NodeB2, out Vector3 NodeB2Dir, false, out angle);
            partC.d = NodeB2;
            CustomNetSegment.CalculateMiddlePoints(NodeB1, NodeB1Dir, NodeB2, -NodeB2Dir, true, true, out partC.b, out partC.c);
            partD.a = NodeB2;
            partD.d = NodeA2;
            CustomNetSegment.CalculateMiddlePoints(NodeB2, NodeB2Dir, NodeA2, -VectorUtils.NormalizeXZ(NodeA2Dir), true, true, out partD.b, out partD.c);
            partE.a = NodeA2;
            partE.d = m_pos2;
            CustomNetSegment.CalculateMiddlePoints(NodeA2, VectorUtils.NormalizeXZ(NodeA2Dir), m_pos2, -endDir, true, true, out partE.b, out partE.c);

            if (NodeA1 == Vector3.zero)
            {
                CustomShowExtraInfo(true, Localization.Get("InvalidShape"), pos);
                isUpdate = true;
                return;
            }
            if (NodeB1 == Vector3.zero)
            {
                CustomShowExtraInfo(true, Localization.Get("InvalidShape"), pos);
                isUpdate = true;
                return;
            }
            if (NodeB2 == Vector3.zero)
            {
                CustomShowExtraInfo(true, Localization.Get("InvalidShape"), pos);
                isUpdate = true;
                return;
            }
            if (NodeA2 == Vector3.zero)
            {
                CustomShowExtraInfo(true, Localization.Get("InvalidShape"), pos);
                isUpdate = true;
                return;
            }

            CustomShowExtraInfo(false, null, Vector3.zero);

            if (store)
            {
                if (rampMode == 2)
                {
                    return;
                }

                for (int i = 0; i < storeIndex; i++)
                {
                    //update mode.
                    if ((storedPos0[i] == m_pos0) && (storedPos2[i] == m_pos2) && (storedNode0[i] == m_node0) && (storedNode2[i] == m_node2))
                    {
                        isUpdate = true;
                        storeIndex = (byte)i;
                    }
                }

                if (storeIndex >= 8)
                {
                    return;
                }
                storedRampMode[storeIndex] = rampMode;

                storedPos0[storeIndex] = m_pos0;
                storedPos1[storeIndex] = m_pos1;
                storedPos2[storeIndex] = m_pos2;
                storedElevation[storeIndex] = m_elevation;
                storedRadius[storeIndex] = m_radius;
                storedNode0[storeIndex] = m_node0;
                storedNode2[storeIndex] = m_node2;
                storedNetInfo[storeIndex] = m_loacalNetInfo;
                return;
            }

            int m_nodeNum = 0;
            int partANum = (int)(BezierDistance(partA, 0, 1) / (OptionUI.nodeGap * 8f));
            int partRoundNum = (int)(16f * Math.PI * (float)radius * ((float)angle / 360f) / (OptionUI.nodeGap * 8f));
            int partENum = (int)(BezierDistance(partE, 0, 1) / (OptionUI.nodeGap * 8f));

            m_nodeNum += partANum + 1;
            m_nodeNum += partRoundNum + 1;
            m_nodeNum += partENum;
            ushort[] node = new ushort[m_nodeNum];
            ushort[] segment = new ushort[m_nodeNum];

            //smooth mode, recalculate bezier
            float totalDistance = 0;
            float heightDiff = m_pos0.y - m_pos2.y;
            float partALength = BezierDistance(partA, 0, 1);
            float partRoundLength = 16f * (float)Math.PI * (float)radius * ((float)angle / 360f);
            float partELength = BezierDistance(partE, 0, 1);
            if (OptionUI.isSmoothMode)
            {
                m_elevation = 0;
                totalDistance += partALength;
                totalDistance += partRoundLength;
                totalDistance += partELength;
            }

            for (int i = 0; i <= partANum; i++)
            {
                float p1 = (float)(i + 1) / (float)(partANum + 1);
                float p2 = (float)(i) / (float)(partANum + 1);
                if (!onlyShow)
                {
                    if (!OptionUI.isSmoothMode)
                    {
                        CreateNode(out node[i], ref rand, netInfo, partA.Position(p1));
                        AdjustElevation(node[i], m_elevation);
                    }
                    else
                    {
                        var height = m_pos0.y - (heightDiff * BezierDistance(partA, 0, p1) / totalDistance);
                        var position = new Vector3(partA.Position(p1).x, height, partA.Position(p1).z);
                        CreateNodeDontFollowTerrain(out node[i], ref rand, netInfo, position);
                    }

                    if (i == 0)
                    {
                        if (!OptionUI.isSmoothMode)
                        {
                            var tmpElevationMin = 0f;
                            var tmpElevationMax = 0f;
                            if (Singleton<NetManager>.instance.m_nodes.m_buffer[m_node0].m_flags.IsFlagSet(NetNode.Flags.Underground))
                            {
                                tmpElevationMin = (-Singleton<NetManager>.instance.m_nodes.m_buffer[m_node0].m_elevation > m_elevation) ? m_elevation : -Singleton<NetManager>.instance.m_nodes.m_buffer[m_node0].m_elevation;
                                tmpElevationMax = (-Singleton<NetManager>.instance.m_nodes.m_buffer[m_node0].m_elevation > m_elevation) ? -Singleton<NetManager>.instance.m_nodes.m_buffer[m_node0].m_elevation : m_elevation;
                            }
                            else
                            {
                                tmpElevationMin = (Singleton<NetManager>.instance.m_nodes.m_buffer[m_node0].m_elevation > m_elevation) ? m_elevation : Singleton<NetManager>.instance.m_nodes.m_buffer[m_node0].m_elevation;
                                tmpElevationMax = (Singleton<NetManager>.instance.m_nodes.m_buffer[m_node0].m_elevation > m_elevation) ? Singleton<NetManager>.instance.m_nodes.m_buffer[m_node0].m_elevation : m_elevation;
                            }
                            var tmpNetInfo = m_prefab.m_netAI.GetInfo(tmpElevationMin, tmpElevationMax, 5, false, false, false, false, ref errors);
                            if (tmpElevationMin < -8f && tmpElevationMax > -8f)
                            {
                                if (Singleton<NetManager>.instance.m_nodes.m_buffer[m_node0].m_position.y > Singleton<NetManager>.instance.m_nodes.m_buffer[node[i]].m_position.y)
                                {
                                    if (Singleton<NetManager>.instance.CreateSegment(out segment[i], ref rand, tmpNetInfo, node[i], m_node0, -VectorUtils.NormalizeXZ(partA.Tangent(p1)), VectorUtils.NormalizeXZ(startDir), Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, true))
                                        Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                                }
                                else
                                {
                                    if (Singleton<NetManager>.instance.CreateSegment(out segment[i], ref rand, tmpNetInfo, m_node0, node[i], VectorUtils.NormalizeXZ(startDir), -VectorUtils.NormalizeXZ(partA.Tangent(p1)), Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                                        Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                                }
                            }
                            else
                            {
                                if (Singleton<NetManager>.instance.CreateSegment(out segment[i], ref rand, tmpNetInfo, m_node0, node[i], VectorUtils.NormalizeXZ(startDir), -VectorUtils.NormalizeXZ(partA.Tangent(p1)), Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                                    Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                            }
                        }
                        else
                        {
                            if (Singleton<NetManager>.instance.CreateSegment(out segment[i], ref rand, netInfo, m_node0, node[i], VectorUtils.NormalizeXZ(startDir), -VectorUtils.NormalizeXZ(partA.Tangent(p1)), Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                                Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                        }
                    }
                    else
                    {
                        if (!OptionUI.isSmoothMode)
                        {
                            if (Singleton<NetManager>.instance.CreateSegment(out segment[i], ref rand, netInfo, node[i - 1], node[i], VectorUtils.NormalizeXZ(partA.Tangent(p2)), -VectorUtils.NormalizeXZ(partA.Tangent(p1)), Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                                Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                        }
                        else
                        {
                            if (Singleton<NetManager>.instance.CreateSegment(out segment[i], ref rand, netInfo, node[i - 1], node[i], VectorUtils.NormalizeXZ(partA.Tangent(p2)), -VectorUtils.NormalizeXZ(partA.Tangent(p1)), Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                                Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                        }
                    }
                }
                else
                {
                    if (i == 0)
                    {
                        currentMoney += netInfo.m_netAI.GetConstructionCost(m_pos0, partA.Position(p1), Singleton<NetManager>.instance.m_nodes.m_buffer[m_node0].m_elevation, m_elevation);
                    }
                    else
                    {
                        currentMoney += netInfo.m_netAI.GetConstructionCost(partA.Position(p2), partA.Position(p1), m_elevation, m_elevation);
                    }
                }
            }

            for (int i = 0; i <= partRoundNum; i++)
            {
                float p1 = (float)(i + 1) / (float)(partRoundNum + 1);
                float p2 = (float)i / (float)(partRoundNum + 1);
                GetRoundCurve(NodeA1, m_pos1, NodeA2, (byte)(p1 * 255f), out Vector3 nodePos, out Vector3 nodeDir, false, out angle);
                GetRoundCurve(NodeA1, m_pos1, NodeA2, (byte)(p2 * 255f), out Vector3 preNodePos, out Vector3 preNodeDir, false, out angle);
                if (!onlyShow)
                {
                    if (!OptionUI.isSmoothMode)
                    {
                        CreateNode(out node[i + partANum + 1], ref rand, netInfo, nodePos);
                        AdjustElevation(node[i + partANum + 1], m_elevation);
                    }
                    else
                    {
                        var height = m_pos0.y - (heightDiff * (partALength + partRoundLength * p1) / totalDistance);
                        var position = new Vector3(nodePos.x, height, nodePos.z);
                        CreateNodeDontFollowTerrain(out node[i + partANum + 1], ref rand, netInfo, position);
                    }
                    if (Singleton<NetManager>.instance.CreateSegment(out segment[i + partANum + 1], ref rand, netInfo, node[i + partANum], node[i + partANum + 1], preNodeDir, -nodeDir, Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                        Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                }
                else
                {
                    currentMoney += netInfo.m_netAI.GetConstructionCost(preNodePos, nodePos, m_elevation, m_elevation);
                }
            }

            if (partENum > 0)
            {
                for (int i = 1; i <= partENum; i++)
                {
                    float p1 = (float)i / (float)(partENum + 1);
                    float p2 = (float)(i - 1) / (float)(partENum + 1);
                    if (!onlyShow)
                    {
                        if (!OptionUI.isSmoothMode)
                        {
                            CreateNode(out node[i + partANum + partRoundNum + 1], ref rand, netInfo, partE.Position(p1));
                            AdjustElevation(node[i + partANum + partRoundNum + 1], m_elevation);
                        }
                        else
                        {
                            var height = m_pos0.y - (heightDiff * (partALength + partRoundLength + BezierDistance(partE, 0, p1)) / totalDistance);
                            var position = new Vector3(partE.Position(p1).x, height, partE.Position(p1).z);
                            CreateNodeDontFollowTerrain(out node[i + partANum + partRoundNum + 1], ref rand, netInfo, position);
                        }
                        if (Singleton<NetManager>.instance.CreateSegment(out segment[i + partANum + partRoundNum + 1], ref rand, netInfo, node[i + partANum + partRoundNum], node[i + partANum + partRoundNum + 1], VectorUtils.NormalizeXZ(partE.Tangent(p2)), -VectorUtils.NormalizeXZ(partE.Tangent(p1)), Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                            Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                    }
                    else
                    {
                        currentMoney += netInfo.m_netAI.GetConstructionCost(partE.Position(p2), partE.Position(p1), m_elevation, m_elevation);
                    }
                }
            }

            if (!onlyShow)
            {
                ushort segmentId;
                float tmp = (float)partENum / (float)(partENum + 1);
                if (!OptionUI.isSmoothMode)
                {
                    var tmpElevationMin = 0f;
                    var tmpElevationMax = 0f;
                    if (Singleton<NetManager>.instance.m_nodes.m_buffer[m_node2].m_flags.IsFlagSet(NetNode.Flags.Underground))
                    {
                        tmpElevationMin = (-Singleton<NetManager>.instance.m_nodes.m_buffer[m_node2].m_elevation > m_elevation) ? m_elevation : -Singleton<NetManager>.instance.m_nodes.m_buffer[m_node2].m_elevation;
                        tmpElevationMax = (-Singleton<NetManager>.instance.m_nodes.m_buffer[m_node2].m_elevation > m_elevation) ? -Singleton<NetManager>.instance.m_nodes.m_buffer[m_node2].m_elevation : m_elevation;
                    }
                    else
                    {
                        tmpElevationMin = (Singleton<NetManager>.instance.m_nodes.m_buffer[m_node2].m_elevation > m_elevation) ? m_elevation : Singleton<NetManager>.instance.m_nodes.m_buffer[m_node2].m_elevation;
                        tmpElevationMax = (Singleton<NetManager>.instance.m_nodes.m_buffer[m_node2].m_elevation > m_elevation) ? Singleton<NetManager>.instance.m_nodes.m_buffer[m_node2].m_elevation : m_elevation;
                    }
                    var tmpNetInfo = m_prefab.m_netAI.GetInfo(tmpElevationMin, tmpElevationMax, 5, false, false, false, false, ref errors);
                    if (tmpElevationMin < -8f && tmpElevationMax > -8f)
                    {
                        if (Singleton<NetManager>.instance.m_nodes.m_buffer[m_node2].m_position.y < Singleton<NetManager>.instance.m_nodes.m_buffer[node[partANum + partRoundNum + partENum + 1]].m_position.y)
                        {
                            if (Singleton<NetManager>.instance.CreateSegment(out segmentId, ref rand, tmpNetInfo, m_node2, node[partANum + partRoundNum + partENum + 1], -VectorUtils.NormalizeXZ(endDir), VectorUtils.NormalizeXZ(partE.Tangent(tmp)), Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, true))
                                Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                        }
                        else
                        {
                            if (Singleton<NetManager>.instance.CreateSegment(out segmentId, ref rand, tmpNetInfo, node[partANum + partRoundNum + partENum + 1], m_node2, VectorUtils.NormalizeXZ(partE.Tangent(tmp)), -VectorUtils.NormalizeXZ(endDir), Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                                Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                        }
                    }
                    else
                    {
                        if (Singleton<NetManager>.instance.CreateSegment(out segmentId, ref rand, tmpNetInfo, node[partANum + partRoundNum + partENum + 1], m_node2, VectorUtils.NormalizeXZ(partE.Tangent(tmp)), -VectorUtils.NormalizeXZ(endDir), Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                            Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                    }
                }
                else
                {
                    if (Singleton<NetManager>.instance.CreateSegment(out segmentId, ref rand, netInfo, node[partANum + partRoundNum + partENum + 1], m_node2, VectorUtils.NormalizeXZ(partE.Tangent(tmp)), -VectorUtils.NormalizeXZ(endDir), Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                        Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                }
            }
            else
            {
                currentMoney += netInfo.m_netAI.GetConstructionCost(partE.Position((float)partENum / (float)(partENum + 1)), m_pos2, m_elevation, Singleton<NetManager>.instance.m_nodes.m_buffer[m_node2].m_elevation);
            }

            if (onlyShow && !onlyShowMesh && !OptionUI.dontUseShaderPreview)
            {
                Singleton<RenderManager>.instance.OverlayEffect.DrawBezier(cameraInfo, m_validColorInfo, partA, Mathf.Max(6f, netInfo.m_halfWidth * 2f), -100000f, -100000f, -1f, 1280f, renderLimits: false, alphaBlend: false);
                Singleton<RenderManager>.instance.OverlayEffect.DrawBezier(cameraInfo, m_validColorInfo, partB, Mathf.Max(6f, netInfo.m_halfWidth * 2f), -100000f, -100000f, -1f, 1280f, renderLimits: false, alphaBlend: false);
                Singleton<RenderManager>.instance.OverlayEffect.DrawBezier(cameraInfo, m_validColorInfo, partC, Mathf.Max(6f, netInfo.m_halfWidth * 2f), -100000f, -100000f, -1f, 1280f, renderLimits: false, alphaBlend: false);
                Singleton<RenderManager>.instance.OverlayEffect.DrawBezier(cameraInfo, m_validColorInfo, partD, Mathf.Max(6f, netInfo.m_halfWidth * 2f), -100000f, -100000f, -1f, 1280f, renderLimits: false, alphaBlend: false);
                Singleton<RenderManager>.instance.OverlayEffect.DrawBezier(cameraInfo, m_validColorInfo, partE, Mathf.Max(6f, netInfo.m_halfWidth * 2f), -100000f, -100000f, -1f, 1280f, renderLimits: false, alphaBlend: false);
            }

            if (onlyShowMesh)
            {
                if (!OptionUI.isSmoothMode)
                {
                    RenderSegment(netInfo, NetSegment.Flags.None, FollowTerrain(partA.Position(0), m_elevation + 1f), FollowTerrain(partA.Position(1), m_elevation + 1f), VectorUtils.NormalizeXZ(partA.Tangent(0)), VectorUtils.NormalizeXZ(partA.Tangent(1)), true, true);
                    RenderSegment(netInfo, NetSegment.Flags.None, FollowTerrain(partB.Position(0), m_elevation + 1f), FollowTerrain(partB.Position(1), m_elevation + 1f), VectorUtils.NormalizeXZ(partB.Tangent(0)), VectorUtils.NormalizeXZ(partB.Tangent(1)), true, true);
                    RenderSegment(netInfo, NetSegment.Flags.None, FollowTerrain(partC.Position(0), m_elevation + 1f), FollowTerrain(partC.Position(1), m_elevation + 1f), VectorUtils.NormalizeXZ(partC.Tangent(0)), VectorUtils.NormalizeXZ(partC.Tangent(1)), true, true);
                    RenderSegment(netInfo, NetSegment.Flags.None, FollowTerrain(partD.Position(0), m_elevation + 1f), FollowTerrain(partD.Position(1), m_elevation + 1f), VectorUtils.NormalizeXZ(partD.Tangent(0)), VectorUtils.NormalizeXZ(partD.Tangent(1)), true, true);
                    RenderSegment(netInfo, NetSegment.Flags.None, FollowTerrain(partE.Position(0), m_elevation + 1f), FollowTerrain(partE.Position(1), m_elevation + 1f), VectorUtils.NormalizeXZ(partE.Tangent(0)), VectorUtils.NormalizeXZ(partE.Tangent(1)), true, true);
                }
                else
                {
                    var heightA = m_pos0.y - (heightDiff * (BezierDistance(partA, 0, 1) / totalDistance));
                    var heightB = m_pos0.y - (heightDiff * ((BezierDistance(partA, 0, 1) + BezierDistance(partB, 0, 1)) / totalDistance));
                    var heightC = m_pos0.y - (heightDiff * ((BezierDistance(partA, 0, 1) + BezierDistance(partB, 0, 1) + BezierDistance(partC, 0, 1)) / totalDistance));
                    var heightD = m_pos0.y - (heightDiff * ((BezierDistance(partA, 0, 1) + BezierDistance(partB, 0, 1) + BezierDistance(partC, 0, 1) + BezierDistance(partD, 0, 1)) / totalDistance));
                    RenderSegment(netInfo, NetSegment.Flags.None, partA.Position(0), SetHeight(partA.Position(1), heightA), VectorUtils.NormalizeXZ(partA.Tangent(0)), VectorUtils.NormalizeXZ(partA.Tangent(1)), true, true);
                    RenderSegment(netInfo, NetSegment.Flags.None, SetHeight(partB.Position(0), heightA), SetHeight(partB.Position(1), heightB), VectorUtils.NormalizeXZ(partB.Tangent(0)), VectorUtils.NormalizeXZ(partB.Tangent(1)), true, true);
                    RenderSegment(netInfo, NetSegment.Flags.None, SetHeight(partC.Position(0), heightB), SetHeight(partC.Position(1), heightC), VectorUtils.NormalizeXZ(partC.Tangent(0)), VectorUtils.NormalizeXZ(partC.Tangent(1)), true, true);
                    RenderSegment(netInfo, NetSegment.Flags.None, SetHeight(partD.Position(0), heightC), SetHeight(partD.Position(1), heightD), VectorUtils.NormalizeXZ(partD.Tangent(0)), VectorUtils.NormalizeXZ(partD.Tangent(1)), true, true);
                    RenderSegment(netInfo, NetSegment.Flags.None, SetHeight(partE.Position(0), heightD), partE.Position(1), VectorUtils.NormalizeXZ(partE.Tangent(0)), VectorUtils.NormalizeXZ(partE.Tangent(1)), true, true);
                }
            }
        }

        public void FindNodeA(bool isStart, Vector3 startPos, Vector3 startDir, Vector3 center, float radius, out Vector3 NodeA, out Vector3 NodeADir, out Vector3 startDirFix)
        {
            // construct the vector from startPos to center
            Vector2 dist = VectorUtils.XZ(center - startPos);
            Vector2 startDir2 = VectorUtils.XZ(startDir).normalized;
            // The correct startdir must have an acute angle between dist
            if (Vector2.Dot(startDir2, dist) < 0) startDir2 = -startDir2;
            // isStart controls the normal direction towards the center of the small circle
            // RIGHT-hand normal if isStart==true otherwise LEFT-hand
            Vector2 startNormal = new Vector2(startDir2.y, -startDir2.x);
            if (!isStart) startNormal = -startNormal;
            float phi = Vector2.Angle(startNormal, dist) * RAD;
            // radius of the small circle
            float r1 = (dist.sqrMagnitude - radius * radius) / (2 * radius + 2 * dist.magnitude * Mathf.Cos(phi));
            // calculate the central angle for NodeA
            float sinTheta = dist.magnitude * Mathf.Sin(phi) / (radius + r1);
            float cosTheta = Mathf.Sqrt(1 - sinTheta * sinTheta);
            if (!isStart) sinTheta = -sinTheta;
            // calculte NodeA position and tangent
            Vector2 nodeANormal = new Vector2(startNormal.x * cosTheta + startNormal.y * sinTheta, -startNormal.x * sinTheta + startNormal.y * cosTheta);
            Vector2 nodeA2 = VectorUtils.XZ(startPos) + (startNormal - nodeANormal) * r1;
            NodeA = new Vector3(nodeA2.x, startPos.y, nodeA2.y);
            NodeADir = new Vector3(-nodeANormal.y, 0, nodeANormal.x);
            //if (!isStart) NodeADir = -NodeADir;
            startDirFix = new Vector3(startDir2.x, 0, startDir2.y);
            //DebugLog.LogToFileOnly($"radius={radius}, phi={phi}, r1={r1}, sinTheta={sinTheta}, cosTheta={cosTheta}, isStart={isStart}");
        }

        protected void CustomShowToolInfo(bool show, string text, Vector3 worldPos)
        {
            if (cursorInfoLabel == null)
            {
                return;
            }
            if (!string.IsNullOrEmpty(text) && show)
            {
                cursorInfoLabel.isVisible = true;
                UIView uIView = cursorInfoLabel.GetUIView();
                Vector2 vector = (!(fullscreenContainer != null)) ? uIView.GetScreenResolution() : fullscreenContainer.size;
                Vector3 v = Camera.main.WorldToScreenPoint(worldPos);
                v /= uIView.inputScale;
                Vector3 vector2 = cursorInfoLabel.pivot.UpperLeftToTransform(cursorInfoLabel.size, cursorInfoLabel.arbitraryPivotOffset);
                Vector3 relativePosition = uIView.ScreenPointToGUI(v) + new Vector2(vector2.x, vector2.y);
                cursorInfoLabel.text = text;
                if (relativePosition.x < 0f)
                {
                    relativePosition.x = 0f;
                }
                if (relativePosition.y < 0f)
                {
                    relativePosition.y = 0f;
                }
                if (relativePosition.x + cursorInfoLabel.width > vector.x)
                {
                    relativePosition.x = vector.x - cursorInfoLabel.width;
                }
                if (relativePosition.y + cursorInfoLabel.height > vector.y)
                {
                    relativePosition.y = vector.y - cursorInfoLabel.height;
                }
                cursorInfoLabel.relativePosition = relativePosition;
            }
            else
            {
                cursorInfoLabel.isVisible = false;
            }
        }

        protected void CustomShowExtraInfo(bool show, string text, Vector3 worldPos)
        {
            if (!((Object)extraInfoLabel == (Object)null))
            {
                if (show)
                {
                    UIView uIView = extraInfoLabel.GetUIView();
                    Vector3 a = Camera.main.WorldToScreenPoint(worldPos);
                    a /= uIView.inputScale;
                    Vector3 relativePosition = uIView.ScreenPointToGUI(a) - extraInfoLabel.size * 0.5f;
                    extraInfoLabel.isVisible = true;
                    extraInfoLabel.textColor = GetToolColor(false, false);
                    if (text != null)
                    {
                        extraInfoLabel.text = text;
                    }
                    extraInfoLabel.relativePosition = relativePosition;
                }
                else
                {
                    extraInfoLabel.isVisible = false;
                }
            }
        }

        public void FindRound(Vector3 pos1, Vector3 pos2, Vector3 pos3, Vector3 endDir, float radius, out Vector3 startPos, out Vector3 roundCenterPos, out Vector3 endPos, out Vector3 startDir, out Vector3 endDirFix)
        {
            radius *= 8;
            startDir = VectorUtils.NormalizeXZ(pos2 - pos1);
            endDirFix = VectorUtils.NormalizeXZ(endDir);
            // calculate the intersection between start and end directions
            Line2 startSegment = new Line2(VectorUtils.XZ(pos2), VectorUtils.XZ(pos2 + startDir));
            Line2 endSegment = new Line2(VectorUtils.XZ(pos3), VectorUtils.XZ(pos3 + endDirFix));
            float tStart, tEnd;
            if (!startSegment.Intersect(endSegment, out tStart, out tEnd))
            {
                DebugLog.LogToFileOnly("Warning: start and end directions are parallel, radius has to be fixed!");
                // cannot create curve in this case, return zero vectors
                startPos = Vector3.zero;
                roundCenterPos = Vector3.zero;
                endPos = Vector3.zero;
                return;
            }
            // determine if we need to reverse the end direction to make a CLOCKWISE circle
            // tStart and tEnd should be of opposite signs, otherwise endDir should be reversed
            if (tStart * tEnd > 0)
            {
                endDirFix = -endDirFix;
                tEnd = -tEnd;
            }
            // Calculate central angle
            float angle = Mathf.Abs(Vector2.Angle(VectorUtils.XZ(startDir), VectorUtils.XZ(endDirFix)));
            //DebugLog.LogToFileOnly($"Intersection: tStart = {tStart}, tEnd = {tEnd}");
            if (endDirFix.x * startDir.z - startDir.x * endDirFix.z > 0) angle = 360 - angle;
            // calculate startPos, the distances between startPos-intersection and endPos-intersections are equal
            float dist = radius * Mathf.Tan(angle / 2 * RAD);
            //DebugLog.LogToFileOnly($"Angle = {angle}, dist = {dist}");
            startPos = pos2 + startDir * (tStart + dist);
            endPos = pos3 + endDirFix * (tEnd - dist);
            // Calculate center position
            Vector3 endRadiusDir = new Vector3(endDirFix.z, 0, -endDirFix.x);
            roundCenterPos = endPos + endRadiusDir * radius;
        }

        public void GetRoundCurve(Vector3 startPos, Vector3 roundCenterPos, Vector3 endPos, byte idex, out Vector3 pos, out Vector3 dir, bool isClockwise, out int angle)
        {
            const float RADIUS_TOL = 4f;
            Vector2 startRadius = VectorUtils.XZ(startPos) - VectorUtils.XZ(roundCenterPos);
            Vector2 endRadius = VectorUtils.XZ(endPos) - VectorUtils.XZ(roundCenterPos);
            if (Mathf.Abs(startRadius.magnitude - endRadius.magnitude) >= RADIUS_TOL)
            {
                //DebugLog.LogToFileOnly("Warning, endPos is not on the circle!");
            }
            // the value from arccos, only between 0 and 180
            float centralAngle = Vector2.Angle(startRadius, endRadius);
            // use the cross product to determine whether end is at start's right hand side
            if (endRadius.x * startRadius.y - startRadius.x * endRadius.y < 0)
            {
                centralAngle = 360 - centralAngle;
            }

            if (!isClockwise)
            {
                centralAngle -= 360;
            }

            angle = (int)Mathf.Abs(centralAngle);

            float cos = Mathf.Cos(centralAngle * idex / 255 * RAD);
            float sin = Mathf.Sin(centralAngle * idex / 255 * RAD);
            dir = new Vector3(startRadius.x * cos + startRadius.y * sin, 0, -startRadius.x * sin + startRadius.y * cos);
            pos = roundCenterPos + dir;
            if (isClockwise)
            {
                dir = new Vector3(dir.z, 0, -dir.x);
            }
            else
            {
                dir = new Vector3(-dir.z, 0, dir.x);
            }
            dir = VectorUtils.NormalizeXZ(dir);
        }

        public void Build1RoundRoad(bool onlyShowMesh, bool onlyShow, bool store, bool load, byte storeIndex, byte loadIndex, RenderManager.CameraInfo cameraInfo, out bool isUpdate)
        {
            Bezier3 partA = default(Bezier3);
            Bezier3 partB = default(Bezier3);
            Bezier3 partC = default(Bezier3);
            Bezier3 partC1 = default(Bezier3);
            Bezier3 partD = default(Bezier3);
            Bezier3 partE = default(Bezier3);
            isUpdate = false;
            Vector3 m_pos0 = pos0;
            Vector3 m_pos1 = pos1;
            Vector3 m_pos2 = pos2;
            float m_elevation = height;
            float m_radius = radius;
            ushort m_node1 = node1;
            ushort m_node2 = node2;
            NetInfo m_loacalNetInfo = m_netInfo;
            isUpdate = false;

            if (load)
            {
                if (loadIndex >= 8)
                {
                    return;
                }

                if (storedRampMode[loadIndex] == 0)
                {
                    return;
                }

                m_pos0 = storedPos0[loadIndex];
                m_pos1 = storedPos1[loadIndex];
                m_pos2 = storedPos2[loadIndex];
                m_elevation = storedElevation[loadIndex];
                m_radius = storedRadius[loadIndex];
                m_node1 = storedNode0[loadIndex];
                m_node2 = storedNode2[loadIndex];
                m_loacalNetInfo = storedNetInfo[loadIndex];
            }

            var rand = new Randomizer(0u);
            var m_prefab = m_loacalNetInfo;
            ToolErrors errors = default(ToolErrors);
            var netInfo = m_prefab.m_netAI.GetInfo(m_elevation, m_elevation, 5, false, false, false, false, ref errors);
            if (OptionUI.isSmoothMode)
            {
                netInfo = m_prefab.m_netAI.GetInfo(5, 5, 5, false, false, false, false, ref errors);
            }

            if (m_node2 == 0)
            {
                return;
            }

            FindRound(m_pos0, m_pos1, m_pos2, GetNodeDir(m_node2), m_radius, out Vector3 NodeA1, out Vector3 RoundCenter, out Vector3 NodeA2, out Vector3 NodeA1Dir, out Vector3 endirFix);
            partA.a = m_pos1;
            partA.d = NodeA1;
            CustomNetSegment.CalculateMiddlePoints(m_pos1, VectorUtils.NormalizeXZ(m_pos1 - m_pos0), NodeA1, -VectorUtils.NormalizeXZ(NodeA1Dir), true, true, out partA.b, out partA.c);
            partB.a = NodeA1;
            int angle = 360;
            GetRoundCurve(NodeA1, RoundCenter, NodeA2, 63, out Vector3 NodeB1, out Vector3 NodeB1Dir, true, out angle);
            partB.d = NodeB1;
            CustomNetSegment.CalculateMiddlePoints(NodeA1, VectorUtils.NormalizeXZ(NodeA1Dir), NodeB1, -NodeB1Dir, true, true, out partB.b, out partB.c);
            partC.a = NodeB1;
            GetRoundCurve(NodeA1, RoundCenter, NodeA2, 127, out Vector3 NodeB2, out Vector3 NodeB2Dir, true, out angle);
            partC.d = NodeB2;
            CustomNetSegment.CalculateMiddlePoints(NodeB1, NodeB1Dir, NodeB2, -NodeB2Dir, true, true, out partC.b, out partC.c);
            partC1.a = NodeB2;
            GetRoundCurve(NodeA1, RoundCenter, NodeA2, 191, out Vector3 NodeB21, out Vector3 NodeB21Dir, true, out angle);
            partC1.d = NodeB21;
            CustomNetSegment.CalculateMiddlePoints(NodeB2, NodeB2Dir, NodeB21, -NodeB21Dir, true, true, out partC1.b, out partC1.c);
            partD.a = NodeB21;
            partD.d = NodeA2;
            CustomNetSegment.CalculateMiddlePoints(NodeB21, NodeB21Dir, NodeA2, -endirFix, true, true, out partD.b, out partD.c);
            partE.a = NodeA2;
            partE.d = m_pos2;
            CustomNetSegment.CalculateMiddlePoints(NodeA2, endirFix, m_pos2, -endirFix, true, true, out partE.b, out partE.c);

            if (NodeA1 == Vector3.zero)
            {
                CustomShowExtraInfo(true, Localization.Get("InvalidShape"), pos);
                isUpdate = true;
                return;
            }
            if (NodeB1 == Vector3.zero)
            {
                CustomShowExtraInfo(true, Localization.Get("InvalidShape"), pos);
                isUpdate = true;
                return;
            }
            if (NodeB2 == Vector3.zero)
            {
                CustomShowExtraInfo(true, Localization.Get("InvalidShape"), pos);
                isUpdate = true;
                return;
            }
            if (NodeB21 == Vector3.zero)
            {
                CustomShowExtraInfo(true, Localization.Get("InvalidShape"), pos);
                isUpdate = true;
                return;
            }
            if (NodeA2 == Vector3.zero)
            {
                CustomShowExtraInfo(true, Localization.Get("InvalidShape"), pos);
                isUpdate = true;
                return;
            }

            int m_nodeNum = 0;
            int partANum;
            if (Vector2.Distance(VectorUtils.XZ(VectorUtils.NormalizeXZ(m_pos1 - m_pos0)), VectorUtils.XZ(VectorUtils.NormalizeXZ(NodeA1 - m_pos1))) < 0.1f)
            {
                partANum = (int)(BezierDistance(partA, 0, 1) / (OptionUI.nodeGap * 8f));
            }
            else
            {
                CustomShowExtraInfo(true, Localization.Get("InvalidShape"), pos);
                isUpdate = true;
                return;
            }

            int partRoundNum = (int)(16f * (float)Math.PI * (float)radius * ((float)angle / 360f) / (OptionUI.nodeGap * 8f));
            int partENum = (int)(BezierDistance(partE, 0, 1) / (OptionUI.nodeGap * 8f));

            m_nodeNum += partANum + 1;
            m_nodeNum += partRoundNum + 1;
            m_nodeNum += partENum;
            ushort[] node = new ushort[m_nodeNum];
            ushort[] segment = new ushort[m_nodeNum];

            CustomShowExtraInfo(false, null, Vector3.zero);

            if (store)
            {
                if (rampMode == 0)
                {
                    return;
                }

                for (int i = 0; i < storeIndex; i++)
                {
                    if ((storedPos1[i] == m_pos1) && (storedPos2[i] == m_pos2) && (storedNode0[i] == m_node1) && (storedNode2[i] == m_node2))
                    {
                        isUpdate = true;
                        storeIndex = (byte)i;
                    }
                }

                if (storeIndex >= 8)
                {
                    return;
                }
                storedRampMode[storeIndex] = rampMode;

                storedPos0[storeIndex] = m_pos0;
                storedPos1[storeIndex] = m_pos1;
                storedPos2[storeIndex] = m_pos2;
                storedElevation[storeIndex] = m_elevation;
                storedRadius[storeIndex] = m_radius;
                storedNode0[storeIndex] = m_node1;
                storedNode2[storeIndex] = m_node2;
                storedNetInfo[storeIndex] = m_loacalNetInfo;
                return;
            }

            //smooth mode, recalculate bezier
            float totalDistance = 0;
            float heightDiff = m_pos1.y - m_pos2.y;
            float partALength = BezierDistance(partA, 0, 1);
            float partRoundLength = 16f * (float)Math.PI * (float)radius * ((float)angle / 360f);
            float partELength = BezierDistance(partE, 0, 1);

            //remove too short segment.
            if (partALength < 24)
                partANum = -1;

            if (partELength < 24)
                partENum = -1;

            if (OptionUI.isSmoothMode)
            {
                m_elevation = 0;
                totalDistance += partALength;
                totalDistance += partRoundLength;
                totalDistance += partELength;
            }

            if (partANum >= 0)
            {
                for (int i = 0; i <= partANum; i++)
                {
                    float p1 = (float)(i + 1) / (float)(partANum + 1);
                    float p2 = (float)(i) / (float)(partANum + 1);
                    if (!onlyShow)
                    {
                        if (!OptionUI.isSmoothMode)
                        {
                            CreateNode(out node[i], ref rand, netInfo, partA.Position(p1));
                            AdjustElevation(node[i], m_elevation);
                        }
                        else
                        {
                            var height = m_pos1.y - (heightDiff * (BezierDistance(partA, 0, p1)) / totalDistance);
                            var position = new Vector3(partA.Position(p1).x, height, partA.Position(p1).z);
                            CreateNodeDontFollowTerrain(out node[i], ref rand, netInfo, position);
                        }

                        if (i == 0)
                        {
                            var startDir = VectorUtils.NormalizeXZ(m_pos1 - m_pos0);
                            if (!OptionUI.isSmoothMode)
                            {
                                float tmpElevationMin;
                                float tmpElevationMax;
                                if (Singleton<NetManager>.instance.m_nodes.m_buffer[m_node1].m_flags.IsFlagSet(NetNode.Flags.Underground))
                                {
                                    tmpElevationMin = (-Singleton<NetManager>.instance.m_nodes.m_buffer[m_node1].m_elevation > m_elevation) ? m_elevation : -Singleton<NetManager>.instance.m_nodes.m_buffer[m_node1].m_elevation;
                                    tmpElevationMax = (-Singleton<NetManager>.instance.m_nodes.m_buffer[m_node1].m_elevation > m_elevation) ? -Singleton<NetManager>.instance.m_nodes.m_buffer[m_node1].m_elevation : m_elevation;
                                }
                                else
                                {
                                    tmpElevationMin = (Singleton<NetManager>.instance.m_nodes.m_buffer[m_node1].m_elevation > m_elevation) ? m_elevation : Singleton<NetManager>.instance.m_nodes.m_buffer[m_node1].m_elevation;
                                    tmpElevationMax = (Singleton<NetManager>.instance.m_nodes.m_buffer[m_node1].m_elevation > m_elevation) ? Singleton<NetManager>.instance.m_nodes.m_buffer[m_node1].m_elevation : m_elevation;
                                }
                                var tmpNetInfo = m_prefab.m_netAI.GetInfo(tmpElevationMin, tmpElevationMax, 5, false, false, false, false, ref errors);
                                if (tmpElevationMin < -8f && tmpElevationMax > -8f)
                                {
                                    if (Singleton<NetManager>.instance.m_nodes.m_buffer[m_node1].m_position.y > Singleton<NetManager>.instance.m_nodes.m_buffer[node[i]].m_position.y)
                                    {
                                        if (Singleton<NetManager>.instance.CreateSegment(out segment[i], ref rand, tmpNetInfo, node[i], m_node1, -VectorUtils.NormalizeXZ(partA.Tangent(p1)), startDir, Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, true))
                                            Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                                    }
                                    else
                                    {
                                        if (Singleton<NetManager>.instance.CreateSegment(out segment[i], ref rand, tmpNetInfo, m_node1, node[i], startDir, -VectorUtils.NormalizeXZ(partA.Tangent(p1)), Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                                            Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                                    }
                                }
                                else
                                {
                                    if (Singleton<NetManager>.instance.CreateSegment(out segment[i], ref rand, tmpNetInfo, m_node1, node[i], startDir, -VectorUtils.NormalizeXZ(partA.Tangent(p1)), Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                                        Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                                }
                            }
                            else
                            {
                                if (Singleton<NetManager>.instance.CreateSegment(out segment[i], ref rand, netInfo, m_node1, node[i], startDir, -VectorUtils.NormalizeXZ(partA.Tangent(p1)), Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                                    Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                            }
                        }
                        else
                        {
                            if (Singleton<NetManager>.instance.CreateSegment(out segment[i], ref rand, netInfo, node[i - 1], node[i], VectorUtils.NormalizeXZ(partA.Tangent(p2)), -VectorUtils.NormalizeXZ(partA.Tangent(p1)), Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                                Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                        }
                    }
                    else
                    {
                        if (i == 0)
                        {
                            currentMoney += netInfo.m_netAI.GetConstructionCost(m_pos1, partA.Position(p1), Singleton<NetManager>.instance.m_nodes.m_buffer[m_node1].m_elevation, m_elevation);
                        }
                        else
                        {
                            currentMoney += netInfo.m_netAI.GetConstructionCost(partA.Position(p2), partA.Position(p1), m_elevation, m_elevation);
                        }
                    }
                }
            }

            for (int i = 0; i <= partRoundNum; i++)
            {
                float p1 = (float)(i + 1) / (float)(partRoundNum + 1);
                float p2 = (float)(i) / (float)(partRoundNum + 1);
                GetRoundCurve(NodeA1, RoundCenter, NodeA2, (byte)(p1 * 255f), out Vector3 nodePos, out Vector3 nodeDir, true, out _);
                GetRoundCurve(NodeA1, RoundCenter, NodeA2, (byte)(p2 * 255f), out Vector3 preNodePos, out Vector3 preNodeDir, true, out _);
                if (!onlyShow)
                {
                    if (!OptionUI.isSmoothMode)
                    {
                        CreateNode(out node[i + partANum + 1], ref rand, netInfo, nodePos);
                        AdjustElevation(node[i + partANum + 1], m_elevation);
                    }
                    else
                    {
                        var height = m_pos1.y - (heightDiff * (partALength + (partRoundLength * p1)) / totalDistance);
                        var position = new Vector3(nodePos.x, height, nodePos.z);
                        CreateNodeDontFollowTerrain(out node[i + partANum + 1], ref rand, netInfo, position);
                    }

                    if (partANum < 0)
                    {
                        if (Singleton<NetManager>.instance.CreateSegment(out segment[i + partANum + 1], ref rand, netInfo, node[i + partANum], node[i + partANum + 1], preNodeDir, -nodeDir, Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                            Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                    }
                    else
                    {
                        if (Singleton<NetManager>.instance.CreateSegment(out segment[i + partANum + 1], ref rand, netInfo, m_node1, node[i + partANum + 1], preNodeDir, -nodeDir, Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                            Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                    }
                }
                else
                {
                    if ((partANum < 0) && (i == 0))
                    {
                        GetRoundCurve(NodeA1, RoundCenter, NodeA2, (byte)(255f / (partRoundNum + 1f)), out Vector3 nodePos1, out _, true, out _);
                        currentMoney += netInfo.m_netAI.GetConstructionCost(m_pos1, nodePos1, m_elevation, m_elevation);
                    }
                    else
                    {
                        currentMoney += netInfo.m_netAI.GetConstructionCost(preNodePos, nodePos, m_elevation, m_elevation);
                    }
                }
            }

            if (partENum > 0)
            {
                for (int i = 1; i <= partENum; i++)
                {
                    float p1 = (float)i / (float)(partENum + 1);
                    float p2 = (float)(i - 1) / (float)(partENum + 1);
                    if (!onlyShow)
                    {
                        if (!OptionUI.isSmoothMode)
                        {
                            CreateNode(out node[i + partANum + partRoundNum + 1], ref rand, netInfo, partE.Position(p1));
                            AdjustElevation(node[i + partANum + partRoundNum + 1], m_elevation);
                        }
                        else
                        {
                            var height = m_pos1.y - (heightDiff * (partALength + partRoundLength + BezierDistance(partE, 0, p1)) / totalDistance);
                            var position = new Vector3(partE.Position(p1).x, height, partE.Position(p1).z);
                            CreateNodeDontFollowTerrain(out node[i + partANum + partRoundNum + 1], ref rand, netInfo, position);
                        }
                        if (Singleton<NetManager>.instance.CreateSegment(out segment[i + partANum + partRoundNum + 1], ref rand, netInfo, node[i + partANum + partRoundNum], node[i + partANum + partRoundNum + 1], VectorUtils.NormalizeXZ(partE.Tangent(p2)), -VectorUtils.NormalizeXZ(partE.Tangent(p1)), Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                            Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                    }
                    else
                    {
                        currentMoney += netInfo.m_netAI.GetConstructionCost(partE.Position(p2), partE.Position(p1), m_elevation, m_elevation);
                    }
                }
            }

            if (!onlyShow)
            {
                Vector3 dir;
                if (partENum > 0)
                {
                    dir = VectorUtils.NormalizeXZ(partE.Tangent((float)partENum / (float)(partENum + 1)));
                }
                else
                {
                    dir = VectorUtils.NormalizeXZ(partE.Tangent((float)partRoundNum / (float)(partRoundNum + 1)));
                }


                if (!OptionUI.isSmoothMode)
                {
                    float tmpElevationMin;
                    float tmpElevationMax;
                    if (Singleton<NetManager>.instance.m_nodes.m_buffer[m_node2].m_flags.IsFlagSet(NetNode.Flags.Underground))
                    {
                        tmpElevationMin = (-Singleton<NetManager>.instance.m_nodes.m_buffer[m_node2].m_elevation > m_elevation) ? m_elevation : -Singleton<NetManager>.instance.m_nodes.m_buffer[m_node2].m_elevation;
                        tmpElevationMax = (-Singleton<NetManager>.instance.m_nodes.m_buffer[m_node2].m_elevation > m_elevation) ? -Singleton<NetManager>.instance.m_nodes.m_buffer[m_node2].m_elevation : m_elevation;
                    }
                    else
                    {
                        tmpElevationMin = (Singleton<NetManager>.instance.m_nodes.m_buffer[m_node2].m_elevation > m_elevation) ? m_elevation : Singleton<NetManager>.instance.m_nodes.m_buffer[m_node2].m_elevation;
                        tmpElevationMax = (Singleton<NetManager>.instance.m_nodes.m_buffer[m_node2].m_elevation > m_elevation) ? Singleton<NetManager>.instance.m_nodes.m_buffer[m_node2].m_elevation : m_elevation;
                    }
                    var tmpNetInfo = m_prefab.m_netAI.GetInfo(tmpElevationMin, tmpElevationMax, 5, false, false, false, false, ref errors);
                    if (tmpElevationMin < -8f && tmpElevationMax > -8f)
                    {
                        if (Singleton<NetManager>.instance.m_nodes.m_buffer[m_node2].m_position.y < Singleton<NetManager>.instance.m_nodes.m_buffer[node[partANum + partRoundNum + partENum + 1]].m_position.y)
                        {
                            if (Singleton<NetManager>.instance.CreateSegment(out _, ref rand, tmpNetInfo, m_node2, node[partANum + partRoundNum + partENum + 1], -VectorUtils.NormalizeXZ(endirFix), dir, Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, true))
                                Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                        }
                        else
                        {
                            if (Singleton<NetManager>.instance.CreateSegment(out _, ref rand, tmpNetInfo, node[partANum + partRoundNum + partENum + 1], m_node2, dir, -VectorUtils.NormalizeXZ(endirFix), Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                                Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                        }
                    }
                    else
                    {
                        if (Singleton<NetManager>.instance.CreateSegment(out _, ref rand, tmpNetInfo, node[partANum + partRoundNum + partENum + 1], m_node2, dir, -VectorUtils.NormalizeXZ(endirFix), Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                            Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                    }
                }
                else
                {
                    if (Singleton<NetManager>.instance.CreateSegment(out _, ref rand, netInfo, node[partANum + partRoundNum + partENum + 1], m_node2, dir, -VectorUtils.NormalizeXZ(endirFix), Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                        Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                }
            }
            else
            {
                if (partENum > 0)
                {
                    currentMoney += netInfo.m_netAI.GetConstructionCost(partE.Position((float)partENum / (float)(partENum + 1)), m_pos2, m_elevation, Singleton<NetManager>.instance.m_nodes.m_buffer[m_node2].m_elevation);
                }
                else
                {
                    GetRoundCurve(NodeA1, RoundCenter, NodeA2, (byte)(((float)partRoundNum / (float)(partRoundNum + 1)) * 255f), out Vector3 nodePos, out Vector3 nodeDir, true, out _);
                    currentMoney += netInfo.m_netAI.GetConstructionCost(nodePos, m_pos2, m_elevation, Singleton<NetManager>.instance.m_nodes.m_buffer[m_node2].m_elevation);
                }
            }

            if (onlyShow && !onlyShowMesh && !OptionUI.dontUseShaderPreview)
            {
                if (Vector2.Distance(VectorUtils.NormalizeXZ(m_pos1 - m_pos0), VectorUtils.NormalizeXZ(NodeA1 - m_pos1)) < 0.2f)
                {
                    Singleton<RenderManager>.instance.OverlayEffect.DrawBezier(cameraInfo, m_validColorInfo, partA, Mathf.Max(6f, m_loacalNetInfo.m_halfWidth * 2f), -100000f, -100000f, -1f, 1280f, renderLimits: false, alphaBlend: false);
                }
                Singleton<RenderManager>.instance.OverlayEffect.DrawBezier(cameraInfo, m_validColorInfo, partB, Mathf.Max(6f, m_loacalNetInfo.m_halfWidth * 2f), -100000f, -100000f, -1f, 1280f, renderLimits: false, alphaBlend: false);
                Singleton<RenderManager>.instance.OverlayEffect.DrawBezier(cameraInfo, m_validColorInfo, partC, Mathf.Max(6f, m_loacalNetInfo.m_halfWidth * 2f), -100000f, -100000f, -1f, 1280f, renderLimits: false, alphaBlend: false);
                Singleton<RenderManager>.instance.OverlayEffect.DrawBezier(cameraInfo, m_validColorInfo, partC1, Mathf.Max(6f, m_loacalNetInfo.m_halfWidth * 2f), -100000f, -100000f, -1f, 1280f, renderLimits: false, alphaBlend: false);
                Singleton<RenderManager>.instance.OverlayEffect.DrawBezier(cameraInfo, m_validColorInfo, partD, Mathf.Max(6f, m_loacalNetInfo.m_halfWidth * 2f), -100000f, -100000f, -1f, 1280f, renderLimits: false, alphaBlend: false);
                Singleton<RenderManager>.instance.OverlayEffect.DrawBezier(cameraInfo, m_validColorInfo, partE, Mathf.Max(6f, m_loacalNetInfo.m_halfWidth * 2f), -100000f, -100000f, -1f, 1280f, renderLimits: false, alphaBlend: false);
            }

            if (onlyShowMesh)
            {
                if (!OptionUI.isSmoothMode)
                {
                    RenderSegment(netInfo, NetSegment.Flags.None, FollowTerrain(partA.Position(0), m_elevation + 1f), FollowTerrain(partA.Position(1), m_elevation + 1f), VectorUtils.NormalizeXZ(partA.Tangent(0)), VectorUtils.NormalizeXZ(partA.Tangent(1)), true, true);
                    RenderSegment(netInfo, NetSegment.Flags.None, FollowTerrain(partB.Position(0), m_elevation + 1f), FollowTerrain(partB.Position(1), m_elevation + 1f), VectorUtils.NormalizeXZ(partB.Tangent(0)), VectorUtils.NormalizeXZ(partB.Tangent(1)), true, true);
                    RenderSegment(netInfo, NetSegment.Flags.None, FollowTerrain(partC.Position(0), m_elevation + 1f), FollowTerrain(partC.Position(1), m_elevation + 1f), VectorUtils.NormalizeXZ(partC.Tangent(0)), VectorUtils.NormalizeXZ(partC.Tangent(1)), true, true);
                    RenderSegment(netInfo, NetSegment.Flags.None, FollowTerrain(partC1.Position(0), m_elevation + 1f), FollowTerrain(partC1.Position(1), m_elevation + 1f), VectorUtils.NormalizeXZ(partC1.Tangent(0)), VectorUtils.NormalizeXZ(partC1.Tangent(1)), true, true);
                    RenderSegment(netInfo, NetSegment.Flags.None, FollowTerrain(partD.Position(0), m_elevation + 1f), FollowTerrain(partD.Position(1), m_elevation + 1f), VectorUtils.NormalizeXZ(partD.Tangent(0)), VectorUtils.NormalizeXZ(partD.Tangent(1)), true, true);
                    RenderSegment(netInfo, NetSegment.Flags.None, FollowTerrain(partE.Position(0), m_elevation + 1f), FollowTerrain(partE.Position(1), m_elevation + 1f), VectorUtils.NormalizeXZ(partE.Tangent(0)), VectorUtils.NormalizeXZ(partE.Tangent(1)), true, true);
                }
                else
                {
                    var heightA = m_pos1.y - (heightDiff * (BezierDistance(partA, 0, 1) / totalDistance));
                    var heightB = m_pos1.y - (heightDiff * ((BezierDistance(partA, 0, 1) + BezierDistance(partB, 0, 1)) / totalDistance));
                    var heightC = m_pos1.y - (heightDiff * ((BezierDistance(partA, 0, 1) + BezierDistance(partB, 0, 1) + BezierDistance(partC, 0, 1)) / totalDistance));
                    var heightC1 = m_pos1.y - (heightDiff * ((BezierDistance(partA, 0, 1) + BezierDistance(partB, 0, 1) + BezierDistance(partC, 0, 1) + BezierDistance(partC1, 0, 1)) / totalDistance));
                    var heightD = m_pos1.y - (heightDiff * ((BezierDistance(partA, 0, 1) + BezierDistance(partB, 0, 1) + BezierDistance(partC, 0, 1) + BezierDistance(partC1, 0, 1) + BezierDistance(partD, 0, 1)) / totalDistance));
                    RenderSegment(netInfo, NetSegment.Flags.None, partA.Position(0), SetHeight(partA.Position(1), heightA), VectorUtils.NormalizeXZ(partA.Tangent(0)), VectorUtils.NormalizeXZ(partA.Tangent(1)), true, true);
                    RenderSegment(netInfo, NetSegment.Flags.None, SetHeight(partB.Position(0), heightA), SetHeight(partB.Position(1), heightB), VectorUtils.NormalizeXZ(partB.Tangent(0)), VectorUtils.NormalizeXZ(partB.Tangent(1)), true, true);
                    RenderSegment(netInfo, NetSegment.Flags.None, SetHeight(partC.Position(0), heightB), SetHeight(partC.Position(1), heightC), VectorUtils.NormalizeXZ(partC.Tangent(0)), VectorUtils.NormalizeXZ(partC.Tangent(1)), true, true);
                    RenderSegment(netInfo, NetSegment.Flags.None, SetHeight(partC1.Position(0), heightC), SetHeight(partC1.Position(1), heightC1), VectorUtils.NormalizeXZ(partC1.Tangent(0)), VectorUtils.NormalizeXZ(partC1.Tangent(1)), true, true);
                    RenderSegment(netInfo, NetSegment.Flags.None, SetHeight(partD.Position(0), heightC1), SetHeight(partD.Position(1), heightD), VectorUtils.NormalizeXZ(partD.Tangent(0)), VectorUtils.NormalizeXZ(partD.Tangent(1)), true, true);
                    RenderSegment(netInfo, NetSegment.Flags.None, SetHeight(partE.Position(0), heightD), partE.Position(1), VectorUtils.NormalizeXZ(partE.Tangent(0)), VectorUtils.NormalizeXZ(partE.Tangent(1)), true, true);
                }
            }
        }

        public Vector3 SetHeight(Vector3 pos, float height)
        {
            return new Vector3(pos.x, height, pos.z);
        }
        public string CheckYRoadVaild(ushort node)
        {
            int segmentCount = 0;
            int directionCount = 1;
            for (int i = 0; i < 8; i++)
            {
                ushort segment = Singleton<NetManager>.instance.m_nodes.m_buffer[node].GetSegment(i);
                if (segment != 0)
                {
                    segmentCount++;
                    NetInfo info = Singleton<NetManager>.instance.m_segments.m_buffer[segment].Info;
                    for (int j = 0; j < info.m_lanes.Length; j++)
                    {
                        if (info.m_lanes[j].m_laneType.IsFlagSet(NetInfo.LaneType.Vehicle))
                        {
                            for (int k = 0; k < info.m_lanes.Length; k++)
                            {
                                if (info.m_lanes[k].m_laneType.IsFlagSet(NetInfo.LaneType.Vehicle))
                                {
                                    if (info.m_lanes[j].m_direction != info.m_lanes[k].m_direction)
                                    {
                                        directionCount = 2;
                                        break;
                                    }
                                }
                            }
                        }
                        if (directionCount == 2)
                            break;
                    }
                }
                if (segmentCount >= 2)
                {
                    break;
                }
            }

            if (segmentCount >= 2)
                return "False";

            if (directionCount == 2)
                return "Dual";

            return "Single";
        }
        public void BuildYRoad(bool onlyShow, bool showMesh)
        {
            //var roadLength = 32;
            var roadLength1 = 16;
            if (CheckYRoadVaild(node0) == "Single")
            {
                CustomShowExtraInfo(show: false, null, Vector3.zero);
                Vector3 startDir = -GetNodeDir(node0);
                startDir = new Vector3(startDir.x, 0, startDir.z).normalized;
                float totalWidth = Singleton<NetManager>.instance.m_nodes.m_buffer[node0].Info.m_halfWidth * 2 + leftAddWidth + rightAddWidth + roadSpace;
                float leftOffset = 0;
                float rightOffset = 0;
                if ((leftAddWidth == 0) && (rightAddWidth != 0))
                {
                    //left position is fixed, right position will be affect by roadspace
                    leftOffset = Singleton<NetManager>.instance.m_nodes.m_buffer[node0].Info.m_halfWidth + leftAddWidth - mainRoadWidth / 2f;
                    rightOffset = Singleton<NetManager>.instance.m_nodes.m_buffer[node0].Info.m_halfWidth + rightAddWidth + roadSpace - (totalWidth - mainRoadWidth - roadSpace) / 2f;
                }
                else if ((leftAddWidth != 0) && (rightAddWidth == 0))
                {
                    leftOffset = Singleton<NetManager>.instance.m_nodes.m_buffer[node0].Info.m_halfWidth + leftAddWidth + roadSpace - mainRoadWidth / 2f;
                    rightOffset = Singleton<NetManager>.instance.m_nodes.m_buffer[node0].Info.m_halfWidth + rightAddWidth - (totalWidth - mainRoadWidth - roadSpace) / 2f;
                }
                else
                {
                    leftOffset = Singleton<NetManager>.instance.m_nodes.m_buffer[node0].Info.m_halfWidth + leftAddWidth + roadSpace / 2f - mainRoadWidth / 2f;
                    rightOffset = Singleton<NetManager>.instance.m_nodes.m_buffer[node0].Info.m_halfWidth + rightAddWidth + roadSpace / 2f - (totalWidth - mainRoadWidth - roadSpace) / 2f;
                }
                Vector3 node1Pos = pos0 + roadLength * startDir + (new Vector3(-startDir.z, startDir.y, startDir.x)) * leftOffset;
                Vector3 node1PosExtra = pos0 + roadLength * startDir + (new Vector3(-startDir.z, startDir.y, startDir.x)) * (-roadSpace / 4f + 3f) * leftOffset;
                Vector3 node2Pos = pos0 + roadLength * startDir + (new Vector3(startDir.z, startDir.y, -startDir.x)) * rightOffset;
                Vector3 node2PosExtra = pos0 + roadLength * startDir + (new Vector3(startDir.z, startDir.y, -startDir.x)) * (-roadSpace / 4f + 3f) * rightOffset;
                Vector3 node3Pos = node1Pos + roadLength1 * startDir;
                Vector3 node4Pos = node2Pos + roadLength1 * startDir;
                var rand = new Randomizer(0u);
                var m_prefab = m_netInfo;
                ToolErrors errors = default(ToolErrors);
                var ele = Singleton<NetManager>.instance.m_nodes.m_buffer[node0].m_elevation;
                var netInfo = m_prefab.m_netAI.GetInfo(ele, ele, 5, false, false, false, false, ref errors);

                ushort localNode1 = 0;
                ushort localSegment1 = 0;
                if (!onlyShow)
                {
                    CreateNodeDontFollowTerrain(out localNode1, ref rand, netInfo, node1Pos);
                    AdjustElevationDontFollowTerrain(localNode1, ele);
                    if (Singleton<NetManager>.instance.CreateSegment(out localSegment1, ref rand, netInfo, node0, localNode1, VectorUtils.NormalizeXZ(node1PosExtra - pos0), -startDir, Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                        Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                }
                else
                {
                    if (showMesh)
                    {
                        RenderSegment(netInfo, Singleton<NetManager>.instance.m_segments.m_buffer[localSegment1].m_flags, FollowTerrain(pos0, ele + 1f), FollowTerrain(node1Pos, ele + 1f), VectorUtils.NormalizeXZ(node1PosExtra - pos0), startDir, true, true);
                    }
                    else
                    {
                        currentMoney += netInfo.m_netAI.GetConstructionCost(pos0, node1Pos, ele, ele);
                    }
                }

                ushort localNode2 = 0;
                ushort localSegment2 = 0;
                if (!onlyShow)
                {
                    CreateNodeDontFollowTerrain(out localNode2, ref rand, netInfo, node2Pos);
                    AdjustElevationDontFollowTerrain(localNode2, ele);
                    if (Singleton<NetManager>.instance.CreateSegment(out localSegment2, ref rand, netInfo, node0, localNode2, VectorUtils.NormalizeXZ(node2PosExtra - pos0), -startDir, Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                        Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                }
                else
                {
                    if (showMesh)
                    {
                        RenderSegment(netInfo, Singleton<NetManager>.instance.m_segments.m_buffer[localSegment2].m_flags, FollowTerrain(pos0, ele + 1f), FollowTerrain(node2Pos, ele + 1f), VectorUtils.NormalizeXZ(node2PosExtra - pos0), startDir, true, true);
                    }
                    else
                    {
                        currentMoney += netInfo.m_netAI.GetConstructionCost(pos0, node2Pos, ele, ele);
                    }
                }

                ushort localNode3 = 0;
                ushort localSegment3 = 0;
                if (!onlyShow)
                {
                    CreateNodeDontFollowTerrain(out localNode3, ref rand, netInfo, node3Pos);
                    AdjustElevationDontFollowTerrain(localNode3, ele);
                    if (Singleton<NetManager>.instance.CreateSegment(out localSegment3, ref rand, netInfo, localNode1, localNode3, startDir, -startDir, Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                        Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                }
                else
                {
                    if (showMesh)
                    {
                        RenderSegment(netInfo, Singleton<NetManager>.instance.m_segments.m_buffer[localSegment3].m_flags, FollowTerrain(node1Pos, ele + 1f), FollowTerrain(node3Pos, ele + 1f), startDir, startDir, true, true);
                    }
                    else
                    {
                        currentMoney += netInfo.m_netAI.GetConstructionCost(node1Pos, node3Pos, ele, ele);
                    }
                }

                ushort localNode4 = 0;
                ushort localSegment4 = 0;
                if (!onlyShow)
                {
                    CreateNodeDontFollowTerrain(out localNode4, ref rand, netInfo, node4Pos);
                    AdjustElevationDontFollowTerrain(localNode4, ele);
                    if (Singleton<NetManager>.instance.CreateSegment(out localSegment4, ref rand, netInfo, localNode2, localNode4, startDir, -startDir, Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                        Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                }
                else
                {
                    if (showMesh)
                    {
                        RenderSegment(netInfo, Singleton<NetManager>.instance.m_segments.m_buffer[localSegment4].m_flags, FollowTerrain(node2Pos, ele + 1f), FollowTerrain(node4Pos, ele + 1f), startDir, startDir, true, true);
                    }
                    else
                    {
                        currentMoney += netInfo.m_netAI.GetConstructionCost(node2Pos, node4Pos, ele, ele);
                    }
                }
            }
            else if (CheckYRoadVaild(node0) == "Dual")
            {
                CustomShowExtraInfo(show: false, null, Vector3.zero);
                Vector3 startDir = -GetNodeDir(node0);
                startDir = new Vector3(startDir.x, 0, startDir.z).normalized;
                float totalWidth = Singleton<NetManager>.instance.m_nodes.m_buffer[node0].Info.m_halfWidth * 2 + leftAddWidth + rightAddWidth + 2 * roadSpace;
                float leftOffset = Singleton<NetManager>.instance.m_nodes.m_buffer[node0].Info.m_halfWidth + leftAddWidth + roadSpace - (totalWidth - mainRoadWidth - 2 * roadSpace) / 4f;
                float rightOffset = Singleton<NetManager>.instance.m_nodes.m_buffer[node0].Info.m_halfWidth + rightAddWidth + roadSpace - (totalWidth - mainRoadWidth - 2 * roadSpace) / 4f;
                Vector3 node1Pos = pos0 + roadLength * startDir + (new Vector3(-startDir.z, startDir.y, startDir.x)) * leftOffset;
                Vector3 node1PosExtra = pos0 + roadLength * startDir + (new Vector3(-startDir.z, startDir.y, startDir.x)) * (-roadSpace / 4f + 3f) * leftOffset;
                Vector3 node2Pos = pos0 + roadLength * startDir + (new Vector3(startDir.z, startDir.y, -startDir.x)) * rightOffset;
                Vector3 node2PosExtra = pos0 + roadLength * startDir + (new Vector3(startDir.z, startDir.y, -startDir.x)) * (-roadSpace / 4f + 3f) * rightOffset;
                Vector3 node3Pos = node1Pos + roadLength1 * startDir;
                Vector3 node4Pos = node2Pos + roadLength1 * startDir;
                Vector3 node5Pos = pos0 + (roadLength + roadLength1) * startDir;
                var rand = new Randomizer(0u);
                var m_prefab = m_netInfo;
                ToolErrors errors = default(ToolErrors);
                var ele = Singleton<NetManager>.instance.m_nodes.m_buffer[node0].m_elevation;
                var netInfo = m_prefab.m_netAI.GetInfo(ele, ele, 5, false, false, false, false, ref errors);

                ushort localNode1 = 0;
                ushort localSegment1 = 0;
                if (!onlyShow)
                {
                    CreateNodeDontFollowTerrain(out localNode1, ref rand, netInfo, node1Pos);
                    AdjustElevationDontFollowTerrain(localNode1, ele);
                    if (Singleton<NetManager>.instance.CreateSegment(out localSegment1, ref rand, netInfo, node0, localNode1, VectorUtils.NormalizeXZ(node1PosExtra - pos0), -startDir, Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                        Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                }
                if (showMesh)
                {
                    RenderSegment(netInfo, Singleton<NetManager>.instance.m_segments.m_buffer[localSegment1].m_flags, FollowTerrain(pos0, ele + 1f), FollowTerrain(node1Pos, ele + 1f), VectorUtils.NormalizeXZ(node1PosExtra - pos0), startDir, true, true);
                }
                else
                {
                    currentMoney += netInfo.m_netAI.GetConstructionCost(pos0, node1Pos, ele, ele);
                }

                ushort localNode2 = 0;
                ushort localSegment2 = 0;
                if (!onlyShow)
                {
                    CreateNodeDontFollowTerrain(out localNode2, ref rand, netInfo, node2Pos);
                    AdjustElevationDontFollowTerrain(localNode2, ele);
                    if (Singleton<NetManager>.instance.CreateSegment(out localSegment2, ref rand, netInfo, node0, localNode2, VectorUtils.NormalizeXZ(node2PosExtra - pos0), -startDir, Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                        Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                }
                if (showMesh)
                {
                    RenderSegment(netInfo, Singleton<NetManager>.instance.m_segments.m_buffer[localSegment2].m_flags, FollowTerrain(pos0, ele + 1f), FollowTerrain(node2Pos, ele + 1f), VectorUtils.NormalizeXZ(node2PosExtra - pos0), startDir, true, true);
                }
                else
                {
                    currentMoney += netInfo.m_netAI.GetConstructionCost(pos0, node2Pos, ele, ele);
                }

                ushort localNode3 = 0;
                ushort localSegment3 = 0;
                if (!onlyShow)
                {
                    CreateNodeDontFollowTerrain(out localNode3, ref rand, netInfo, node3Pos);
                    AdjustElevationDontFollowTerrain(localNode3, ele);
                    if (Singleton<NetManager>.instance.CreateSegment(out localSegment3, ref rand, netInfo, localNode1, localNode3, startDir, -startDir, Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                    {
                        Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                    }
                }
                if (showMesh)
                {
                    RenderSegment(netInfo, Singleton<NetManager>.instance.m_segments.m_buffer[localSegment3].m_flags, FollowTerrain(node1Pos, ele + 1f), FollowTerrain(node3Pos, ele + 1f), startDir, startDir, true, true);
                }
                else
                {
                    currentMoney += netInfo.m_netAI.GetConstructionCost(node1Pos, node3Pos, ele, ele);
                }

                ushort localNode4 = 0;
                ushort localSegment4 = 0;
                if (!onlyShow)
                {
                    CreateNodeDontFollowTerrain(out localNode4, ref rand, netInfo, node4Pos);
                    AdjustElevationDontFollowTerrain(localNode4, ele);
                    if (Singleton<NetManager>.instance.CreateSegment(out localSegment4, ref rand, netInfo, localNode2, localNode4, startDir, -startDir, Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                        Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                }
                if (showMesh)
                {
                    RenderSegment(netInfo, Singleton<NetManager>.instance.m_segments.m_buffer[localSegment4].m_flags, FollowTerrain(node2Pos, ele + 1f), FollowTerrain(node4Pos, ele + 1f), startDir, startDir, true, true);
                }
                else
                {
                    currentMoney += netInfo.m_netAI.GetConstructionCost(node2Pos, node4Pos, ele, ele);
                }

                ushort localNode5 = 0;
                ushort localSegment5 = 0;
                if (!onlyShow)
                {
                    CreateNodeDontFollowTerrain(out localNode5, ref rand, netInfo, node5Pos);
                    AdjustElevationDontFollowTerrain(localNode5, ele);
                    if (Singleton<NetManager>.instance.CreateSegment(out localSegment5, ref rand, netInfo, node0, localNode5, startDir, -startDir, Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                        Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                }
                if (showMesh)
                {
                    RenderSegment(netInfo, Singleton<NetManager>.instance.m_segments.m_buffer[localSegment5].m_flags, FollowTerrain(pos0, ele + 1f), FollowTerrain(node5Pos, ele + 1f), startDir, startDir, true, true);
                }
                else
                {
                    currentMoney += netInfo.m_netAI.GetConstructionCost(pos0, node5Pos, ele, ele);
                }
            }
            else if (CheckYRoadVaild(node0) == "False")
            {
                //CustomShowExtraInfo(true, "This Node is available", pos);
            }
        }

        public static void RenderSegment(NetInfo info, NetSegment.Flags flags, Vector3 startPosition, Vector3 endPosition, Vector3 startDirection, Vector3 endDirection, bool smoothStart, bool smoothEnd)
        {
            if (info.m_segments != null)
            {
                NetManager instance = Singleton<NetManager>.instance;
                startPosition.y += 0.15f;
                endPosition.y += 0.15f;
                Vector3 vector = (startPosition + endPosition) * 0.5f;
                Quaternion identity = Quaternion.identity;
                Vector3 b = new Vector3(startDirection.z, 0f, 0f - startDirection.x) * info.m_halfWidth;
                Vector3 vector2 = startPosition - b;
                Vector3 vector3 = startPosition + b;
                b = new Vector3(endDirection.z, 0f, 0f - endDirection.x) * info.m_halfWidth;
                Vector3 vector4 = endPosition - b;
                Vector3 vector5 = endPosition + b;
                CustomNetSegment.CalculateMiddlePoints(vector2, startDirection, vector4, -endDirection, smoothStart, smoothEnd, out Vector3 middlePos, out Vector3 middlePos2);
                CustomNetSegment.CalculateMiddlePoints(vector3, startDirection, vector5, -endDirection, smoothStart, smoothEnd, out Vector3 middlePos3, out Vector3 middlePos4);
                float vScale = info.m_netAI.GetVScale();
                Matrix4x4 value = NetSegment.CalculateControlMatrix(vector2, middlePos, middlePos2, vector4, vector3, middlePos3, middlePos4, vector5, vector, vScale);
                Matrix4x4 value2 = NetSegment.CalculateControlMatrix(vector3, middlePos3, middlePos4, vector5, vector2, middlePos, middlePos2, vector4, vector, vScale);
                Vector4 value3 = new Vector4(0.5f / info.m_halfWidth, 1f / info.m_segmentLength, 1f, 1f);
                instance.m_materialBlock.Clear();
                instance.m_materialBlock.SetMatrix(instance.ID_LeftMatrix, value);
                instance.m_materialBlock.SetMatrix(instance.ID_RightMatrix, value2);
                instance.m_materialBlock.SetVector(instance.ID_MeshScale, value3);
                instance.m_materialBlock.SetVector(instance.ID_ObjectIndex, RenderManager.DefaultColorLocation);
                instance.m_materialBlock.SetColor(instance.ID_Color, info.m_color);
                if (info.m_requireSurfaceMaps)
                {
                    Singleton<TerrainManager>.instance.GetSurfaceMapping(vector, out Texture _SurfaceTexA, out Texture _SurfaceTexB, out Vector4 _SurfaceMapping);
                    if ((Object)_SurfaceTexA != (Object)null)
                    {
                        instance.m_materialBlock.SetTexture(instance.ID_SurfaceTexA, _SurfaceTexA);
                        instance.m_materialBlock.SetTexture(instance.ID_SurfaceTexB, _SurfaceTexB);
                        instance.m_materialBlock.SetVector(instance.ID_SurfaceMapping, _SurfaceMapping);
                    }
                }
                bool flag = false;
                for (int i = 0; i < info.m_segments.Length; i++)
                {
                    NetInfo.Segment segment = info.m_segments[i];
                    if (segment.CheckFlags(flags, out bool turnAround))
                    {
                        if (turnAround != flag)
                        {
                            value3.x = 0f - value3.x;
                            value3.y = 0f - value3.y;
                            instance.m_materialBlock.SetVector(instance.ID_MeshScale, value3);
                            flag = turnAround;
                        }
                        Singleton<ToolManager>.instance.m_drawCallData.m_defaultCalls++;
                        Graphics.DrawMesh(segment.m_segmentMesh, vector, identity, segment.m_segmentMaterial, segment.m_layer, null, 0, instance.m_materialBlock);
                    }
                }
            }
        }
    }
}