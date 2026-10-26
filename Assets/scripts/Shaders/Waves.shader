Shader "VertexInputSimple" {
    Properties{
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
    }
    SubShader {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass {
            Tags { "LightMode" = "UniversalForward" }
            // _CTime("Time", float)
            
            HLSLPROGRAM
            float _CTime  =0;
            float amplitudes[2];
            float4 directions[2];
            float frequencies[2];


            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #include "waves.hlsl"
            
            ENDHLSL
        }
    } 
}