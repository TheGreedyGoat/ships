Shader "Custom/AimingMesh"
{
    Properties
    {
        _EmptyColor("Negative Proogress Color", Color ) = (1,0,0,1)
        _ProgressColor("Positive Progress Color", Color ) = (0,1,0,1)
        _CompleteColor("Completed", Color) = (1,1,1,1)
        _Progress("Progress", Range(0,1)) = 0
        _FadeStart("Fade Start", Range(0,1)) = 0.1
        _FadeEnd("Fade End", Range(0,1)) = 0.1
    }

    SubShader
    {
        Tags { 
        "RenderType" = "Transparent" 
        "RenderPipeline" = "UniversalPipeline"
        "Queue"="Transparent"
        }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            half4 _EmptyColor;
            half4 _ProgressColor;
            half4 _CompleteColor;
            float _Progress;
            float _FadeStart;
            float _FadeEnd;
            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes IN){
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            float invLerp(float a, float b, float value){
                return (value-a )/ (b-a);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float percFromCenter =  abs(IN.uv.x - 0.5) * 2;
                half4 color;
                if(_Progress == 1){
                    color = _CompleteColor;
                }
                else if(percFromCenter <= _Progress){
                    color =  _ProgressColor;
                }else{
                    color = _EmptyColor;
                }

                float alphaPerc = clamp(invLerp(_FadeStart, _FadeEnd, percFromCenter), 0,1);
                // lerp(_EmptyColor, _ProgressColor, posValue);
                return half4(color.xyz, lerp(0.5, 0, alphaPerc));
            }
            ENDHLSL
        }
    }
}
