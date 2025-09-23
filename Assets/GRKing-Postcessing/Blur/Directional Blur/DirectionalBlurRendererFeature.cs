using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomPostcessing.Blur.DirectionalBlur
{
    public class DirectionalBlurRendererFeature : ScriptableRendererFeature
    {
        public RenderPassEvent m_Event;
        public Shader m_Shader;
        public DirectionalBlurSettings m_Settings =  new DirectionalBlurSettings();
        
        private Material m_Material;
        private DirectionalBlurRenderPass m_RenderPass;
        
        public override void Create()
        {
            m_Material = CoreUtils.CreateEngineMaterial(m_Shader);

            m_RenderPass = new DirectionalBlurRenderPass(m_Material, m_Event);
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
                m_Settings.blurRadius, 
                m_Settings.blurIterations,
                m_Settings.RTDownScale,
                m_Settings.angle);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(m_Material);
            m_RenderPass?.Dispose();
            m_RenderPass = null;
        }
    }

    [Serializable]
    public class DirectionalBlurSettings
    {
        [Range(0f,2f)] public float blurRadius;
        [Range(1,50)] public int blurIterations;
        public DownSampleMode RTDownScale;
        [Range(0f,3.1415926f)] public float angle;
    }
}