using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomPostcessing.Glitch.ScreenJumpGlitch
{
    public class ScreenJumpGlitchRendererFeature : ScriptableRendererFeature
    {
        public RenderPassEvent m_Event;
        public Shader m_Shader;
        public ScreenJumpGlitchSettings m_settings = new ScreenJumpGlitchSettings();
        
        private Material m_Material;
        private ScreenJumpGlitchRenderPass m_RenderPass;
        
        public override void Create()
        {
            m_Material = CoreUtils.CreateEngineMaterial(m_Shader);

            m_RenderPass = new ScreenJumpGlitchRenderPass(m_Material, m_Event);
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
            m_RenderPass.Setup(m_settings.jumpSpeed, m_settings.jumpIndensity);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(m_Material);
            m_RenderPass?.Dispose();
        }
    }
    
    [Serializable]
    public class ScreenJumpGlitchSettings
    {
        [Range(0f, 10f)] public float jumpSpeed;
        [Range(0f, 1f)] public float jumpIndensity;
    }
}