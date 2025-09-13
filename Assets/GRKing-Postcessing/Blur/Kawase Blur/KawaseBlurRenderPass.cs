using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomPostcessing.Blur.KawaseBlur
{
    public class KawaseBlurRenderPass:ScriptableRenderPass
    {
        private const string m_ProfilerTag = "KawaseBlurRenderPass";
        private ProfilingSampler m_ProfilingSampler =  new ProfilingSampler(m_ProfilerTag);
        
        private KawaseBlurSettings m_KawaseBlurSettings = new KawaseBlurSettings();
        private Material m_Material;

        private RTHandle m_Source;
        private RTHandle m_PingRT;
        private RTHandle m_PengRT;
        private RenderTextureDescriptor m_Descriptor;
        
        public KawaseBlurRenderPass(Material mat, RenderPassEvent evt)
        {
            m_Material = mat;
            renderPassEvent = evt;
        }

        public void Setup(float radius, int iterations, DownSampleMode mode)
        {
            m_KawaseBlurSettings.blurRadius = radius;
            m_KawaseBlurSettings.blurIterations = iterations;
            m_KawaseBlurSettings.RTDownScale = mode;
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
                switch (m_KawaseBlurSettings.RTDownScale)
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
                
                int tw = Mathf.Max(1,m_Descriptor.width >>downBit);
                int th = Mathf.Max(1,m_Descriptor.height >>downBit);
                
                var desc =  m_Descriptor;
                desc.depthBufferBits = 0;
                desc.msaaSamples = 1;
                desc.width = tw;
                desc.height = th;
                
                RenderingUtils.ReAllocateIfNeeded(ref m_PingRT,desc,
                    FilterMode.Bilinear,TextureWrapMode.Clamp,name:"_PingRT");
                RenderingUtils.ReAllocateIfNeeded(ref m_PengRT,desc,
                    FilterMode.Bilinear,TextureWrapMode.Clamp,name:"_PengRT");
                
                Blitter.BlitCameraTexture(cmd,m_Source,m_PingRT);
                
                for (int i = 1; i < m_KawaseBlurSettings.blurIterations; i++)
                {
                    m_Material.SetFloat("_Offset",m_KawaseBlurSettings.blurRadius * i);
                    Blitter.BlitCameraTexture(cmd,m_PingRT,m_PengRT,
                        RenderBufferLoadAction.DontCare,RenderBufferStoreAction.Store,m_Material,0);
                    (m_PingRT,m_PengRT) = (m_PengRT,m_PingRT);
                }
                
                Blitter.BlitCameraTexture(cmd,m_PingRT,m_Source);

            }
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            m_PingRT?.Release();
            m_PengRT?.Release();
        }
        
    }
}