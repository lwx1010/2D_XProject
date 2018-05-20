using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace EditorTool.PsdExport
{
    public class RotationBinder : ABinder
    {
        public override void StartBinding(GameObject gObj, string args, string layerName)
        {
            RectTransform rectTrans = gObj.transform as RectTransform;
            if (rectTrans != null)
            {
                string[] tmps = args.Split(',');
                float xR = 0;
                float yR = 0;
                float zR = 0;
                foreach (var tmp in tmps)
                {
                    var param = tmp.Split('-');
                    if (param[0].ToLower() == "x")
                    {
                        xR = Convert.ToSingle(param[1]);
                    }
                    else if (param[0].ToLower() == "y")
                    {
                        yR = Convert.ToSingle(param[1]);
                    }
                    else if (param[0].ToLower() == "z")
                    {
                        zR = Convert.ToSingle(param[1]);
                    }
                }
                rectTrans.localRotation = Quaternion.Euler(xR, yR, zR);
            }
            LayerWordBinder.swapComponent<UButtonScale>(gObj);
        }
    }
}
