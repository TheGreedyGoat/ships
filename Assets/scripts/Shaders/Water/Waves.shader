Shader "VertexInputSimple" {
    Properties{
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _FogColor("Fog Color", Color) = (1, 1, 1, 1)
        _FogStartDist("Fog Start Distance", Range(10, 500)) =10
        _FogGradientLength("Fog Gradient", Range(1, 500)) = 10
    }
    SubShader {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass {
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            int _WaveCount;
            float _CTime  =0;
            float _Speeds[100];
            float _Amplitudes[100];
            float4 _Directions[100];
            float _WaveLengths[100];


            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #include "waves.hlsl"
            
            ENDHLSL
        }
    } 
}