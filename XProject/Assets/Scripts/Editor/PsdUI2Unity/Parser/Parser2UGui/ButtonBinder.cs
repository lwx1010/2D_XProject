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
            Image imgBtn = LayerWordBinder.findChildComponent<Image>(gObj, "background", "bg");
            button.targetGraphic = imgBtn;
            button.transition = Selectable.Transition.None;
            if (imgBtn != null)
            {
                Vector3 relativePos = imgBtn.transform.localPosition;
                RectTransform rect = button.transform as RectTransform;
                rect.localPosition = relativePos;
                rect.sizeDelta = (imgBtn.transform as RectTransform).sizeDelta;
                Transform[] transCaches = gObj.GetComponentsInChildren<Transform>(true);
                foreach (var trans in transCaches)
                {
                    if (trans == button.transform) continue;
                    if (trans.localPosition != Vector3.zero)
                        trans.localPosition = trans.localPosition - relativePos;
                }
            }
            LayerWordBinder.swapComponent<UButtonScale>(gObj);
        }

    }
}