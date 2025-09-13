Shader "CustomEffects/BoxBlur"
{
    HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        float _Offset;
        float4 _BlitTexture_TexelSize;

        TEXTURE2D(_BlitTexture);
        SAMPLER(sampler_BlitTexture);

        struct Attributes
        {
            uint vertexID : SV_VertexID;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
            float4 uv01 : TEXCOORD1;
            float4 uv23 : TEXCOORD2;
        };

        Varyings VerticalVert(Attributes IN)
        {
            Varyings OUT;
	        OUT.positionCS = float4(
		        IN.vertexID <= 1 ? -1.0 : 3.0,
		        IN.vertexID == 1 ? 3.0 : -1.0,
		        0.0, 1.0
	        );
	        OUT.uv = float2(
		        IN.vertexID <= 1 ? 0.0 : 2.0,
		        IN.vertexID == 1 ? 2.0 : 0.0
	        );
        	
        	#if UNITY_UV_STARTS_AT_TOP
			OUT.uv = OUT.uv * float2(1.0, -1.0) + float2(0.0, 1.0);
			#endif

        	
            OUT.uv01.xy = OUT.uv + float2(0, _Offset) * _BlitTexture_TexelSize.xy;
            OUT.uv01.zw = OUT.uv - float2(0, _Offset) * _BlitTexture_TexelSize.xy;
            OUT.uv23.xy = OUT.uv + float2(0, 2 * _Offset) * _BlitTexture_TexelSize.xy;
            OUT.uv23.zw = OUT.uv - float2(0, 2 * _Offset) * _BlitTexture_TexelSize.xy;
            
	        return OUT;
        }

        Varyings HorizontalVert(Attributes IN)
        {
            Varyings OUT;
	        OUT.positionCS = float4(
		        IN.vertexID <= 1 ? -1.0 : 3.0,
		        IN.vertexID == 1 ? 3.0 : -1.0,
		        0.0, 1.0
	        );
	        OUT.uv = float2(
		        IN.vertexID <= 1 ? 0.0 : 2.0,
		        IN.vertexID == 1 ? 2.0 : 0.0
	        );

        	#if UNITY_UV_STARTS_AT_TOP
			OUT.uv = OUT.uv * float2(1.0, -1.0) + float2(0.0, 1.0);
			#endif
        	
            OUT.uv01.xy = OUT.uv + float2(_Offset, 0) * _BlitTexture_TexelSize.xy;
            OUT.uv01.zw = OUT.uv - float2(_Offset, 0) * _BlitTexture_TexelSize.xy;
            OUT.uv23.xy = OUT.uv + float2(2 * _Offset, 0) * _BlitTexture_TexelSize.xy;
            OUT.uv23.zw = OUT.uv - float2(2 * _Offset, 0) * _BlitTexture_TexelSize.xy;
            
	        return OUT;
            
        }


        half4 BlurFrag (Varyings IN) : SV_Target
        {
			half4 finalColor = 0;

        	finalColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, IN.uv);

        	finalColor += SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, IN.uv01.xy);
        	finalColor += SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, IN.uv01.zw);
        	finalColor += SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, IN.uv23.xy);
        	finalColor += SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, IN.uv23.zw);

        	return finalColor * 0.2;
        }



    ENDHLSL
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "BlurPassVertical"

            HLSLPROGRAM
            
            #pragma vertex VerticalVert
            #pragma fragment BlurFrag
            
            ENDHLSL
        }
        
        Pass
        {
            Name "BlurPassHorizontal"

            HLSLPROGRAM
            
            #pragma vertex HorizontalVert
            #pragma fragment BlurFrag
            
            ENDHLSL
        }
    }
}