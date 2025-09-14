Shader "CustomEffects/BokehBlur"
{
    HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        // x: cos;  y: sin; z: radius; w: iterations 
        float4 _Params;
        float4 _BlitTexture_TexelSize;
        half4 frag (Varyings input) : SV_Target
        {
            float2x2 rot = float2x2(_Params.x, -_Params.y,
                                    _Params.y, _Params.x);

            float4 accum = 0;
            float4 weight = 0;
            
            float scale = 1;
            float2 pos = float2(0, _Params.z);
            for (int i =0; i < _Params.w;i++)
            {
                scale += 1/scale;
                pos = mul(rot,pos);
                half4 bokeh = SAMPLE_TEXTURE2D(_BlitTexture,sampler_LinearClamp,
                    input.texcoord + pos * (scale - 1) * _BlitTexture_TexelSize.xy);
                accum += bokeh * bokeh;
                weight += bokeh;
            }
            return accum / weight;
        }
    
    ENDHLSL
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "BokehBlur"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment frag

            ENDHLSL
        }
    }
}