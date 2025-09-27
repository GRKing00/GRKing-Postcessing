using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomPostcessing.Glitch.WaveJitterGlitch
{
    public class WaveJitterGlitchRendererFeature : ScriptableRendererFeature
    {
        public RenderPassEvent m_Event;
        public Shader m_Shader;
        public WaveJitterGlitchSettings m_settings = new WaveJitterGlitchSettings();
        
        private Material m_Material;
        private WaveJitterGlitchRenderPass m_RenderPass;
        
        public override void Create()
        {
            m_Material = CoreUtils.CreateEngineMaterial(m_Shader);

            m_RenderPass = new WaveJitterGlitchRenderPass(m_Material, m_Event);
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
                m_settings.resolutionX,
                m_settings.resolutionY,
                m_settings.frequency,
                m_settings.RGBSplit,
                m_settings.speed,
                m_settings.amount);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(m_Material);
            m_RenderPass?.Dispose();
        }
    }
    
    [Serializable]
    public class WaveJitterGlitchSettings
    {
        [Range(0, 2048)] public int resolutionX;
        [Range(0, 2048)] public int resolutionY;
        [Range(0f, 5f)] public float frequency;
        [Range(0f, 50f)] public float RGBSplit;
        [Range(0f,1f)] public float speed;
        [Range(0f,1f)] public float amount;
    }
}