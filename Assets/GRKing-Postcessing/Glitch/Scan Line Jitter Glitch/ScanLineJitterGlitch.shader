Shader "CustomProcessing/ScanLineJitterGlitch"
{
    HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        float4 _Params;
        #define _ScanLineJitter _Params.xy

        float randomNoise(float x, float y)
        {
            return frac(sin(dot(float2(x, y), float2(12.9898, 78.233))) * 43758.5453);
        }

        
        half4 frag (Varyings input) : SV_Target
        {
            float jitter = randomNoise(input.texcoord.y, _Time.x) * 2 - 1;
            jitter *= step(_ScanLineJitter.y, abs(jitter)) * _ScanLineJitter.x * 0.1;

            half4 sceneColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, frac(input.texcoord + float2(jitter, 0)));

            return sceneColor;
        }
    ENDHLSL
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "ScanLineJitterGlitchPass"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment frag

            ENDHLSL
        }
    }
}