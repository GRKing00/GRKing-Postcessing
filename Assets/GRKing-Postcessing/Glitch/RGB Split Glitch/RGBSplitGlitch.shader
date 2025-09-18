Shader "CustomProcessing/RGBSplitGlitch"
{
    HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        //x:amplitude;  y:distance
        float4 _Params;

        half4 frag (Varyings input) : SV_Target
        {
            float splitDistance = (1 + sin(_Time.y * 6.0)) * 0.5;
            splitDistance *= 1 + sin(_Time.y * 16) * 0.5;
            splitDistance *= 1 + sin(_Time.y * 19) * 0.5;
            splitDistance *= 1 + sin(_Time.y * 27) * 0.5;
            splitDistance = saturate(pow(splitDistance,_Params.x));
            splitDistance *= (0.05 * _Params.y);

            half3 finalColor;
            float2 uv0 = float2(input.texcoord.x + splitDistance, input.texcoord.y);
            finalColor.r = SAMPLE_TEXTURE2D(_BlitTexture,sampler_LinearClamp,uv0).r;
            finalColor.g = SAMPLE_TEXTURE2D(_BlitTexture,sampler_LinearClamp,input.texcoord).g;
            float2 uv1 = float2(input.texcoord.x - splitDistance, input.texcoord.y);
            finalColor.b = SAMPLE_TEXTURE2D(_BlitTexture,sampler_LinearClamp,uv1).b;

            finalColor *= (1 - splitDistance * 0.5);
            
            return half4(finalColor,1.0);
        }
    ENDHLSL
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "RGBSplitGlitchPass"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment frag

            ENDHLSL
        }
    }
}