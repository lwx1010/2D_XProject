using UnityEngine;
using System.Collections;

namespace ShaderForge
{

    [System.Serializable]
    public class SFN_Final : SF_Node
    {




        public SF_NodeConnector
             diffuse
            , specular
            , gloss
            , normal
            , emissive
            , alpha
            , alphaClip
            , diffusePower
            , refraction
            , transmission
            , lightWrap
            , ambientDiffuse
            , ambientSpecular
            , diffuseOcclusion
            , specularOcclusion
            , customLighting
            , outlineWidth
            , outlineColor
            //, anisotropicDirection
            , vertexOffset
            , displacement
            , tessellation
        ;

        public SFN_Final()
        {

        }

        public SFN_Final Initialize(SF_Editor editor)
        {
            base.Initialize("Main");
            this.editor = editor;
            base.showColor = true;
            AssignID();
            base.nodeName = "Main 主干";
            Vector2 pos = new Vector2(32768, 32768);
            base.rect = new Rect(pos.x - NODE_WIDTH / 2, pos.y - NODE_HEIGHT / 2, NODE_WIDTH * 2.2f, 400 + 20f * 2);

            this.connectors = new SF_NodeConnector[]{
				 
		// SURFACE
				diffuse                 = SF_NodeConnector.Create(this,"diff",                      "Diffuse 漫反射",  ConType.cInput, ValueType.VTvPending    ,true,"float3(0,0,0)"   ).Skip(PassType.ShadCast, PassType.Outline).TypecastTo(3),
                diffusePower            = SF_NodeConnector.Create(this,"diffpow",             "Diffuse Power 漫反射强度",  ConType.cInput, ValueType.VTvPending  ,true,"1"               ).Skip(PassType.Meta, PassType.ShadCast, PassType.Outline).DisplayLockIfDeferredPrePassIsOn(),
                specular                = SF_NodeConnector.Create(this,"spec",                     "Specular 镜面反射（高光）",  ConType.cInput, ValueType.VTvPending    ,true                   ).Skip(PassType.ShadCast, PassType.Outline).TypecastTo(3),
                gloss                   = SF_NodeConnector.Create(this,"gloss",                       "Gloss 光泽",  ConType.cInput, ValueType.VTv1          ,true,"0.5"             ).Skip(PassType.ShadCast, PassType.Outline),
                normal                  = SF_NodeConnector.Create(this,"normal",                     "Normal 法线",  ConType.cInput, ValueType.VTv3          ,true                   ).Skip(PassType.Meta, PassType.ShadCast, PassType.Outline),
                emissive                = SF_NodeConnector.Create(this,"emission",                 "Emission 自发光",  ConType.cInput, ValueType.VTvPending    ,true,"float3(0,0,0)"   ).Skip(PassType.ShadCast, PassType.Outline).TypecastTo(3),
                transmission            = SF_NodeConnector.Create(this,"transm",               "Transmission 透射",  ConType.cInput, ValueType.VTvPending    ,true                   ).Skip(PassType.Meta, PassType.ShadCast, PassType.Outline).TypecastTo(3).DisplayLockIfDeferredPrePassIsOn(),
                lightWrap               = SF_NodeConnector.Create(this,"lwrap",              "Light Wrapping 环绕光",  ConType.cInput, ValueType.VTvPending    ,true                   ).Skip(PassType.Meta, PassType.ShadCast, PassType.Outline).TypecastTo(3).DisplayLockIfDeferredPrePassIsOn(),

		// LIGHTING
				ambientDiffuse          = SF_NodeConnector.Create(this,"amdfl",       "Diffuse Ambient Light 漫反射环境光",  ConType.cInput, ValueType.VTvPending    ,true,"float3(0,0,0)"   ).Skip(PassType.Meta, PassType.ShadCast, PassType.FwdAdd, PassType.Outline).TypecastTo(3),
                ambientSpecular         = SF_NodeConnector.Create(this,"amspl",      "Specular Ambient Light 镜面反射环境光",  ConType.cInput, ValueType.VTvPending    ,true,"float3(0,0,0)"   ).Skip(PassType.Meta, PassType.ShadCast, PassType.FwdAdd, PassType.Outline).TypecastTo(3),
                diffuseOcclusion        = SF_NodeConnector.Create(this,"difocc",  "Diffuse Ambient Occlusion 漫反射环境光闭塞",  ConType.cInput, ValueType.VTv1          ,true,"1"               ).Skip(PassType.Meta, PassType.ShadCast, PassType.FwdAdd, PassType.Outline).TypecastTo(1),
                specularOcclusion       = SF_NodeConnector.Create(this,"spcocc", "Specular Ambient Occlusion 镜面反射环境光闭塞",  ConType.cInput, ValueType.VTv1          ,true,"1"               ).Skip(PassType.Meta, PassType.ShadCast, PassType.FwdAdd, PassType.Outline).TypecastTo(1),
                customLighting          = SF_NodeConnector.Create(this,"custl",             "Custom Lighting 自定义照明",  ConType.cInput, ValueType.VTvPending    ,true                   ).Skip(PassType.Meta, PassType.ShadCast, PassType.Outline).TypecastTo(3).DisplayLockIfDeferredPrePassIsOn(),

		// TRANSPARENCY
				alpha                   = SF_NodeConnector.Create(this,"alpha",                     "Opacity 不透明度",  ConType.cInput, ValueType.VTv1          ,true,"1"               ).Skip(PassType.Meta, PassType.ShadCast, PassType.Outline).DisplayLockIfDeferredPrePassIsOn(),
                alphaClip               = SF_NodeConnector.Create(this,"clip",                 "Opacity Clip 不透明度裁剪",  ConType.cInput, ValueType.VTv1          ,true                   ).Skip(PassType.Meta),
                refraction              = SF_NodeConnector.Create(this,"refract",                "Refraction 折射",  ConType.cInput, ValueType.VTv2          ,true                   ).Skip(PassType.Meta, PassType.ShadCast, PassType.Outline).TypecastTo(2).DisplayLockIfDeferredPrePassIsOn(),

		// DEFORMERS
				outlineWidth            = SF_NodeConnector.Create(this,"olwid",               "Outline Width 描边宽度",  ConType.cInput, ValueType.VTv1          ,true                   ).Skip(PassType.Meta, PassType.ShadCast, PassType.FwdAdd, PassType.FwdBase).DisplayLockIfDeferredPrePassIsOn(),
                outlineColor            = SF_NodeConnector.Create(this,"olcol",               "Outline Color 描边颜色",  ConType.cInput, ValueType.VTvPending    ,true,"float3(0,0,0)"   ).Skip(PassType.Meta, PassType.ShadCast, PassType.FwdAdd, PassType.FwdBase).TypecastTo(3).DisplayLockIfDeferredPrePassIsOn(),
                vertexOffset            = SF_NodeConnector.Create(this,"voffset",             "Vertex Offset 顶点偏移",  ConType.cInput, ValueType.VTvPending    ,true                   ).ForceBlock(ShaderProgram.Vert).TypecastTo(3),
                displacement            = SF_NodeConnector.Create(this,"disp",                 "Displacement 置换",  ConType.cInput, ValueType.VTv3          ,true                   ).ForceBlock(ShaderProgram.Vert).TypecastTo(3),
                tessellation            = SF_NodeConnector.Create(this,"tess",                 "Tessellation 曲面细分",  ConType.cInput, ValueType.VTv1          ,true                   ).ForceBlock(ShaderProgram.Vert)
            };

            //distortion.enableState = EnableState.Disabled;
            //customLighting.enableState = EnableState.Disabled;
            //cusomLightingDiffuse.enableState = EnableState.Disabled;
            //anisotropicDirection.enableState = EnableState.Disabled;


            return this;

        }

        public override void OnUpdateNode(NodeUpdateType updType, bool cascade = true)
        {
            if (cascade)
                editor.OnShaderModified(updType);
        }



    }
}