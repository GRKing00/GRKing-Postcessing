using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomPostcessing.Glitch.RGBSplitGlitch
{
    public class RGBSplitGlitchRenderPass : ScriptableRenderPass
    {
        private const string m_ProfilerTag = "RGB Split Glitch";
        private ProfilingSampler m_ProfilingSampler = new ProfilingSampler(m_ProfilerTag);
        
        private RGBSplitGlitchSettings m_Settings =  new RGBSplitGlitchSettings();
        private Material m_Material;

        private RTHandle m_Source;
        private RTHandle m_TempRT;
        private RenderTextureDescriptor m_Descriptor;
        
        public RGBSplitGlitchRenderPass(Material mat, RenderPassEvent evt)
        {
            m_Material = mat;
            renderPassEvent = evt;
        }

        public void Setup(float amplitude, float distance)
        {
            m_Settings.amplitude = amplitude;
            m_Settings.distance = distance;
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
                
                m_Material.SetVector("_Params",new Vector4(m_Settings.amplitude, m_Settings.distance, 0,0));
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