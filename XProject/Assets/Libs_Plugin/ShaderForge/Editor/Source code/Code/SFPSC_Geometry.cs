using UnityEngine;
using UnityEditor;
using System.Collections;

namespace ShaderForge
{


    [System.Serializable]
    public class SFPSC_Geometry : SFPS_Category
    {

        public enum VertexPositioning { LocalSpace, ClipSpace, Billboard };
        public string[] strVertexPositioning = new string[] { "局部空间", "裁剪空间", "公告牌" };
        public enum NormalQuality { Interpolated, Normalized };
        public string[] strNormalQuality = new string[] { "内插", "归一化" };
        public enum NormalSpace { Tangent, Object, World };
        public string[] strNormalSpace = new string[] { "切线", "对象", "世界" };
        public enum TessellationMode { Regular, EdgeLength/*, EdgeLengthCulled*/};
        public string[] tessModeStr = new string[] { "整齐", "边缘长度为基础"/*, "Edge length based with frustrum culling"*/};
        public enum VertexOffsetMode { Relative, Absolute }
        public string[] vertexOffsetModeStr = new string[] { "相对", "绝对" };
        public enum OutlineMode { FromOrigin, VertexNormals, VertexColors };
        public string[] outlineModeStr = new string[] { "从源点", "顶点法线", "法线颜色" };
        public enum CullMode { BackfaceCulling, FrontfaceCulling, DoubleSided };
        public static string[] strCullMode = new string[] { "Back", "Front", "Off" };

        public VertexPositioning vertexPositioning = VertexPositioning.LocalSpace;
        public NormalQuality normalQuality = NormalQuality.Normalized;
        public NormalSpace normalSpace = NormalSpace.Tangent;
        public VertexOffsetMode vertexOffsetMode = VertexOffsetMode.Relative;
        public bool showPixelSnap = false;
        public bool highQualityScreenCoords = true;
        public TessellationMode tessellationMode = TessellationMode.Regular;
        public OutlineMode outlineMode = OutlineMode.VertexNormals;
        public CullMode cullMode = CullMode.BackfaceCulling;



        public override string Serialize()
        {
            string s = "";
            s += Serialize("vtps", ((int)vertexPositioning).ToString());
            s += Serialize("hqsc", highQualityScreenCoords.ToString());
            s += Serialize("nrmq", ((int)normalQuality).ToString());
            s += Serialize("nrsp", ((int)normalSpace).ToString());
            s += Serialize("vomd", ((int)vertexOffsetMode).ToString());
            s += Serialize("spxs", showPixelSnap.ToString());
            s += Serialize("tesm", ((int)tessellationMode).ToString());
            s += Serialize("olmd", ((int)outlineMode).ToString());
            s += Serialize("culm", ((int)cullMode).ToString());
            return s;
        }

        public override void Deserialize(string key, string value)
        {

            switch (key)
            {
                case "vtps":
                    vertexPositioning = (VertexPositioning)int.Parse(value);
                    break;
                case "nrmq":
                    normalQuality = (NormalQuality)int.Parse(value);
                    break;
                case "nrsp":
                    normalSpace = (NormalSpace)int.Parse(value);
                    break;
                case "vomd":
                    vertexOffsetMode = (VertexOffsetMode)int.Parse(value);
                    break;
                case "hqsc":
                    highQualityScreenCoords = bool.Parse(value);
                    break;
                case "spxs":
                    showPixelSnap = bool.Parse(value);
                    break;
                case "tesm":
                    tessellationMode = (TessellationMode)int.Parse(value);
                    break;
                case "olmd":
                    outlineMode = (OutlineMode)int.Parse(value);
                    break;
                case "culm":
                    cullMode = (CullMode)int.Parse(value);
                    break;
            }

        }


        public override float DrawInner(ref Rect r)
        {

            float prevYpos = r.y;
            r.y = 0;


            r.xMin += 20;
            r.y += 20;


            cullMode = (CullMode)UndoableLabeledEnumPopup(r, "面剔除", cullMode, "face culling");
            r.y += 20;

            GUI.enabled = ps.catLighting.renderPath == SFPSC_Lighting.RenderPath.Forward;
            normalQuality = (NormalQuality)UndoableContentScaledToolbar(r, "法线质量", (int)normalQuality, strNormalQuality, "normal quality");
            GUI.enabled = true;
            r.y += 20;

            vertexPositioning = (VertexPositioning)UndoableContentScaledToolbar(r, "顶点定位", (int)vertexPositioning, strVertexPositioning, "vertex positioning");
            r.y += 20;

            GUI.enabled = ps.mOut.normal.IsConnectedEnabledAndAvailable();
            normalSpace = (NormalSpace)UndoableContentScaledToolbar(r, "法线空间", (int)normalSpace, strNormalSpace, "normal space");
            GUI.enabled = true;
            r.y += 20;

            vertexOffsetMode = (VertexOffsetMode)UndoableContentScaledToolbar(r, "顶点偏移模式", (int)vertexOffsetMode, vertexOffsetModeStr, "vertex offset mode");
            r.y += 20;

            GUI.enabled = ps.HasTessellation();
            tessellationMode = (TessellationMode)UndoableLabeledEnumPopupNamed(r, "细分曲面模式", tessellationMode, tessModeStr, "tessellation mode");
            GUI.enabled = true;
            r.y += 20;

            GUI.enabled = ps.HasOutline();
            outlineMode = (OutlineMode)UndoableLabeledEnumPopupNamed(r, "描边拉伸方向", outlineMode, outlineModeStr, "outline mode");
            GUI.enabled = true;
            r.y += 20;

            highQualityScreenCoords = UndoableToggle(r, highQualityScreenCoords, "每像素屏幕坐标（开启：像素 关闭：顶点）", "per-pixel screen coordinates", null);
            r.y += 20;

            showPixelSnap = UndoableToggle(r, showPixelSnap, "在材质中显示二维切片 Pixel snap（像素捕捉）选项", "show pixel snap", null);
            r.y += 20;

            r.y += prevYpos;

            return (int)r.yMax;
        }



        // TODO: Double sided support
        public string GetNormalSign()
        {
            if (cullMode == CullMode.BackfaceCulling)
                return "";
            if (cullMode == CullMode.FrontfaceCulling)
                return "-";
            //if( cullMode == CullMode.DoubleSided )
            return "";
        }

        public bool UseCulling()
        {
            return (cullMode != CullMode.BackfaceCulling);
        }
        public string GetCullString()
        {
            return "Cull " + strCullMode[(int)cullMode];
        }
        public bool IsDoubleSided()
        {
            return (cullMode == CullMode.DoubleSided);
        }




    }
}