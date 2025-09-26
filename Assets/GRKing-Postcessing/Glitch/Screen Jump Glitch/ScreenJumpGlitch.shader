Shader "CustomProcessing/ScreenJumpGlitch"
{
    HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        float4 _Params;
        #define _JumpSpeed _Params.x
        #define _JumpIndensity _Params.y
        
        half4 frag (Varyings input) : SV_Target
        {
            float jump = lerp(input.texcoord.y, frac(input.texcoord.y + _JumpSpeed * _Time.y), _JumpIndensity);        
            half4 sceneColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, frac(float2(input.texcoord.x, jump)));   
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
            Name "ScreenJumpGlitchPass"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment frag

            ENDHLSL
        }
    }
}