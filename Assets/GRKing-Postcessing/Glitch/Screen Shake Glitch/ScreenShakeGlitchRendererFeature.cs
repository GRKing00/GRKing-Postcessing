using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomPostcessing.Glitch.ScreenShakeGlitch
{
    public class ScreenShakeGlitchRendererFeature : ScriptableRendererFeature
    {
        public RenderPassEvent m_Event;
        public Shader m_Shader;
        public ScreenShakeGlitchSettings m_settings = new ScreenShakeGlitchSettings();
        
        private Material m_Material;
        private ScreenShakeGlitchRenderPass m_RenderPass;
        
        public override void Create()
        {
            m_Material = CoreUtils.CreateEngineMaterial(m_Shader);

            m_RenderPass = new ScreenShakeGlitchRenderPass(m_Material, m_Event);
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
            m_RenderPass.Setup(m_settings.shakeSpeed, m_settings.shakeIntensity);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(m_Material);
            m_RenderPass?.Dispose();
        }
    }
    
    [Serializable]
    public class ScreenShakeGlitchSettings
    {
        [Range(0f, 20f)] public float shakeSpeed;
        [Range(0f, 1f)] public float shakeIntensity;
    }
}