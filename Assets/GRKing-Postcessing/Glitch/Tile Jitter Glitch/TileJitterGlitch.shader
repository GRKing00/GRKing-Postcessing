Shader "CustomProcessing/TileJitterGlitch"
{
    HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        float4 _Params;
        #define _Frequency _Params.x
        #define _SplittingNumber _Params.y
        #define _JitterSpeed _Params.z
        #define _JitterAmount _Params.w

        half4 frag (Varyings input) : SV_Target
        {
			float2 uv = input.texcoord.xy;
			half strength = 0.5 + 0.5 * cos(_Time.y * _Frequency);
			half pixelSizeX = 1.0 / _ScreenParams.x;

			if(fmod(uv.y * int(_SplittingNumber), 2) < 1.0)
			{
				uv.x += pixelSizeX * cos(_Time.y * _JitterSpeed) * _JitterAmount * strength;
			}

			half4 sceneColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv);
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
            Name "TileJitterGlitchPass"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment frag

            ENDHLSL
        }
    }
}