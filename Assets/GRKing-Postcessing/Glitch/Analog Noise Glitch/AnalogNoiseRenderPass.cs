using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomPostcessing.Glitch.AnalogNoiseGlitch
{
    public class AnalogNoiseGlitchRenderPass : ScriptableRenderPass
    {
        private const string m_ProfilerTag = "Analog Noise Glitch";
        private ProfilingSampler m_ProfilingSampler = new ProfilingSampler(m_ProfilerTag);
        
        private AnalogNoiseGlitchSettings m_Settings =  new AnalogNoiseGlitchSettings();
        private Material m_Material;

        private RTHandle m_Source;
        private RTHandle m_TempRT;
        private RenderTextureDescriptor m_Descriptor;
        
        public AnalogNoiseGlitchRenderPass(Material mat, RenderPassEvent evt)
        {
            m_Material = mat;
            renderPassEvent = evt;
        }

        public void Setup(float speed, float fading, float luminanceJitterThreshold)
        {
            m_Settings.speed = speed;
            m_Settings.fading = fading;
            m_Settings.luminanceJitterThreshold = luminanceJitterThreshold;
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
                var desc =  m_Descriptor;
                desc.depthBufferBits = 0;
                desc.msaaSamples = 1;
                
                RenderingUtils.ReAllocateIfNeeded(ref m_TempRT,desc,
                    FilterMode.Bilinear,TextureWrapMode.Clamp,name:"_TempRT");
                
                m_Material.SetVector("_Params",new Vector4(m_Settings.speed, m_Settings.fading, m_Settings.luminanceJitterThreshold,0));
                Blitter.BlitCameraTexture(cmd,m_Source,m_TempRT,
                    RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,m_Material,0);
                Blitter.BlitCameraTexture(cmd,m_TempRT,m_Source);
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