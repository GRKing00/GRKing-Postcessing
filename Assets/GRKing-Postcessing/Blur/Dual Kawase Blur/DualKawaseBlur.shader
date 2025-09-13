Shader "CustomEffects/DualKawaseBlur"
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

        struct DownVaryings
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
            float4 uv01 : TEXCOORD1;
            float4 uv23 : TEXCOORD2;
        };

        struct UpVaryings
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
            float4 uv01 : TEXCOORD1;
            float4 uv23 : TEXCOORD2;
        	float4 uv45 : TEXCOORD3;
        	float4 uv67 : TEXCOORD4;
        };

        DownVaryings DonwVert(Attributes IN)
        {
            DownVaryings OUT;
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

        	float2 offset = float2(_Offset, _Offset) * _BlitTexture_TexelSize * 0.5;
            OUT.uv01.xy = OUT.uv +  offset;
            OUT.uv01.zw = OUT.uv - offset;
            OUT.uv23.xy = OUT.uv + offset * float2(1,-1);
            OUT.uv23.zw = OUT.uv - offset * float2(1,-1);
            
	        return OUT;
        }

        half4 DownBlurFrag (DownVaryings IN) : SV_Target
        {
			half4 finalColor = 0;

            finalColor += SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, IN.uv) * 4;
        	finalColor += SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, IN.uv01.xy);
        	finalColor += SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, IN.uv01.zw);
        	finalColor += SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, IN.uv23.xy);
        	finalColor += SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, IN.uv23.zw);

        	return finalColor * 0.125;
        }

        UpVaryings UpVert(Attributes IN)
        {
            UpVaryings OUT;
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

        	float2 offset = float2(_Offset, _Offset) * _BlitTexture_TexelSize * 0.5;
            OUT.uv01.xy = OUT.uv +  offset;
            OUT.uv01.zw = OUT.uv - offset;
            OUT.uv23.xy = OUT.uv + offset * float2(1,-1);
            OUT.uv23.zw = OUT.uv - offset * float2(1,-1);
        	OUT.uv45.xy = OUT.uv + offset * float2(0,2);
        	OUT.uv45.zw = OUT.uv - offset * float2(0,2);
        	OUT.uv67.xy = OUT.uv + offset * float2(2,0);
        	OUT.uv67.zw = OUT.uv - offset * float2(2,0);
        	
            
	        return OUT;
        }

        half4 UpBlurFrag (UpVaryings IN) : SV_Target
        {
			half4 finalColor = 0;

        	finalColor += SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, IN.uv01.xy) * 2;
        	finalColor += SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, IN.uv01.zw) * 2;
        	finalColor += SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, IN.uv23.xy) * 2;
        	finalColor += SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, IN.uv23.zw) * 2;
        	finalColor += SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, IN.uv45.xy);
        	finalColor += SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, IN.uv45.zw);
        	finalColor += SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, IN.uv67.xy);
        	finalColor += SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, IN.uv67.zw);

        	return finalColor * 0.0833;
        }



    ENDHLSL
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "DownSample"

            HLSLPROGRAM
            
            #pragma vertex DonwVert
            #pragma fragment DownBlurFrag
            
            ENDHLSL
        }

        Pass
        {
            Name "UpSample"

            HLSLPROGRAM
            
            #pragma vertex UpVert
            #pragma fragment UpBlurFrag
            
            ENDHLSL
        }
    }
}