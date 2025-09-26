Shader "CustomProcessing/DigitalStripeGlitch"
{
    HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        float4 _StripColorAdjustColor;
        float4 _Params;
        #define _StripColorAdjustIndensity _Params.x
        #define _Indensity _Params.y 

        TEXTURE2D(_NoiseTex);
        SAMPLER(sampler_NoiseTex);
        
        half4 frag (Varyings input) : SV_Target
        {
            // 基础数据准备
             half4 stripNoise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, input.texcoord);
             half threshold = 1.001 - _Indensity * 1.001;

            // uv偏移
            half uvShift = step(threshold, pow(abs(stripNoise.x), 3));
            float2 uv = frac(input.texcoord + stripNoise.yz * uvShift);
            half4 source = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv);

            // 基于废弃帧插值
            half stripIndensity = step(threshold, pow(abs(stripNoise.w), 3)) * _StripColorAdjustIndensity;
            half3 color = lerp(source,  0.5 - source + _StripColorAdjustColor, stripIndensity).rgb;
            return float4(color, source.a);
        }
    ENDHLSL
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "DigitalStripeGlitchPass"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment frag

            ENDHLSL
        }
    }
}