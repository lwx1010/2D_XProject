using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AL
{
    public class MyColor
    {
        public static Color white 
        { 
            get 
            { 
                return new Color(196f / 255, 212f / 255, 209f / 255);
            }
        }

        public static Color green
        {
            get
            {
                return Color.green;
            }
        }

        public static Color blue
        {
            get
            {
                return new Color(8f / 255, 98f / 255, 196f / 255);
            }
        }

        public static Color sky_blue
        {
            get
            {
                return new Color(171f / 255, 242f / 255, 1f);
            }
        }


        public static Color purple
        {
            get
            {
                return new Color(199f / 255, 0f, 242f / 255);
            }
        }

        public static Color orange
        {
            get
            {
                return new Color(1f, 161f / 255, 0f);
            }
        }

        public static Color red
        {
            get
            {
                return new Color(1f, 35f / 255, 35f / 255);
            }
        }

        public static Color black
        {
            get
            {
                return Color.black;
            }
        }

        public static Color gray
        {
            get
            {
                return Color.gray;
            }
        }

        public static Color yellow
        {
            get
            {
                return new Color(237f / 255, 252f / 255, 0f);
            }
        }

        public static Color dark_red
        {
            get
            {
                return new Color(153f / 255, 0f, 0f);
            }
        }

        public static Color dark_golden
        {
            get
            {
                return new Color(164f / 255, 123f / 255, 2f / 255);
            }
        }

        public static Color setting_color
        {
            get
            {
                return new Color(164f / 255, 123f / 255, 2f / 255);
            }
        }

        public static Color light_green
        {
            get
            {
                return new Color(41f / 255, 144f / 255, 1f / 255);
            }
        }

        public static Color brown
        {
            get
            {
                return new Color(110f / 255, 74f / 255, 38f / 255);
            }
        }

        public static Color outline_color_0
        {
            get
            {
                return new Color(52f / 255, 120f / 255, 10f / 255);
            }
        }

        public static Color outline_color_1
        {
            get
            {
                return new Color(190f / 255, 32f / 255, 30f / 255);
            }
        }

        public static Color outline_blue
        {
            get 
            {
                return new Color(245f / 255, 241f / 255, 204f / 255);
            }
        }

        public static Color outline_green
        {
            get
            {
                return new Color(245f / 255, 241f / 255, 204f / 255);
            }
        }

        public static Color outline_purple
        {
            get
            {
                return new Color(245f / 255, 241f / 255, 204f / 255);
            }
        }

        public static Color outline_orange
        {
            get
            {
                return new Color(245f / 255, 241f / 255, 204f / 255);
            }
        }

        public static Color outline_golden
        {
            get
            {
                return new Color(152f / 255, 50f / 255, 1f / 255);
            }
        }

        public static List<Color> outline_colors = new List<Color>(){white, outline_blue, outline_green, outline_purple, outline_orange, outline_golden };


        public static Color toggle_green
        {
            get
            {
                return new Color(139f / 255, 255f / 255, 0f / 255);
            }
        }

        public static Color toggle_gray
        {
            get
            {
                return new Color(187f / 255, 187f / 255, 187f / 255);
            }
        }

        public static Color lineup_di
        {
            get
            {
                return new Color(254f / 255, 233f / 255, 9f / 255);
            }
        }

        public static Color lineup_shui
        {
            get
            {
                return new Color(0 / 255, 115f / 255, 221f / 255);
            }
        }

        public static Color lineup_huo
        {
            get
            {
                return new Color(225f / 255, 80f / 255, 3f / 255);
            }
        }

        public static Color lineup_feng
        {
            get
            {
                return new Color(2f / 255, 144f / 255, 53f / 255);
            }
        }

        public static List<Color> lineup_colors = new List<Color>() {white, lineup_di, lineup_shui, lineup_huo, lineup_feng};
        public static List<Color> lineup_darkbg_colors = new List<Color>() { white, CommonTools.ConvertStringToColor("#fce700ff"), CommonTools.ConvertStringToColor("#0bd7fdff"), CommonTools.ConvertStringToColor("#ff2323ff"), CommonTools.ConvertStringToColor("#58c715ff") };
#region 16进制颜色

        public static Color scene_npc_color
        {
            get 
            {
                return CommonTools.ConvertStringToColor("#ffea09ff");
            }
        }

        public static Color scene_npc_outline_color
        {
            get
            {
                return CommonTools.ConvertStringToColor("#983201ff");
            }
        }
#endregion
    }
}

