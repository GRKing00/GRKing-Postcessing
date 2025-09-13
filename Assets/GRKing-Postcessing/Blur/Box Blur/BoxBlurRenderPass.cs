using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomPostcessing.Blur.BoxBlur
{
    public class BoxBlurRenderPass : ScriptableRenderPass
    {
        private const string m_ProfilerTag = "BoxBlurRenderPass";
        private ProfilingSampler m_ProfilingSampler = new ProfilingSampler(m_ProfilerTag);
        
        private BoxBlurSettings m_BoxBlurSettings = new BoxBlurSettings();
        private Material m_Material;

        private RTHandle m_Source;
        private RTHandle m_TempRT;
        private RTHandle m_PingRT;
        private RTHandle m_PengRT;
        private RenderTextureDescriptor m_Descriptor;

        public BoxBlurRenderPass(Material mat, RenderPassEvent evt)
        {
            m_Material = mat;
            renderPassEvent = evt;
        }

        public void Setup(float radius, int iterations, DownSampleMode mode)
        {
            m_BoxBlurSettings.blurRadius =  radius;
            m_BoxBlurSettings.blurIterations = iterations;
            m_BoxBlurSettings.RTDownScale = mode;
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
                switch (m_BoxBlurSettings.RTDownScale)
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
                RenderingUtils.ReAllocateIfNeeded(ref m_PingRT,desc,
                    FilterMode.Bilinear,TextureWrapMode.Clamp,name:"_PingRT");
                RenderingUtils.ReAllocateIfNeeded(ref m_PengRT,desc,
                    FilterMode.Bilinear,TextureWrapMode.Clamp,name:"_PengRT");
                
                Blitter.BlitCameraTexture(cmd,m_Source,m_TempRT);

                m_Material.SetFloat("_Offset", m_BoxBlurSettings.blurRadius);
                var lastBlur = m_TempRT;
                for (int i = 1; i < m_BoxBlurSettings.blurIterations; i++)
                {
                    Blitter.BlitCameraTexture(cmd,lastBlur,m_PingRT,
                        RenderBufferLoadAction.DontCare,RenderBufferStoreAction.Store,m_Material,0);
                    Blitter.BlitCameraTexture(cmd,m_PingRT,m_PengRT,
                        RenderBufferLoadAction.DontCare,RenderBufferStoreAction.Store,m_Material,1);
                    lastBlur = m_PengRT;
                }                

                Blitter.BlitCameraTexture(cmd,lastBlur,m_Source);
            }
            
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
        
        public void Dispose()
        {
            m_TempRT?.Release();
            m_PingRT?.Release();
            m_PengRT?.Release();
        }
    }
}