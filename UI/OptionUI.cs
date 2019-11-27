using AdvancedRoadTools.Util;
using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AdvancedRoadTools.UI
{
    public class OptionUI : MonoBehaviour
    {
        public static bool isMoneyNeeded = false;
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
            SaveSetting();

            // Function_ShortCut
            /*++tabIndex;

            AddOptionTab(tabStrip, "ShortCut");
            tabStrip.selectedIndex = tabIndex;

            currentPanel = tabStrip.tabContainer.components[tabIndex] as UIPanel;
            currentPanel.autoLayout = true;
            currentPanel.autoLayoutDirection = LayoutDirection.Vertical;
            currentPanel.autoLayoutPadding.top = 5;
            currentPanel.autoLayoutPadding.left = 10;
            currentPanel.autoLayoutPadding.right = 10;

            panelHelper = new UIHelper(currentPanel);

            generalGroup = panelHelper.AddGroup("Beta function") as UIHelper;
            panel = generalGroup.self as UIPanel;

            var generalGroup1 = panelHelper.AddGroup("Beta function") as UIHelper;
            generalGroup1.AddCheckbox("Allow road tools to make road more like a circle(may impact performance)", isMoreRound, (index) => isMoreRoundEnable(index));
            SaveSetting();*/
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
                sr.Close();
                fs.Close();
            }
        }
        public static void isMoneyNeededEnable(bool index)
        {
            isMoneyNeeded = index;
            SaveSetting();
        }
    }
}
