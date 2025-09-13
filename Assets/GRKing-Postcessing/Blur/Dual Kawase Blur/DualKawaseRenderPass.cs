using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomPostcessing.Blur.DualKawaseBlur
{
    public class DualKawaseRenderPass : ScriptableRenderPass
    {
        private const string m_ProfilerTag = "DualKawaseRenderPass";
        private ProfilingSampler m_ProfilingSampler = new ProfilingSampler(m_ProfilerTag);
        
        private DualKawaseBlurSettings m_Settings = new DualKawaseBlurSettings();
        private Material m_Material;

        private const int m_MaxMipCount = 16;
        private RTHandle m_Source;
        private RTHandle[] m_Pyrimid;
        private RenderTextureDescriptor m_Descriptor;
        public DualKawaseRenderPass(Material mat, RenderPassEvent evt )
        {
            m_Material = mat;
            renderPassEvent = evt;
            
            m_Pyrimid = new RTHandle[m_MaxMipCount];
        }

        public void Setup(float radius, int iterations, DownSampleMode mode)
        {
            m_Settings.blurRadius = radius;
            m_Settings.blurIterations = iterations;
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
                
                int tw = Mathf.Max(1,m_Descriptor.width >> downBit);
                int th = Mathf.Max(1,m_Descriptor.height >> downBit);
                
                //确定迭代次数
                int maxSize = Mathf.Max(tw, th);
                int mipCount = Mathf.FloorToInt(Mathf.Log(maxSize, 2) - 1);
                int iterations = Mathf.Clamp(mipCount,1,m_MaxMipCount);
                iterations = Mathf.Min(iterations, m_Settings.blurIterations);
                
                var desc = m_Descriptor;
                desc.depthBufferBits = 0;
                desc.msaaSamples = 1;
                desc.width = tw;
                desc.height = th;

                //分配RT
                for (int i = 0; i < iterations; i++)
                {
                    RenderingUtils.ReAllocateIfNeeded(ref m_Pyrimid[i],desc,
                        FilterMode.Bilinear,TextureWrapMode.Clamp,name:"_Pyrimid" + i);
                    desc.width >>= 1;
                    desc.height >>= 1;
                }
                
                Blitter.BlitCameraTexture(cmd,m_Source,m_Pyrimid[0]);

                m_Material.SetFloat("_Offset",m_Settings.blurRadius);
                var lastLayer = m_Pyrimid[0];
                for (int i = 1; i < iterations; i++)
                {
                    Blitter.BlitCameraTexture(cmd,lastLayer,m_Pyrimid[i],
                        RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,m_Material,0);
                    lastLayer = m_Pyrimid[i];
                }

                for (int i = iterations - 2; i >= 0; i--)
                {
                    Blitter.BlitCameraTexture(cmd,lastLayer,m_Pyrimid[i],
                        RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,m_Material,1);
                    lastLayer = m_Pyrimid[i];
                }
                
                Blitter.BlitCameraTexture(cmd,lastLayer,m_Source);
            }
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            foreach (var handle in m_Pyrimid)
            {
                handle?.Release();
            }
        }
    }
}