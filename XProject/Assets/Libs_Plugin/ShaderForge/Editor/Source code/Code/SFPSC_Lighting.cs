using UnityEngine;
using UnityEditor;
using System.Collections;

namespace ShaderForge
{


    [System.Serializable]
    public class SFPSC_Lighting : SFPS_Category
    {

        public RenderPath renderPath = RenderPath.Forward;
        public LightPrecision lightPrecision = LightPrecision.Fragment;
        public LightMode lightMode = LightMode.BlinnPhong;
        public SpecularMode specularMode = SpecularMode.Metallic;
        public TransparencyMode transparencyMode = TransparencyMode.Fade;
        public GlossRoughMode glossRoughMode = GlossRoughMode.Gloss;
        public LightCount lightCount = LightCount.Multi;

        public bool useAmbient = true;
        public bool maskedSpec = true;
        public bool geometricAntiAliasing = false;
        //public bool shadowCast = true;
        //public bool shadowReceive = true;
        public bool bakedLight = false;
        public bool highQualityLightProbes = false;
        public bool reflectprobed = false;
        public bool energyConserving = false;
        public bool remapGlossExponentially = true;
        public bool includeMetaPass = true;

        public enum RenderPath { Forward, Deferred };
        public string[] strRenderPath = new string[] { "正向", "延时" };


        public enum LightPrecision { Vertex, Fragment };
        public string[] strLightPrecision = new string[] { "Per-Vertex", "Per-Fragment" };
        public enum LightMode { Unlit, BlinnPhong, Phong, PBL };
        public string[] strLightMode = new string[] { "无光照/自定义", "Blinn-Phong", "Phong", "PBL" };
        public enum SpecularMode { Specular, Metallic };
        public string[] strSpecularMode = new string[] { "镜面", "金属" };
        public enum TransparencyMode { Fade, Reflective };
        public string[] strTransparencyMode = new string[] { "减淡", "反光" };
        public enum GlossRoughMode { Gloss, Roughness };
        public string[] strGlossRoughMode = new string[] { "光泽", "粗糙" };
        public enum LightCount { Single, Multi };
        public string[] strLightCount = new string[] { "单一方向", "多光照" };



        public override string Serialize()
        {
            string s = "";
            s += Serialize("lico", ((int)lightCount).ToString());
            s += Serialize("lgpr", ((int)lightPrecision).ToString());
            s += Serialize("limd", ((int)lightMode).ToString());
            s += Serialize("spmd", ((int)specularMode).ToString());
            s += Serialize("trmd", ((int)transparencyMode).ToString());
            s += Serialize("grmd", ((int)glossRoughMode).ToString());
            s += Serialize("uamb", useAmbient.ToString());
            s += Serialize("mssp", maskedSpec.ToString());
            s += Serialize("bkdf", bakedLight.ToString());
            s += Serialize("hqlp", highQualityLightProbes.ToString());
            s += Serialize("rprd", reflectprobed.ToString());
            s += Serialize("enco", energyConserving.ToString());
            s += Serialize("rmgx", remapGlossExponentially.ToString());
            s += Serialize("imps", includeMetaPass.ToString());
            s += Serialize("rpth", ((int)renderPath).ToString());

            //s += Serialize( "shdc", shadowCast.ToString() );
            //s += Serialize( "shdr", shadowReceive.ToString() );
            return s;
        }

        public override void Deserialize(string key, string value)
        {


            switch (key)
            {
                case "lgpr":
                    lightPrecision = (LightPrecision)int.Parse(value);
                    break;
                case "limd":
                    lightMode = (LightMode)int.Parse(value);
                    break;
                case "uamb":
                    useAmbient = bool.Parse(value);
                    break;
                case "mssp":
                    maskedSpec = bool.Parse(value);
                    break;
                case "bkdf":
                    bakedLight = bool.Parse(value);
                    break;
                case "spmd":
                    specularMode = (SpecularMode)int.Parse(value);
                    break;
                case "trmd":
                    transparencyMode = (TransparencyMode)int.Parse(value);
                    break;
                case "grmd":
                    glossRoughMode = (GlossRoughMode)int.Parse(value);
                    break;

                /*case "shdc":
                    shadowCast = bool.Parse( value );
                    break;
                case "shdr":
                    shadowReceive = bool.Parse( value );
                    break;*/
                case "lico":
                    lightCount = (LightCount)int.Parse(value);
                    break;
                case "lmpd":
                    bakedLight |= bool.Parse(value);
                    break;
                case "lprd":
                    bakedLight |= bool.Parse(value);
                    break;
                case "hqlp":
                    highQualityLightProbes = bool.Parse(value);
                    break;
                case "rprd":
                    reflectprobed = bool.Parse(value);
                    break;
                case "enco":
                    energyConserving = bool.Parse(value);
                    break;


                case "rmgx":
                    remapGlossExponentially = bool.Parse(value);
                    break;
                case "imps":
                    includeMetaPass = bool.Parse(value);
                    break;
                case "rpth":
                    renderPath = (RenderPath)int.Parse(value);
                    break;
            }

        }



