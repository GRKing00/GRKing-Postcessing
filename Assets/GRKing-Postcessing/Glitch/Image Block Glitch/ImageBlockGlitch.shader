Shader "CustomProcessing/ImageBlockGlitch"
{
    HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        float _Offset;
        float4 _Params;
        #define _Speed _Params.x
        #define _RGBSplit_Intensity _Params.y
        #define _BlockLayer1_Intensity _Params.z
        #define _BlockLayer2_Intensity _Params.w

        //block1_Tiling:xy; block2_Tiling:zw
        float4 _BlockTiling;
        
        inline float randomNoise(float2 seed)
        {
            return frac(sin(dot(seed * floor(_Time.y * _Speed), float2(17.13, 3.71))) * 43758.5453123);
        }

        inline float randomNoise(float seed)
        {
            return randomNoise(float2(seed, 1.0));
        }

        half4 frag (Varyings input) : SV_Target
        {
            float2 uv = input.texcoord;
            float2 blockLayer1 = floor(uv * _BlockTiling.xy);
            float2 blockLayer2 = floor(uv * _BlockTiling.zw);

            float lineNoise1 = pow(randomNoise(blockLayer1), _BlockLayer1_Intensity);
            float lineNoise2 = pow(randomNoise(blockLayer2), _BlockLayer2_Intensity);
            float RGBSplitNoise = pow(randomNoise(5.1379), 7.1) * _RGBSplit_Intensity;
            float lineNoise = lineNoise1 * lineNoise2 * _Offset  - RGBSplitNoise;

            // return lineNoise;

		    float4 colorR = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv);
		    float4 colorG = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + float2(lineNoise * 0.05 * randomNoise(7.0), 0));
		    float4 colorB = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv - float2(lineNoise * 0.05 * randomNoise(23.0), 0));
		    
		    float4 result = float4(float3(colorR.x, colorG.y, colorB.z), saturate(colorR.a + colorG.a + colorB.a));
		    
		    return result;
        }
    ENDHLSL
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "ImageBlockGlitchPass"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment frag

            ENDHLSL
        }
    }
}