using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Sloane.PixelartURP
{
    public class SloanePixelartRendererFeature : ScriptableRendererFeature
    {
        private BufferSetupPass m_BufferSetupPass;
        private PixelartResultBlitPass m_PixelartResultBlitPass;

        public override void Create()
        {
            m_BufferSetupPass = new BufferSetupPass()
            {
                renderPassEvent = RenderPassEvent.BeforeRendering
            };

            m_PixelartResultBlitPass = new PixelartResultBlitPass()
            {
                renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_BufferSetupPass);
            renderer.EnqueuePass(m_PixelartResultBlitPass);
        }
    }
}