        public override float DrawInner(ref Rect r)
        {

            float prevYpos = r.y;
            r.y = 0;


            r.xMin += 20;
            r.y += 20;

            renderPath = (RenderPath)UndoableContentScaledToolbar(r, "渲染途径", (int)renderPath, strRenderPath, "render path");


            if (renderPath == RenderPath.Deferred)
            {
                if (lightMode != LightMode.PBL)
                    lightMode = LightMode.PBL;
                if (ps.catBlending.autoSort == false)
                {
                    ps.catBlending.autoSort = true;
                }
                if (ps.catBlending.blendModePreset != BlendModePreset.Opaque)
                {
                    ps.catBlending.blendModePreset = BlendModePreset.Opaque;
                    ps.catBlending.ConformBlendsToPreset();
                }
            }
            r.y += 20;
            if (renderPath == RenderPath.Deferred)
            {
                GUI.enabled = false;
                UndoableContentScaledToolbar(r, "光照模式", (int)LightMode.PBL, strLightMode, "light mode");
                GUI.enabled = true;
            }
            else
            {
                lightMode = (LightMode)UndoableContentScaledToolbar(r, "光照模式", (int)lightMode, strLightMode, "light mode");
            }
            r.y += 20;

            if (IsPBL())
            {
                specularMode = (SpecularMode)UndoableContentScaledToolbar(r, "镜面模式", (int)specularMode, strSpecularMode, "specular mode");
                r.y += 20;
            }

            GUI.enabled = ps.HasSpecular();
            glossRoughMode = (GlossRoughMode)UndoableContentScaledToolbar(r, "光泽模式", (int)glossRoughMode, strGlossRoughMode, "gloss mode");
            r.y += 20;
            GUI.enabled = true;

            GUI.enabled = ps.HasAlpha(); // Has Opacity connected
            transparencyMode = (TransparencyMode)UndoableContentScaledToolbar(r, "透明模式", (int)transparencyMode, strTransparencyMode, "transparency mode");
            r.y += 20;
            GUI.enabled = true;



            if (ps.catLighting.IsPBL() == false)
            {
                UndoableConditionalToggle(r, ref remapGlossExponentially,
                                     usableIf: ps.HasGloss() && renderPath != RenderPath.Deferred,
                                     disabledDisplayValue: renderPath == RenderPath.Deferred ? true : false,
                                     label: "从 [0-1] 到 " + ((renderPath == RenderPath.Deferred) ? "[0-128]" : "[1-2048]" + " 重映射光泽"),
                                     undoSuffix: "gloss remap"
                                     );
                r.y += 20;
            }



            if (lightMode == LightMode.Unlit || lightMode == LightMode.PBL)
                GUI.enabled = false;
            {

                //bool b = energyConserving;
                if (lightMode == LightMode.PBL)
                    GUI.Toggle(r, true, "能量守恒"); // Dummy display of a checked energy conserve
                else
                    energyConserving = UndoableToggle(r, energyConserving, "能量守恒", "energy conservation", null);
                //energyConserving = GUI.Toggle( r, energyConserving, "Energy Conserving" );

                r.y += 20;
                GUI.enabled = true;
            }


            GUI.enabled = renderPath == RenderPath.Forward;
            lightCount = (LightCount)UndoableContentScaledToolbar(r, "光照数量", (int)lightCount, strLightCount, "light count");
            GUI.enabled = true;
            r.y += 20;


            //lightPrecision = (LightPrecision)ContentScaledToolbar(r, "Light Quality", (int)lightPrecision, strLightPrecision ); // TODO: Too unstable for release
            //r.y += 20;	


            UndoableConditionalToggle(r, ref bakedLight,
                                     usableIf: ps.HasDiffuse() && lightMode != LightMode.Unlit,
                                     disabledDisplayValue: false,
                                     label: "光照贴图 和 光照探针 （请指定 MainTex）",
                                     undoSuffix: "lightmap & light probes"
                                     );
            r.y += 20;


            bool wantsMetaPass = ps.catLighting.bakedLight && (ps.HasDiffuse() || ps.HasEmissive());
            UndoableConditionalToggle(r, ref includeMetaPass,
                                     usableIf: wantsMetaPass,
                                     disabledDisplayValue: false,
                                     label: "写入元通道（光反射颜色）",
                                     undoSuffix: "write meta pass"
                                     );
            r.y += 20;

            //includeMetaPass = UndoableToggle( r, includeMetaPass, "Write meta pass (light bounce coloring)", "write meta pass", null );
            //r.y += 20;

            highQualityLightProbes = UndoableToggle(r, highQualityLightProbes, "每像素光探针采样", "per-pixel light probe sampling", null);
            r.y += 20;



            UndoableConditionalToggle(r, ref reflectprobed,
                                    usableIf: ps.HasSpecular() && lightMode != LightMode.Unlit,
                                    disabledDisplayValue: false,
                                    label: "反射探测器支持",
                                    undoSuffix: "reflection probe support"
                                    );
            r.y += 20;




            /*shadowCast = GUI.Toggle( r, shadowCast, "Cast shadows" );
			r.y += 20;
			shadowReceive = GUI.Toggle( r, shadowReceive, "Receive shadows" );
			r.y += 20;*/




            //GUI.enabled = IsLit();
            /*
			UndoableConditionalToggle( r, ref geometricAntiAliasing,
									 usableIf: ps.HasSpecular() && ps.catLighting.IsPBL(),
									 disabledDisplayValue: false,
									 label: "Geometric specular anti-aliasing",
									 undoSuffix: "geometric specular anti-aliasing"
									 );
			r.y += 20;
			*/

            UndoableConditionalToggle(r, ref useAmbient,
                                     usableIf: !bakedLight && ps.catLighting.IsLit(),
                                     disabledDisplayValue: bakedLight,
                                     label: "接收环境光",
                                     undoSuffix: "receive ambient light"
                                     );
            r.y += 20;

            /*
			if(lightprobed){
				GUI.enabled = false;
				GUI.Toggle( r, true, "Receive Ambient Light" );
				GUI.enabled = true;
			}else{
				useAmbient = GUI.Toggle( r, useAmbient, "Receive Ambient Light" );
			}*/


            //r.y += 20;

            /* DISABLED DUE TO CAUSING TOO MANY ARTIFACTS
			if(ps.catLighting.HasSpecular() && renderPath == RenderPath.Forward){
				maskedSpec = UndoableToggle( r, maskedSpec, "Mask directional light specular by shadows", "directional light specular shadow masking", null );
			} else {
				GUI.enabled = false;
				GUI.Toggle( r, false, "Mask directional light specular by shadows" );
				GUI.enabled = true;
			}
			r.y += 20;*/

            r.y += prevYpos;

            return (int)r.yMax;
        }







        public bool UseMultipleLights()
        {
            return lightCount == LightCount.Multi;
        }

        public bool IsVertexLit()
        {
            return (IsLit() && (lightPrecision == LightPrecision.Vertex));
        }

        public bool IsFragmentLit()
        {
            return (IsLit() && (lightPrecision == LightPrecision.Fragment));
        }

        public bool IsLit()
        {
            return (lightMode != LightMode.Unlit && (ps.HasDiffuse() || HasSpecular()));
        }

        public bool IsEnergyConserving()
        {
            return IsLit() && (energyConserving || lightMode == LightMode.PBL);
        }

        public bool IsPBL()
        {
            return lightMode == LightMode.PBL;
        }

        public bool HasSpecular()
        {
            return (lightMode == LightMode.BlinnPhong || lightMode == LightMode.Phong || lightMode == LightMode.PBL) && (ps.mOut.specular.IsConnectedAndEnabled());
        }









    }
}