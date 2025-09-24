using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomPostcessing.Glitch.TileJitterGlitch
{
    public class TileJitterGlitchRendererFeature : ScriptableRendererFeature
    {
        public RenderPassEvent m_Event;
        public Shader m_Shader;
        public TileJitterGlitchSettings m_settings = new TileJitterGlitchSettings();
        
        private Material m_Material;
        private TileJitterGlitchRenderPass m_RenderPass;
        
        public override void Create()
        {
            m_Material = CoreUtils.CreateEngineMaterial(m_Shader);

            m_RenderPass = new TileJitterGlitchRenderPass(m_Material, m_Event);
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
                m_settings.frequency,
                m_settings.splitNumber,
                m_settings.jitterSpeed,
                m_settings.jitterAmount);   
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(m_Material);
            m_RenderPass?.Dispose();
        }
    }
    
    [Serializable]
    public class TileJitterGlitchSettings
    {
        [Range(0f, 50f)] public float frequency;
        [Range(0, 10)] public int splitNumber;
        [Range(0f, 50f)] public float jitterSpeed;
        [Range(0f, 500f)] public float jitterAmount;
    }
}