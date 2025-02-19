using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace Sloane.PixelartURP
{
    public class SloanePixelartRendererFeature : ScriptableRendererFeature
    {
        private BufferSetupPass m_BufferSetupPass;
        private PixelartResultBlitPass m_PixelartResultBlitPass;
        private RenderOpaqueObjectPass m_RenderOpaqueObjectPass;

        [SerializeField]
        private RenderOpaqueObjectPass.Settings m_RenderOpaqueObjectPassSettings = new RenderOpaqueObjectPass.Settings()
        {
            LayerMask = -1
        };

        public override void Create()
        {
            m_BufferSetupPass = new BufferSetupPass()
            {
                renderPassEvent = RenderPassEvent.BeforeRendering
            };

            m_RenderOpaqueObjectPass = new RenderOpaqueObjectPass(m_RenderOpaqueObjectPassSettings)
            {
                renderPassEvent = RenderPassEvent.AfterRenderingTransparents
            };            

            m_PixelartResultBlitPass = new PixelartResultBlitPass()
            {
                renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_BufferSetupPass);
            renderer.EnqueuePass(m_RenderOpaqueObjectPass);
            renderer.EnqueuePass(m_PixelartResultBlitPass);
        }
    }
}
