Shader "Custom/FogOfWarShader" {
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FogOfWar ("Fog Of War", 2D) = "white" {}
        _MapWidth ("Map Width", float) = 1.0
        _MapHeight ("Map Height", float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex VS
            #pragma fragment PS
            
            #include "UnityCG.cginc"

            struct AppData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _FogOfWar;
            float _MapWidth;
            float _MapHeight;
            
            v2f VS (AppData v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul (unity_ObjectToWorld, v.vertex) * -1.0;

                o.worldPos.x = -o.worldPos.x;
                o.worldPos.y = -o.worldPos.y;
                o.worldPos.z = -o.worldPos.z;
                return o;
            }
            
            fixed4 PS (v2f i) : SV_Target
            {
                fixed4 col = tex2D (_MainTex, i.uv);
                float2 mapCoord;
                mapCoord.x = floor (i.worldPos.x + 0.5f + _MapWidth / 2.0) / _MapWidth;
                mapCoord.y = floor (i.worldPos.y + 0.5f + _MapHeight / 2.0) / _MapHeight;

                fixed4 mapColor = tex2D (_FogOfWar, mapCoord);
                col.r *= mapColor.r;
                col.g *= mapColor.r;
                col.b *= mapColor.r;
                return col;
            }
            ENDCG
        }
    }
}