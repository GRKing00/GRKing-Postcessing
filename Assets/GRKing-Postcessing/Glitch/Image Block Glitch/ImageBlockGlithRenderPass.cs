using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomPostcessing.Glitch.ImageBlockGlitch
{
    public class ImageBlockGlitchRenderPass : ScriptableRenderPass
    {
        private const string m_ProfilerTag = "Image Block Glitch";
        private ProfilingSampler m_ProfilingSampler = new ProfilingSampler(m_ProfilerTag);
        
        private ImageBlockGlitchSettings m_Settings =  new ImageBlockGlitchSettings();
        private Material m_Material;

        private RTHandle m_Source;
        private RTHandle m_TempRT;
        private RenderTextureDescriptor m_Descriptor;
        
        public ImageBlockGlitchRenderPass(Material mat, RenderPassEvent evt)
        {
            m_Material = mat;
            renderPassEvent = evt;
        }

        public void Setup(
            float offset,
            float speed,
            float RGBSplitIntensity,
            float layer1Power,
            float layer2Power,
            float layer1TilingX,
            float layer1TilingY,
            float layer2TilingX,
            float layer2TilingY)
        {
            m_Settings.offset = offset;
            m_Settings.speed = speed;
            m_Settings.RGBSplitIntensity = RGBSplitIntensity;
            m_Settings.layer1Power = layer1Power;
            m_Settings.layer2Power = layer2Power;
            m_Settings.layer1TilingX = layer1TilingX;
            m_Settings.layer1TilingY = layer1TilingY;
            m_Settings.layer2TilingX = layer2TilingX;
            m_Settings.layer2TilingY = layer2TilingY;
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
                

                m_Material.SetFloat("_Offset",m_Settings.offset);
                m_Material.SetVector("_Params",new Vector4(m_Settings.speed, m_Settings.RGBSplitIntensity, 
                    m_Settings.layer1Power,m_Settings.layer2Power));
                m_Material.SetVector("_BlockTiling",new Vector4(m_Settings.layer1TilingX,m_Settings.layer1TilingY,
                    m_Settings.layer2TilingX,m_Settings.layer2TilingY));
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