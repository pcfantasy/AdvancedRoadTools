using AdvancedRoadTools.Util;
using ColossalFramework.UI;
using UnityEngine;

namespace AdvancedRoadTools.UI
{
    public class SmoothButton : UIButton
    {
        public override void Start()
        {
            name = "SmoothButton";
            //text = "O";
            relativePosition = new Vector3((Loader.parentGuiView.fixedWidth / 2f - 570f), (Loader.parentGuiView.fixedHeight / 2f + 370f));
            atlas = SpriteUtilities.GetAtlas(Loader.m_atlasName);
            if (OptionUI.isSmoothMode)
                normalBgSprite = "Smooth_S";
            else
                normalBgSprite = "Smooth";
            size = new Vector2(30f, 30f);
            zOrder = 11;
            eventClick += delegate (UIComponent component, UIMouseEventParameter eventParam)
            {
                if (OptionUI.isSmoothMode)
                {
                    OptionUI.isSmoothMode = false;
                    normalBgSprite = "Smooth";
                }
                else
                {
                    OptionUI.isSmoothMode = true;
                    normalBgSprite = "Smooth_S";
                }
            };
        }
        public void OnGUI()
        {
            //base.Update();
            if (!isVisible)
            {
                ToolBase currentTool = ToolsModifierControl.GetCurrentTool<ToolBase>();
                if ((currentTool != null) && (currentTool is NetTool))
                {
                    //DebugLog.LogToFileOnly("try show");
                    Show();
                }
            }
            else
            {
                ToolBase currentTool = ToolsModifierControl.GetCurrentTool<ToolBase>();
                if (!((currentTool != null) && (currentTool is NetTool)))
                {
                    Hide();
                }
            }
        }
    }
}
