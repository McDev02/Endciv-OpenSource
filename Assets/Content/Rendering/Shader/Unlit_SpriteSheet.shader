// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Unlit shader. Simplest possible colored shader.
// - no lighting
// - no lightmap support
// - no texture

Shader "Endciv/Sprite Sheet Additive" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("Base (RGB) Alpha (A)", 2D) = "grey" {}
    _Sheets ("Sheets (X,Y) Speed (Z) ColorMul (W)", Vector) = (1,1,1,1)
}

SubShader {
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
    LOD 100
    ZWrite Off
    Cull Off

    Pass {
        Blend One One
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(0)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
            half4 _Sheets;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);

                half2 t = fmod(_Time.x* _Sheets.z,1);
                t.x = floor(t.x*_Sheets.x*_Sheets.y);
                t.y = floor((.999-t.y)*_Sheets.y);
                o.uv.x = (v.texcoord.x + t.x)/_Sheets.x;
                o.uv.y = (v.texcoord.y + t.y)/_Sheets.y;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            sampler2D _MainTex;
            fixed4 frag (v2f i) : COLOR
            {
                fixed4 col = tex2D( _MainTex, i.uv) * _Sheets.w * _Color;
                UNITY_APPLY_FOG(i.fogCoord, col);
                UNITY_OPAQUE_ALPHA(col.a);
                return col;
            }
        ENDCG
    }
}

}
