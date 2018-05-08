using UnityEngine;

namespace UnityEditorHelper
{
    public class EditorStlyeUtil
    {
        private static GUILayoutOption midButtonHeight;
        public static GUILayoutOption MidButtonHeight
        {
            get
            {
                if(midButtonHeight == null)
                    midButtonHeight = GUILayout.Height(25);
                return midButtonHeight;
            }
        }

        private static GUILayoutOption bigButtonHeight;

        public static GUILayoutOption BigButtonHeight
        {
            get
            {
                if (bigButtonHeight == null)
                    bigButtonHeight = GUILayout.Height(30);
                return bigButtonHeight;
            }
        }
    }
}