using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

namespace ShaderForge
{
    public class SFN_Blend : SF_Node_Arithmetic
    {


        public enum BlendMode
        {
            Darken变暗 = 0,
            Multiply正片叠底 = 1,
            ColorBurn颜色加深 = 2,
            LinearBurn颜色加深 = 3,
            //	DarkerColor = 4,

            Lighten变亮 = 5,
            Screen滤色 = 6,
            ColorDodge滤色 = 7,
            LinearDodge线性减淡 = 8,
            //	LighterColor = 9,

            Overlay叠加 = 10,
            //	SoftLight = 11,
            HardLight强光 = 12,
            VividLight亮光 = 13,
            LinearLight线性光 = 14,
            PinLight点光 = 15,
            HardMix实色混合 = 16,

            Difference差值 = 17,
            Exclusion排除 = 18,
            Subtract减去 = 19,
            Divide划分 = 20
        };


        const int maxEnum = 20;
        static int[] skipEnum = new int[] { 4, 9, 11 };

        public BlendMode currentBlendMode = BlendMode.Overlay叠加;
        public bool clamp = true;

        public SFN_Blend()
        {

        }

        public override void Initialize()
        {
            base.Initialize("Blend");
            base.UseLowerPropertyBox(true, true);
            base.showColor = true;
            base.texture.uniform = false;
            base.texture.CompCount = 3;
            base.node_height += 15;
            base.shaderGenMode = ShaderGenerationMode.Modal;

            connectors = new SF_NodeConnector[]{
                SF_NodeConnector.Create(this,"OUT","",ConType.cOutput,ValueType.VTvPending,false),
                SF_NodeConnector.Create(this,"SRC","Src",ConType.cInput,ValueType.VTvPending,false).SetRequired(true),
                SF_NodeConnector.Create(this,"DST","Dst",ConType.cInput,ValueType.VTvPending,false).SetRequired(true)
            };

            base.conGroup = ScriptableObject.CreateInstance<SFNCG_Arithmetic>().Initialize(connectors[0], connectors[1], connectors[2]);
        }

        public override bool IsUniformOutput()
        {
            return false;
        }


        public void StepBlendMode(int inc, bool registerUndo)
        {
            int nextBlendIndex = (int)currentBlendMode + inc;




        restart:
            foreach (int i in skipEnum)
            {
                if (nextBlendIndex == i)
                {
                    nextBlendIndex += inc;
                    goto restart; // Watch out for raptors
                }
            }


            if (nextBlendIndex == -1)
            {
                BlendMode nextBlendMode = (BlendMode)maxEnum;
                if (registerUndo)
                {
                    UndoRecord("switch blend mode to " + nextBlendMode.ToString());
                }
                currentBlendMode = nextBlendMode;
                return;
            }
            else if (nextBlendIndex > maxEnum)
            {
                BlendMode nextBlendMode = (BlendMode)0;
                if (registerUndo)
                {
                    UndoRecord("switch blend mode to " + nextBlendMode.ToString());
                }
                currentBlendMode = nextBlendMode;
                return;
            }

            if (registerUndo)
            {
                UndoRecord("switch blend mode to " + (BlendMode)nextBlendIndex);
            }

            currentBlendMode = (BlendMode)nextBlendIndex;

        }





        public override void RefreshValue()
        {
            RefreshValue(1, 2);
        }

        public override void DrawLowerPropertyBox()
        {
            GUI.color = Color.white;
            EditorGUI.BeginChangeCheck();
            Rect r = lowerRect;
            r.height = 17;

            currentBlendMode = (BlendMode)UndoableEnumPopup(r, currentBlendMode, "switch blend mode");

            r = r.MovedDown();
            r.width -= r.height * 2 + 8;
            UndoableToggle(r, ref clamp, "Clamp", "blend node clamp", SF_Styles.ToggleDiscrete);
            r.width = lowerRect.width;
            r = r.MovedRight();
            r.width = r.height + 4;
            r = r.MovedLeft(2);
            if (GUI.Button(r, "\u25B2"))
            {
                StepBlendMode(-1, registerUndo: true);
            }
            r = r.MovedRight();
            if (GUI.Button(r, "\u25BC"))
            {
                StepBlendMode(1, registerUndo: true);
            }


            if (EditorGUI.EndChangeCheck())
                OnUpdateNode();
        }

        public override string SerializeSpecialData()
        {
            string s = "";
            s += "blmd:" + (int)currentBlendMode + ",";
            s += "clmp:" + clamp.ToString();
            return s;
        }



        public override void DeserializeSpecialData(string key, string value)
        {
            switch (key)
            {
                case "blmd":
                    currentBlendMode = (BlendMode)int.Parse(value);
                    break;
                case "clmp":
                    clamp = bool.Parse(value);
                    break;
            }
        }


        public override float EvalCPU(int c)
        {

            float a = GetInputData("SRC", c);
            float b = GetInputData("DST", c);

            float blended = Blend(a, b);

            if (clamp)
                blended = Mathf.Clamp01(blended);


            return blended;
        }


        public override string Evaluate(OutChannel channel = OutChannel.All)
        {

            // Update eval count here

            string a = GetConnectorByStringID("SRC").TryEvaluate();
            string b = GetConnectorByStringID("DST").TryEvaluate();




            string blend = Blend(a, b, currentBlendMode);

            if (clamp)
            {
                return "saturate(" + blend + ")";
            }

            return blend;
        }


