using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomPostcessing.Glitch.LineBlockGlitch
{
    public class LineBlockGlitchRendererFeature : ScriptableRendererFeature
    {
        public RenderPassEvent m_Event;
        public Shader m_Shader;
        public LineBlockGlitchSettings m_settings = new LineBlockGlitchSettings();
        
        private Material m_Material;
        private LineBlockGlitchRenderPass m_RenderPass;
        
        public override void Create()
        {
            m_Material = CoreUtils.CreateEngineMaterial(m_Shader);

            m_RenderPass = new LineBlockGlitchRenderPass(m_Material, m_Event);
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
                m_settings.speed, 
                m_settings.linesWidth,
                m_settings.amount,
                m_settings.offset,
                m_settings.alpha);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(m_Material);
            m_RenderPass?.Dispose();
        }
    }
    
    [Serializable]
    public class LineBlockGlitchSettings
    {
        [Range(0f, 2f)] public float speed;
        [Range(0f, 5f)] public float linesWidth;
        [Range(0f,1f)] public float amount;
        [Range(0f,1f)] public float offset;
        [Range(0f,1f)] public float alpha;
        
    }
}