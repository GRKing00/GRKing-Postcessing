
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomPostcessing.Blur.GaussianBlur
{
    public class GaussianBlurRenderPass: ScriptableRenderPass
    {
        private const string m_ProfilerTag = "Gaussian Blur";
        private ProfilingSampler m_ProfilingSampler = new ProfilingSampler(m_ProfilerTag);
        
        private GaussianBlurSettings m_Settings =  new GaussianBlurSettings();
        private Material m_Material;

        private RTHandle m_Source;
        private RTHandle m_TempRT;
        private RTHandle m_Ping;
        private RTHandle m_Peng;
        private RenderTextureDescriptor m_Descriptor;
        
        public GaussianBlurRenderPass(Material mat, RenderPassEvent evt)
        {
            m_Material = mat;
            renderPassEvent = evt;
        }

        public void Setup(float radius, int iterations, DownSampleMode mode)
        {
            m_Settings.blurRadius =  radius;
            m_Settings.iterations = iterations;
            m_Settings.RTDownScale = mode;
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
                int donwBit = 0;
                switch (m_Settings.RTDownScale)
                {
                    case DownSampleMode.Full:
                        donwBit = 0;
                        break;
                    case DownSampleMode.Half:
                        donwBit = 1;
                        break;
                    case DownSampleMode.Quarter:
                        donwBit = 2;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                //要做处理的纹理大小
                int tw = Mathf.Max(1, m_Descriptor.width>>donwBit);
                int th = Mathf.Max(1, m_Descriptor.height>>donwBit);
            
                var desc = m_Descriptor;
                desc.depthBufferBits = 0;
                desc.msaaSamples = 1;
                desc.width = tw;
                desc.height = th;
            
                RenderingUtils.ReAllocateIfNeeded(ref m_TempRT,desc,
                    FilterMode.Bilinear,TextureWrapMode.Clamp,name:"_TempRT");
                RenderingUtils.ReAllocateIfNeeded(ref m_Ping,desc,
                    FilterMode.Bilinear,TextureWrapMode.Clamp,name:"_Ping");
                RenderingUtils.ReAllocateIfNeeded(ref m_Peng,desc,
                    FilterMode.Bilinear,TextureWrapMode.Clamp,name:"_Peng");
                
                Blitter.BlitCameraTexture(cmd,m_Source,m_TempRT);
                
                m_Material.SetFloat("_Offset",m_Settings.blurRadius);
                var lastBlur = m_TempRT;
                for (int i = 1; i < m_Settings.iterations; i++)
                {
                    Blitter.BlitCameraTexture(cmd,lastBlur,m_Ping,RenderBufferLoadAction.DontCare,
                        RenderBufferStoreAction.Store,m_Material,0);
                    Blitter.BlitCameraTexture(cmd,m_Ping,m_Peng,RenderBufferLoadAction.DontCare,
                        RenderBufferStoreAction.Store,m_Material,1);
                    lastBlur = m_Peng;
                }
                Blitter.BlitCameraTexture(cmd, lastBlur, m_Source);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            m_Ping?.Release();
            m_Peng?.Release();
            m_TempRT?.Release();
        }
    }
}