        public void UpdateUsageCount()
        {

            SF_NodeConnector src = GetConnectorByStringID("SRC");
            SF_NodeConnector dst = GetConnectorByStringID("DST");



            if (currentBlendMode == BlendMode.Overlay叠加)
            {
                src.usageCount = 2;
                dst.usageCount = 3;
                return;
            }

            if (currentBlendMode == BlendMode.HardLight强光 ||
               currentBlendMode == BlendMode.VividLight亮光 ||
               currentBlendMode == BlendMode.LinearLight线性光 ||
               currentBlendMode == BlendMode.PinLight点光)
            {

                src.usageCount = 3;
                dst.usageCount = 2;
                return;
            }



            src.usageCount = 1;
            dst.usageCount = 1;

        }



        public override string[] GetModalModes()
        {
            return Enum.GetNames(typeof(BlendMode));
        }

        public override string GetCurrentModalMode()
        {
            return currentBlendMode.ToString();
        }

        public override string[] GetBlitOutputLines(string mode)
        {
            string s = Blend("_src", "_dst", (BlendMode)Enum.Parse(typeof(BlendMode), mode));
            return new string[] { s };
        }



        // lerp( 2.0*a*b, 1.0-(1.0-2.0*(a-0.5))*(1.0-b), round(a) ) 


        public string Blend(string a, string b, BlendMode mode)
        {
            switch (mode)
            {
                case BlendMode.Darken变暗:
                    return "min(" + a + "," + b + ")";
                case BlendMode.Multiply正片叠底:
                    return "(" + a + "*" + b + ")";
                case BlendMode.ColorBurn颜色加深:
                    return "(1.0-((1.0-" + b + ")/" + a + "))";
                case BlendMode.LinearBurn颜色加深:
                    return "(" + a + "+" + b + "-1.0)";
                case BlendMode.Lighten变亮:
                    return "max(" + a + "," + b + ")";
                case BlendMode.Screen滤色:
                    return "(1.0-(1.0-" + a + ")*(1.0-" + b + "))";
                case BlendMode.ColorDodge滤色:
                    return "(" + b + "/(1.0-" + a + "))";
                case BlendMode.LinearDodge线性减淡:
                    return "(" + a + "+" + b + ")";
                case BlendMode.Overlay叠加:
                    return "( " + b + " > 0.5 ? (1.0-(1.0-2.0*(" + b + "-0.5))*(1.0-" + a + ")) : (2.0*" + b + "*" + a + ") )";
                case BlendMode.HardLight强光:
                    return "(" + a + " > 0.5 ?  (1.0-(1.0-2.0*(" + a + "-0.5))*(1.0-" + b + ")) : (2.0*" + a + "*" + b + ")) ";
                case BlendMode.VividLight亮光:
                    return "( " + a + " > 0.5 ? (" + b + "/((1.0-" + a + ")*2.0)) : (1.0-(((1.0-" + b + ")*0.5)/" + a + ")))";
                case BlendMode.LinearLight线性光:
                    return "( " + a + " > 0.5 ? (" + b + " + 2.0*" + a + " -1.0) : (" + b + " + 2.0*(" + a + "-0.5)))";
                case BlendMode.PinLight点光:
                    return "( " + a + " > 0.5 ? max(" + b + ",2.0*(" + a + "-0.5)) : min(" + b + ",2.0*" + a + ") )";
                case BlendMode.HardMix实色混合:
                    return "round( 0.5*(" + a + " + " + b + "))";
                case BlendMode.Difference差值:
                    return "abs(" + a + "-" + b + ")";
                case BlendMode.Exclusion排除:
                    return "(0.5 - 2.0*(" + a + "-0.5)*(" + b + "-0.5))";
                case BlendMode.Subtract减去:
                    return "(" + b + "-" + a + ")";
                case BlendMode.Divide划分:
                    return "(" + b + "/" + a + ")";
            }
            return "0";
        }


        public float Blend(float a, float b)
        {
            switch (currentBlendMode)
            {
                case BlendMode.Darken变暗:
                    return Mathf.Min(a, b);
                case BlendMode.Multiply正片叠底:
                    return a * b;
                case BlendMode.ColorBurn颜色加深:
                    return 1f - ((1f - b) / a);
                case BlendMode.LinearBurn颜色加深:
                    return a + b - 1f;
                case BlendMode.Lighten变亮:
                    return Mathf.Max(a, b);
                case BlendMode.Screen滤色:
                    return 1f - (1f - a) * (1f - b);
                case BlendMode.ColorDodge滤色:
                    return b / (1f - a);
                case BlendMode.LinearDodge线性减淡:
                    return a + b;
                case BlendMode.Overlay叠加:
                    return b > 0.5f ? 1f - (1f - 2f * (b - 0.5f)) * (1f - a) : 2f * a * b;
                case BlendMode.HardLight强光:
                    return a > 0.5f ? 1f - (1f - 2f * (a - 0.5f)) * (1f - b) : 2f * a * b; ;
                case BlendMode.VividLight亮光:
                    return a > 0.5f ? b / ((1f - a) * 2f) : 1f - (((1f - b) * 0.5f) / a);
                case BlendMode.LinearLight线性光:
                    return a > 0.5f ? b + 2f * (a - 0.5f) : b + 2f * a - 1f;
                case BlendMode.PinLight点光:
                    return a > 0.5f ? Mathf.Max(b, 2f * (a - 0.5f)) : Mathf.Min(b, 2f * a);
                case BlendMode.HardMix实色混合:
                    return Mathf.Round((a + b) * 0.5f);
                case BlendMode.Difference差值:
                    return Mathf.Abs(a - b);
                case BlendMode.Exclusion排除:
                    return 0.5f - 2f * (a - 0.5f) * (b - 0.5f);
                case BlendMode.Subtract减去:
                    return b - a;
                case BlendMode.Divide划分:
                    return b / a;
            }
            return 0f;
        }


    }
}
