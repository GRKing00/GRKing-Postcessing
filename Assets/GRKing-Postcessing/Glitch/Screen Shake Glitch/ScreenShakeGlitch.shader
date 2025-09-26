Shader "CustomProcessing/ScreenShakeGlitch"
{
    HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        float4 _Params;
        #define _ShakeSpeed _Params.x
        #define _ShakeIntensity _Params.y

        float randomNoise(float x, float y)
	    {
		    return frac(sin(dot(float2(x, y), float2(127.1, 311.7))) * 43758.5453);
	    }
        
        half4 frag (Varyings input) : SV_Target
        {
            float shake = (randomNoise(floor(_Time.y * _ShakeSpeed), 2) - 0.5) * _ShakeIntensity;

            half4 sceneColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, frac(float2(input.texcoord.x + shake, input.texcoord.y)));

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
            Name "ScreenShakeGlitchPass"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment frag

            ENDHLSL
        }
    }
}