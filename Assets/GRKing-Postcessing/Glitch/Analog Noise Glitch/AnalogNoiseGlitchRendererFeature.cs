using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomPostcessing.Glitch.AnalogNoiseGlitch
{
    public class AnalogNoiseGlitchRendererFeature : ScriptableRendererFeature
    {
        public RenderPassEvent m_Event;
        public Shader m_Shader;
        public AnalogNoiseGlitchSettings m_settings = new AnalogNoiseGlitchSettings();
        
        private Material m_Material;
        private AnalogNoiseGlitchRenderPass m_RenderPass;
        
        public override void Create()
        {
            m_Material = CoreUtils.CreateEngineMaterial(m_Shader);

            m_RenderPass = new AnalogNoiseGlitchRenderPass(m_Material, m_Event);
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
            m_RenderPass.Setup(m_settings.speed, m_settings.fading, m_settings.luminanceJitterThreshold);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(m_Material);
            m_RenderPass?.Dispose();
        }
    }
    
    [Serializable]
    public class AnalogNoiseGlitchSettings
    {
        [Range(0f, 1f)] public float speed;
        [Range(0f, 1f)] public float fading;
        [Range(0f, 1f)] public float luminanceJitterThreshold;
    }
}