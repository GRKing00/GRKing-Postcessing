using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomPostcessing.Blur.TiltShiftBlur
{
    public class TiltShiftBlurRenderPass : ScriptableRenderPass
    {
        private const string m_ProfilerTag = "TiltShiftBlurRenderPass";
        private static readonly ProfilingSampler m_ProfilingSampler = new ProfilingSampler(m_ProfilerTag);
        
        private TiltShiftBlurSettings m_Settings = new TiltShiftBlurSettings();
        private Material m_Material;

        private float m_CosValue;
        private float m_SinValue;

        private RTHandle m_Source;
        private RTHandle m_TempRT;
        private RenderTextureDescriptor m_Descriptor;

        public TiltShiftBlurRenderPass(Material mat, RenderPassEvent evt)
        {
            m_Material = mat;
            renderPassEvent = evt;
            
            m_CosValue = Mathf.Cos(2.39996323f);
            m_SinValue = Mathf.Sin(2.39996323f);
        }

        public void Setup(float radius, int iterations, DownSampleMode mode,
            float centerOffset, float blurRange, float power)
        {
            m_Settings.blurRadius =  radius;
            m_Settings.blurIterations = iterations;
            m_Settings.RTDownScale = mode;
            m_Settings.centerOffset = centerOffset;
            m_Settings.blurRange = blurRange;
            m_Settings.power = power;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            m_Source = renderingData.cameraData.renderer.cameraColorTargetHandle;
            
            m_Descriptor = renderingData.cameraData.cameraTargetDescriptor;
            m_Descriptor.useMipMap = false;
            m_Descriptor.autoGenerateMips = false;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                int downBit = 0;
                switch (m_Settings.RTDownScale)
                {
                    case DownSampleMode.Full:
                        downBit = 0;
                        break;
                    case DownSampleMode.Half:
                        downBit = 1;
                        break;
                    case DownSampleMode.Quarter:
                        downBit = 2;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                int tw = Mathf.Max(1, m_Descriptor.width >> downBit);
                int th = Mathf.Max(1, m_Descriptor.height >> downBit);
                
                var desc = m_Descriptor;
                desc.depthBufferBits = 0;
                desc.msaaSamples = 1;
                desc.width = tw;
                desc.height = th;
                
                RenderingUtils.ReAllocateIfNeeded(ref m_TempRT,desc,
                    FilterMode.Bilinear,TextureWrapMode.Clamp,name:"_TempRT");
                
                Blitter.BlitCameraTexture(cmd,m_Source,m_TempRT);
                
                m_Material.SetVector("_Params", new Vector4(m_CosValue,m_SinValue,
                    m_Settings.blurRadius, m_Settings.blurIterations));
                m_Material.SetFloat("_Offset",m_Settings.centerOffset);
                m_Material.SetFloat("_Area",m_Settings.blurRange);
                m_Material.SetFloat("_Power",m_Settings.power);
                
                Blitter.BlitCameraTexture(cmd,m_TempRT,m_Source,
                    RenderBufferLoadAction.DontCare,RenderBufferStoreAction.Store,m_Material,0);
            }
            
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
        
        public void Dispose()
        {
            m_TempRT?.Release();
        }
    }
}