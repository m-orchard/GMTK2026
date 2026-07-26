Shader "Sprites/ActivePieceOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [HDR] _OutlineColor ("Outline Color", Color) = (0.5, 2.5, 3, 1)
        _OutlineWidth ("Outline Width (texels)", Range(0, 16)) = 4
        _AlphaThreshold ("Alpha Threshold", Range(0, 1)) = 0.1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #include "UnityCG.cginc"

            struct VertexInput
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct FragmentInput
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            float _AlphaThreshold;

            FragmentInput Vertex(VertexInput input)
            {
                FragmentInput output;
                output.position = UnityObjectToClipPos(input.position);
                output.uv = input.uv;
                return output;
            }

            fixed4 Fragment(FragmentInput input) : SV_Target
            {
                fixed ownAlpha = tex2D(_MainTex, input.uv).a;

                float2 sampleRadius = _MainTex_TexelSize.xy * _OutlineWidth;

                const int DirectionCount = 16;
                fixed neighbourAlpha = 0;

                [unroll(16)]
                for (int direction = 0; direction < DirectionCount; direction++)
                {
                    float angle = (6.28318530718 / DirectionCount) * direction;
                    float2 offset = float2(cos(angle), sin(angle)) * sampleRadius;
                    neighbourAlpha = max(neighbourAlpha, tex2D(_MainTex, input.uv + offset).a);
                }

                float neighbourIsOpaque = step(_AlphaThreshold, neighbourAlpha);
                float ownIsTransparent = 1 - step(_AlphaThreshold, ownAlpha);

                fixed4 color = _OutlineColor;
                color.a *= neighbourIsOpaque * ownIsTransparent;
                return color;
            }
            ENDCG
        }
    }
}
