using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomPostcessing.Glitch.ImageBlockGlitch
{
    public class ImageBlockGlitchRendererFeature : ScriptableRendererFeature
    {
        public RenderPassEvent m_Event;
        public Shader m_Shader;
        public ImageBlockGlitchSettings m_settings = new ImageBlockGlitchSettings();
        
        private Material m_Material;
        private ImageBlockGlitchRenderPass m_RenderPass;
        
        public override void Create()
        {
            m_Material = CoreUtils.CreateEngineMaterial(m_Shader);

            m_RenderPass = new ImageBlockGlitchRenderPass(m_Material, m_Event);
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
                m_settings.offset, 
                m_settings.speed,
                m_settings.RGBSplitIntensity,
                m_settings.layer1Power,
                m_settings.layer2Power,
                m_settings.layer1TilingX,
                m_settings.layer1TilingY,
                m_settings.layer2TilingX,
                m_settings.layer2TilingY
                );
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(m_Material);
            m_RenderPass?.Dispose();
        }
    }
    
    [Serializable]
    public class ImageBlockGlitchSettings
    {
        [Range(0f, 10f)] public float offset;
        [Range(0f, 10f)] public float speed;
        [Range(0f,5f)] public float RGBSplitIntensity;
        [Range(0f,5f)] public float layer1Power;
        [Range(0f,5f)] public float layer2Power;
        [Range(0f, 30f)] public float layer1TilingX;
        [Range(0f, 30f)] public float layer1TilingY;
        [Range(0f, 30f)] public float layer2TilingX;
        [Range(0f, 30f)] public float layer2TilingY;
    }
}