Shader "CustomEffects/DirectionalBlur"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "DirectionalBlurPass"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment frag

            float4 _Params;
            #define _Iteration _Params.x
            #define _Direction _Params.yz
            
            half4 frag (Varyings input) : SV_Target
            {
                half4 color = half4(0.0, 0.0, 0.0, 0.0);

                for (int k = -_Iteration; k < int(_Iteration); k++)
                {
                    color += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord - _Direction * k);
                }
                half4 finalColor = color / (_Iteration * 2.0);

                return finalColor;
            }
            ENDHLSL
        }
    }
}