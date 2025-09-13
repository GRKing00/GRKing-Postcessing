using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomPostcessing.Blur.DualKawaseBlur
{
    public class DualKawaseBlurRendererFeature : ScriptableRendererFeature
    {
        public RenderPassEvent m_Event;
        public Shader m_Shader;
        public DualKawaseBlurSettings m_Settings =  new DualKawaseBlurSettings();
        
        private Material m_Material;
        private DualKawaseRenderPass m_RenderPass;
        
        public override void Create()
        {
            m_Material = CoreUtils.CreateEngineMaterial(m_Shader);

            m_RenderPass = new DualKawaseRenderPass(m_Material, m_Event);
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
                m_Settings.RTDownScale);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(m_Material);
            m_RenderPass?.Dispose();
            m_RenderPass = null;
        }
    }

    [Serializable]
    public class DualKawaseBlurSettings
    {
        [Range(0f, 10f)] public float blurRadius;
        [Range(1,10)] public int blurIterations;
        public DownSampleMode RTDownScale;
    }
    
}