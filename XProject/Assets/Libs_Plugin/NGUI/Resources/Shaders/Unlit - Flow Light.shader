// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Flow Light"  
{  
    Properties  
    {  
        _MainTex ("Base (RGB), Alpha (A)", 2D) = "black" {}  
		//流光纹理
		_FlowLightTex("Flow Light Texture" , 2D) = "white" {}
		//流光强度
		_FlowLightPower ("Flow Power" , float) = 1.0
		//流光的偏移，通过设置此值来达到UV动画效果
		_FlowLightOffset ("Flow Offset" , Vector) = (0,0,0,0)

		_WidthRate("Sprite width Rate", float) = 1
		_HeightRate("Sprite height Rate", float) = 1
		_Offset("offset", Vector) = (0,0,0,0)
    }  
      
    SubShader  
    {  
        LOD 100  
  
        Tags  
        {  
            "Queue" = "Transparent"  
            "IgnoreProjector" = "True"  
            "RenderType" = "Transparent"  
        }  
          
        Cull Off  
        Lighting Off  
        ZWrite Off  
        Fog { Mode Off }  
        Offset -1, -1  
        Blend SrcAlpha OneMinusSrcAlpha  
  
        Pass  
        {  
            CGPROGRAM  
            #pragma vertex vert  
            #pragma fragment frag  
                  
            #include "UnityCG.cginc"  
      
            struct appdata_t  
            {  
                float4 vertex : POSITION;  
                float2 texcoord : TEXCOORD0;  
                fixed4 color : COLOR;  
            };  
      
            struct v2f  
            {  
                float4 vertex : SV_POSITION;  
                half2 texcoord : TEXCOORD0;  
                fixed4 color : COLOR;  
                fixed gray : TEXCOORD1;   
            };  
      
            sampler2D _MainTex;  
			sampler2D _FlowLightTex;
            float4 _MainTex_ST;  
			float _FlowLightPower;
			float4 _FlowLightOffset;
			float _WidthRate;
			float _HeightRate;
			float4 _Offset;

            v2f vert (appdata_t v)  
            {  
                v2f o;  
                o.vertex = UnityObjectToClipPos(v.vertex);  
                o.texcoord = v.texcoord;  
                o.color = v.color;  
                o.gray = dot(v.color, fixed4(1,1,1,0));  
                return o;  
            }  
                  
            fixed4 frag (v2f i) : COLOR  
            {  
                fixed4 col = tex2D(_MainTex, i.texcoord);  
				col = col * i.color;

				//UV减半处理
				float2 uvFlowLight = float2((i.texcoord.x - _Offset.x) / _WidthRate , (i.texcoord.y - _Offset.y) / _HeightRate);
				if(_Offset.x > 0) uvFlowLight.x *= 0.5;
				if(_Offset.y > 0) uvFlowLight.y *= 0.5;
			    //根据速度变化
				uvFlowLight.x -= _FlowLightOffset.x;
				uvFlowLight.y += _FlowLightOffset.y;
				//用计算后的uv取流光纹理
				fixed4 flowColor = tex2D(_FlowLightTex, uvFlowLight) * _FlowLightPower;
				//颜色叠加
				flowColor.rgb *= col.rgb;
				col.rgb += flowColor.rgb;
				col.rgb *= col.a;
                  
                return col;  
            }  
            ENDCG  
        }  
    }  
  
    SubShader  
    {  
        LOD 100  
  
        Tags  
        {  
            "Queue" = "Transparent"  
            "IgnoreProjector" = "True"  
            "RenderType" = "Transparent"  
        }  
          
        Pass  
        {  
            Cull Off  
            Lighting Off  
            ZWrite Off  
            Fog { Mode Off }  
            Offset -1, -1  
            ColorMask RGB  
            AlphaTest Greater .01  
            Blend SrcAlpha OneMinusSrcAlpha  
            ColorMaterial AmbientAndDiffuse  
              
            SetTexture [_MainTex]  
            {  
                Combine Texture * Primary  
            }  
        }  
    }  
}  