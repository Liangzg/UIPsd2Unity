Shader "Unlit/Transparent Colored"  
{  
    Properties  
    {  
        _MainTex ("Base (RGB), Alpha (A)", 2D) = "black" {}  
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
            float4 _MainTex_ST;  
                  
            v2f vert (appdata_t v)  
            {  
                v2f o;  
                o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);  
                o.texcoord = v.texcoord;  
                o.color = v.color;  
                o.gray = dot(v.color, fixed4(1,1,1,0));  
                return o;  
            }  
                  
            fixed4 frag (v2f i) : COLOR  
            {  
                fixed4 col;  
                col = tex2D(_MainTex, i.texcoord);  
                  
                if(i.gray == 0)  
                {  
                    float grey = dot(col.rgb, float3(0.299, 0.587, 0.114));  
                    col.rgb = float3(grey, grey, grey);  
                }  
                else  
                {  
                  col = col * i.color;  
                }  
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