using AdvancedRoadTools.Util;
using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AdvancedRoadTools.UI
{
    public class OptionUI : MonoBehaviour
    {
        public static bool isMoneyNeeded = false;
        public static bool isSmoothMode = false;
        public static bool dontUseShaderPreview = false;
        static UISlider nodeGapSlider;
        public static int nodeGap = 10;
        public static void makeSettings(UIHelperBase helper)
        {
            // tabbing code is borrowed from RushHour mod
            // https://github.com/PropaneDragon/RushHour/blob/release/RushHour/Options/OptionHandler.cs
            LoadSetting();
            UIHelper actualHelper = helper as UIHelper;
            UIComponent container = actualHelper.self as UIComponent;

            UITabstrip tabStrip = container.AddUIComponent<UITabstrip>();
            tabStrip.relativePosition = new Vector3(0, 0);
            tabStrip.size = new Vector2(container.width - 20, 40);

            UITabContainer tabContainer = container.AddUIComponent<UITabContainer>();
            tabContainer.relativePosition = new Vector3(0, 40);
            tabContainer.size = new Vector2(container.width - 20, container.height - tabStrip.height - 20);
            tabStrip.tabPages = tabContainer;

            int tabIndex = 0;
            // Lane_ShortCut

            AddOptionTab(tabStrip, Localization.Get("ShortCut"));
            tabStrip.selectedIndex = tabIndex;

            UIPanel currentPanel = tabStrip.tabContainer.components[tabIndex] as UIPanel;
            currentPanel.autoLayout = true;
            currentPanel.autoLayoutDirection = LayoutDirection.Vertical;
            currentPanel.autoLayoutPadding.top = 5;
            currentPanel.autoLayoutPadding.left = 10;
            currentPanel.autoLayoutPadding.right = 10;

            UIHelper panelHelper = new UIHelper(currentPanel);

            var generalGroup = panelHelper.AddGroup(Localization.Get("ShortCut")) as UIHelper;
            var panel = generalGroup.self as UIPanel;

            panel.gameObject.AddComponent<OptionsKeymappingRoadTool>();

            var generalGroup1 = panelHelper.AddGroup(Localization.Get("OtherOption")) as UIHelper;
            generalGroup1.AddCheckbox(Localization.Get("NeedMoney"), isMoneyNeeded, (index) => isMoneyNeededEnable(index));
            generalGroup1.AddCheckbox(Localization.Get("NOSHADERPREVIEW"), dontUseShaderPreview, (index) => dontUseShaderPreviewEnable(index));
            nodeGapSlider = generalGroup1.AddSlider(Localization.Get("NODEGAP") + "(" + nodeGap.ToString() + "U)", 5, 10, 1, nodeGap, onNodeGapChanged) as UISlider;
            SaveSetting();
        }
        private static UIButton AddOptionTab(UITabstrip tabStrip, string caption)
        {
            UIButton tabButton = tabStrip.AddTab(caption);

            tabButton.normalBgSprite = "SubBarButtonBase";
            tabButton.disabledBgSprite = "SubBarButtonBaseDisabled";
            tabButton.focusedBgSprite = "SubBarButtonBaseFocused";
            tabButton.hoveredBgSprite = "SubBarButtonBaseHovered";
            tabButton.pressedBgSprite = "SubBarButtonBasePressed";

            tabButton.textPadding = new RectOffset(10, 10, 10, 10);
            tabButton.autoSize = true;
            tabButton.tooltip = caption;

            return tabButton;
        }

        public static void SaveSetting()
        {
            //save langugae
            FileStream fs = File.Create("AdvancedRoadTools_setting.txt");
            StreamWriter streamWriter = new StreamWriter(fs);
            streamWriter.WriteLine(isMoneyNeeded);
            streamWriter.WriteLine(dontUseShaderPreview);
            streamWriter.WriteLine(nodeGap);
            streamWriter.Flush();
            fs.Close();
        }

        public static void LoadSetting()
        {
            if (File.Exists("AdvancedRoadTools_setting.txt"))
            {
                FileStream fs = new FileStream("AdvancedRoadTools_setting.txt", FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                string strLine = sr.ReadLine();

                if (strLine == "False")
                {
                    isMoneyNeeded = false;
                }
                else
                {
                    isMoneyNeeded = true;
                }

                strLine = sr.ReadLine();

                if (strLine == "False")
                {
                    dontUseShaderPreview = false;
                }
                else
                {
                    dontUseShaderPreview = true;
                }

                strLine = sr.ReadLine();
                if (!int.TryParse(strLine, out nodeGap)) nodeGap = 10;
                sr.Close();
                fs.Close();
            }
        }
        public static void isMoneyNeededEnable(bool index)
        {
            isMoneyNeeded = index;
            SaveSetting();
        }

        public static void dontUseShaderPreviewEnable(bool index)
        {
            dontUseShaderPreview = index;
            SaveSetting();
        }

        private static void onNodeGapChanged(float newVal)
        {
            nodeGap = (int)newVal;
            nodeGapSlider.tooltip = newVal.ToString();
            nodeGapSlider.parent.Find<UILabel>("Label").text = Localization.Get("NODEGAP") + "(" + nodeGap.ToString() + "U)";
            SaveSetting();
        }
    }
}
