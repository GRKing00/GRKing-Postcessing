using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomPostcessing.Glitch.DigitalStripeGlitch
{
    public class DigitalStripeGlitchRendererFeature : ScriptableRendererFeature
    {
        public RenderPassEvent m_Event;
        public Shader m_Shader;
        public DigitalStripeGlitchSettings m_settings = new DigitalStripeGlitchSettings();
        
        private Material m_Material;
        private DigitalStripeGlitchRenderPass m_RenderPass;
        
        public override void Create()
        {
            m_Material = CoreUtils.CreateEngineMaterial(m_Shader);

            m_RenderPass = new DigitalStripeGlitchRenderPass(m_Material, m_Event,
                m_settings.noiseTextureWidth, m_settings.noiseTextureHeight);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if(renderingData.cameraData.cameraType !=  CameraType.Game)
                return;
            renderer.EnqueuePass(m_RenderPass);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            if(renderingData.cameraData.cameraType != CameraType.Game)
                return;
            m_RenderPass.Setup(
                m_settings.noiseTextureWidth,
                m_settings.noiseTextureHeight,
                m_settings.updateInterval,
                m_settings.stripLength,
                m_settings.stripColorAdjustIndensity,
                m_settings.intensity,
                m_settings.stripColorAdjustColor);   
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(m_Material);
            m_RenderPass?.Dispose();
        }
    }
    
    [Serializable]
    public class DigitalStripeGlitchSettings
    {
        [Range(5, 20)] public int noiseTextureWidth;
        [Range(5, 20)] public int noiseTextureHeight;
        [Range(0f, 1f)] public float updateInterval;
        [Range(0f, 1f)] public float stripLength;
        [Range(0f, 1f)] public float stripColorAdjustIndensity;
        [Range(0f, 1f)] public float intensity;
        public Color stripColorAdjustColor;

    }
}