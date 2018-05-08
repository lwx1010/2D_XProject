using LuaFramework;
using UnityEngine;
using UnityEngine.UI;

namespace EditorTool.PsdExport
{
    public class ButtonBinder:ABinder
    {
        public override void StartBinding(GameObject gObj, string args, string layerName)
        {
            Button button = LayerWordBinder.swapComponent<Button>(gObj);
            Image imgBtn = LayerWordBinder.findChildComponent<Image>(gObj, "background" , "bg");
            button.targetGraphic = imgBtn;
            LayerWordBinder.swapComponent<UButtonScale>(gObj);
        }

    }
}