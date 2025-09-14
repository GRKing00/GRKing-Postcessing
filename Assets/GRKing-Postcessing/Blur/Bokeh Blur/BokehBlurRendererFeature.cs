using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomPostcessing.Blur.BokehBlur
{
    public class BokehBlurRendererFeature : ScriptableRendererFeature
    {
        public RenderPassEvent m_Event;
        public Shader m_Shader;
        public BokehBlurSettings m_BokehBlurSettings =  new BokehBlurSettings();
        
        private Material m_Material;
        private BokehBlurRenderPass m_RenderPass;
        
        public override void Create()
        {
            m_Material = CoreUtils.CreateEngineMaterial(m_Shader);

            m_RenderPass = new BokehBlurRenderPass(m_Material, m_Event);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if(renderingData.cameraData.cameraType != CameraType.Game)
                return;
            renderer.EnqueuePass(m_RenderPass);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            if(renderingData.cameraData.cameraType != CameraType.Game)
                return;
            m_RenderPass.Setup(
                m_BokehBlurSettings.blurRadius, 
                m_BokehBlurSettings.blurIterations,
                m_BokehBlurSettings.RTDownScale);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(m_Material);
            m_RenderPass?.Dispose();
            m_RenderPass = null;
        }
    }

    [Serializable]
    public class BokehBlurSettings
    {
        [Range(0f,10f)] public float blurRadius;
        [Range(1,100)] public int blurIterations;
        public DownSampleMode RTDownScale;
    }
}