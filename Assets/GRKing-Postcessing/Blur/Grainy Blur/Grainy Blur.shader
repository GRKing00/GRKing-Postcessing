Shader "CustomEffects/GrainyBlur"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "GrainyBlurPass"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment frag

            float4 _Params;
            #define _BlurRadius _Params.x
            #define _Iteration _Params.y

            float Rand(float2 n)
	        {
		        return sin(dot(n, half2(1233.224, 1743.335)));
	        }

            half4 frag (Varyings input) : SV_Target
            {
				half2 randomOffset = float2(0.0, 0.0);
				half4 finalColor = half4(0.0, 0.0, 0.0, 0.0);
				float random = Rand(input.texcoord + _Time.y);
				
				for (int k = 0; k < int(_Iteration); k ++)
				{
					random = frac(43758.5453 * random + 0.61432);;
					randomOffset.x = (random - 0.5) * 2.0;
					random = frac(43758.5453 * random + 0.61432);
					randomOffset.y = (random - 0.5) * 2.0;
					
					finalColor += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, half2(input.texcoord + randomOffset * _BlurRadius * 0.01));
				}

				return finalColor / _Iteration;
            }
            ENDHLSL
        }
    }
}