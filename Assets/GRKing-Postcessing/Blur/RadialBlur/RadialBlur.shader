Shader "CustomEffects/RadialBlur"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "RadialBlurPass"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment frag

            float4 _Params;
            #define _BlurRadius _Params.x
            #define _Iteration _Params.y
            #define _RadialCenter _Params.zw
            
            half4 frag (Varyings input) : SV_Target
            {

                float2 blurVector = (_RadialCenter - input.texcoord.xy) * _BlurRadius * 0.01;

                half4 acumulateColor = half4(0, 0, 0, 0);

                
                for (int j = 0; j < int(_Iteration); j ++)
                {
                    acumulateColor += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);
                    input.texcoord.xy += blurVector;
                }

                return acumulateColor / _Iteration;
            }
            ENDHLSL
        }
    }
}