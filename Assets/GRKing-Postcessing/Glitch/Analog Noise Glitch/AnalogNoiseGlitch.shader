Shader "CustomProcessing/AnalogNoiseGlitch"
{
    HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        float4 _Params;
        #define _Speed _Params.x
        #define _Fading _Params.y
        #define _LuminanceJitterThreshold _Params.z

        float randomNoise(float2 c)
	    {
		    return frac(sin(dot(c.xy, float2(12.9898, 78.233))) * 43758.5453);
	    }

        
        half4 frag (Varyings input) : SV_Target
        {
			half4 sceneColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);
			half4 noiseColor = sceneColor;

			half luminance = dot(noiseColor.rgb, half3(0.22, 0.707, 0.071));
			if (randomNoise(float2(_Time.y * _Speed, _Time.y * _Speed)) > _LuminanceJitterThreshold)
			{
				noiseColor = float4(luminance, luminance, luminance, luminance);
			}

			float noiseX = randomNoise(_Time.y * _Speed + input.texcoord / float2(-213, 5.53));
			float noiseY = randomNoise(_Time.y * _Speed - input.texcoord / float2(213, -5.53));
			float noiseZ = randomNoise(_Time.y * _Speed + input.texcoord / float2(213, 5.53));

			noiseColor.rgb += 0.25 * float3(noiseX,noiseY,noiseZ) - 0.125;

			noiseColor = lerp(sceneColor, noiseColor, _Fading);
			
			return noiseColor;
        }
    ENDHLSL
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "AnalogNoiseGlitchPass"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment frag

            ENDHLSL
        }
    }
}