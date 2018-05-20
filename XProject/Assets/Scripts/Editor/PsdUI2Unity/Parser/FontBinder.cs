using System;
using UnityEngine;
using UnityEngine.UI;

namespace EditorTool.PsdExport
{
    public class FontBinder : ABinder
    {
        public override void StartBinding(GameObject gObj, string args, string layerName)
        {
#if NGUI
            UILabel lab = gObj.GetComponent<UILabel>();
            if (lab == null) return;

            string[] argArr = args.Split(',');
            try
            {
                lab.fontSize = Convert.ToInt32(argArr[0]); //fontSize
            }
            catch (Exception)
            {
                Debug.LogError(layerName);
                throw;
            }
#elif UGUI
            Text text = gObj.GetComponent<Text>();
            if (text == null) return;

            string[] argArr = args.Split(',');
            try
            {
                text.fontSize = Convert.ToInt32(argArr[0]); //fontSize
            }
            catch (Exception)
            {
                Debug.LogError(layerName);
                throw;
            }
#endif
        }



    }
}