using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomPostcessing.Blur.TiltShiftBlur
{
    public class TiltShiftBlurRendererFeature : ScriptableRendererFeature
    {
        public RenderPassEvent m_Event;
        public Shader m_Shader;
        public TiltShiftBlurSettings m_Settings =  new TiltShiftBlurSettings();
        
        private Material m_Material;
        private TiltShiftBlurRenderPass m_RenderPass;
        
        public override void Create()
        {
            m_Material = CoreUtils.CreateEngineMaterial(m_Shader);

            m_RenderPass = new TiltShiftBlurRenderPass(m_Material, m_Event);
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
                m_Settings.centerOffset,
                m_Settings.blurRange,
                m_Settings.power);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(m_Material);
            m_RenderPass?.Dispose();
            m_RenderPass = null;
        }
    }

    [Serializable]
    public class TiltShiftBlurSettings
    {
        [Range(0f,10f)] public float blurRadius;
        [Range(1,100)] public int blurIterations;
        public DownSampleMode RTDownScale;
        [Range(-1f,1f)]public float centerOffset;
        [Range(0f,5f)]public float blurRange;
        [Range(0f,5f)]public float power;
    }
}