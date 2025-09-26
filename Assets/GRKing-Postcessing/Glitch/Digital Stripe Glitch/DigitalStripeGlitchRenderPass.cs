using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomPostcessing.Glitch.DigitalStripeGlitch
{
    public class DigitalStripeGlitchRenderPass : ScriptableRenderPass
    {
        private const string m_ProfilerTag = "Digital Stripe Jitter Glitch";
        private ProfilingSampler m_ProfilingSampler = new ProfilingSampler(m_ProfilerTag);
        
        private DigitalStripeGlitchSettings m_Settings =  new DigitalStripeGlitchSettings();
        private Material m_Material;

        private RTHandle m_Source;
        private RTHandle m_TempRT;
        private RTHandle m_NoiseRT;
        private RenderTextureDescriptor m_Descriptor;
        
        private float timer = 1f;
        Texture2D noiseTex;
        
        public DigitalStripeGlitchRenderPass(Material mat, RenderPassEvent evt,
            int noiseTextureWidth,int  noiseTextureHeight)
        {
            m_Material = mat;
            renderPassEvent = evt;
            
            noiseTex = new Texture2D(noiseTextureWidth, noiseTextureHeight, 
                TextureFormat.RGBA32, false);
        }

        public void Setup(
            int noiseTextureWidth,
            int noiseTextureHeight,
            float updateInterval,
            float stripLength,
            float stripColorAdjustIndensity,
            float intensity,
            Color stripColorAdjustColor)
        {
            m_Settings.noiseTextureWidth = noiseTextureWidth;
            m_Settings.noiseTextureHeight = noiseTextureHeight;
            m_Settings.updateInterval = updateInterval;
            m_Settings.stripLength = stripLength;
            m_Settings.stripColorAdjustIndensity = stripColorAdjustIndensity;
            m_Settings.intensity = intensity;
            m_Settings.stripColorAdjustColor = stripColorAdjustColor;
            

        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            m_Source = renderingData.cameraData.renderer.cameraColorTargetHandle;
            
            m_Descriptor = renderingData.cameraData.cameraTargetDescriptor;
            m_Descriptor.useMipMap = false;
            m_Descriptor.autoGenerateMips = false;
        }

        public void UpdateNoise()
        {
            
            Color color = new Color(Random.value, Random.value, Random.value, Random.value);
            for (int y = 0; y < noiseTex.height; y++)
            {
                for (int x = 0; x < noiseTex.width; x++)
                {
                    //随机值若大于给定strip随机阈值，重新随机颜色
                    if (Random.value > m_Settings.stripLength)
                    {
                        color.r = Random.value;
                        color.g = Random.value;
                        color.b = Random.value;
                        color.a = Random.value;
                    }
                    //设置贴图像素值
                    noiseTex.SetPixel(x, y, color);
                }
            }
            noiseTex.Apply();
            Graphics.Blit(noiseTex, m_NoiseRT);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                var tmpDesc = m_Descriptor;
                tmpDesc.depthBufferBits = 0;
                tmpDesc.msaaSamples = 1;
                tmpDesc.width = m_Settings.noiseTextureWidth;
                tmpDesc.height = m_Settings.noiseTextureHeight;
                tmpDesc.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
                RenderingUtils.ReAllocateIfNeeded(ref m_NoiseRT,tmpDesc,
                    FilterMode.Point,TextureWrapMode.Clamp,name:"_NoiseRT");
                
                timer += Time.deltaTime;
                if (timer >= m_Settings.updateInterval)
                {
                    timer = 0f;
                    UpdateNoise();
                }
                
                
                var desc =  m_Descriptor;
                desc.depthBufferBits = 0;
                desc.msaaSamples = 1;
                
                RenderingUtils.ReAllocateIfNeeded(ref m_TempRT,desc,
                    FilterMode.Bilinear,TextureWrapMode.Clamp,name:"_TempRT");
                
                
                m_Material.SetTexture("_NoiseTex", m_NoiseRT);
                m_Material.SetVector("_StripColorAdjustColor", m_Settings.stripColorAdjustColor);
                m_Material.SetVector("_Params",
                    new Vector4(m_Settings.stripColorAdjustIndensity, m_Settings.intensity,0,0));
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
            m_NoiseRT?.Release();
        }
    }
}