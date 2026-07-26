Shader "Sprites/ActivePieceOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [HDR] _OutlineColor ("Outline Color", Color) = (0.5, 2.5, 3, 1)
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
            fixed4 _OutlineColor;
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
                fixed spriteAlpha = tex2D(_MainTex, input.uv).a;

                fixed4 color = _OutlineColor;
                color.a *= step(_AlphaThreshold, spriteAlpha);
                return color;
            }
            ENDCG
        }
    }
}
