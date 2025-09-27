using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomPostcessing.Glitch.WaveJitterGlitch
{
    public class WaveJitterGlitchRenderPass : ScriptableRenderPass
    {
        private const string m_ProfilerTag = "Wave Jitter Glitch";
        private ProfilingSampler m_ProfilingSampler = new ProfilingSampler(m_ProfilerTag);
        
        private WaveJitterGlitchSettings m_Settings =  new WaveJitterGlitchSettings();
        private Material m_Material;

        private RTHandle m_Source;
        private RTHandle m_TempRT;
        private RenderTextureDescriptor m_Descriptor;
        
        public WaveJitterGlitchRenderPass(Material mat, RenderPassEvent evt)
        {
            m_Material = mat;
            renderPassEvent = evt;
        }

        public void Setup(
            int resolutionX, 
            int resolutionY,
            float frequency,
            float RGBSplit,
            float speed,
            float amount)
        {
            m_Settings.resolutionX = resolutionX;
            m_Settings.resolutionY = resolutionY;
            m_Settings.frequency = frequency;
            m_Settings.RGBSplit = RGBSplit;
            m_Settings.speed = speed;
            m_Settings.amount = amount;
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
                
                m_Material.SetVector("_Resolution",new Vector4(m_Settings.resolutionX,m_Settings.resolutionY,0,0));
                m_Material.SetVector("_Params",new Vector4(m_Settings.frequency, m_Settings.RGBSplit, 
                    m_Settings.speed,m_Settings.amount));
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