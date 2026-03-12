Shader "Custom/UI/DarknessHole"
{
    Properties
    {
        _Color ("Darkness Color", Color) = (0.07, 0.09, 0.14, 0.78)
        _HoleCenter ("Hole Center", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Hole Radius", Float) = 0.18
        _Softness ("Hole Softness", Float) = 0.12
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
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
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            fixed4 _Color;
            float4 _HoleCenter;
            float _Radius;
            float _Softness;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float dist = distance(i.uv, _HoleCenter.xy);

                // inside hole = more transparent
                float mask = smoothstep(_Radius, _Radius + _Softness, dist);

                fixed4 col = _Color;
                col.a *= mask;

                return col;
            }
            ENDCG
        }
    }
}