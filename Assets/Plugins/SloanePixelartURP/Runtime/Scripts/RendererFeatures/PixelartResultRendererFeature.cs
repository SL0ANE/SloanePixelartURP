using System;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_6000_0_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#endif
using UnityEngine.Rendering.Universal;

namespace Sloane.PixelartURP
{
    public class PixelartResultRendererFeature : ScriptableRendererFeature
    {
        private PixelartResultBlitPass m_PixelartResultBlitPass;
        public override void Create()
        {
            m_PixelartResultBlitPass = new PixelartResultBlitPass()
            {
                renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_PixelartResultBlitPass);
        }
    }

    public class PixelartResultBlitPass : ScriptableRenderPass
    {
        private static readonly string k_PassTag = "Pixelart Result Blit";
        [Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;
            var pixelArtCamera = PixelartCamera.GetPixelartCamera(camera);
            if (pixelArtCamera == null) return;

            var cmd = CommandBufferPool.Get(k_PassTag);
            using (new ProfilingScope(cmd, new ProfilingSampler(k_PassTag)))
            {
                cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                cmd.SetRenderTarget(pixelArtCamera.ResultTexture);
                cmd.Blit(pixelArtCamera.ResultTexture, renderingData.cameraData.renderer.cameraColorTargetHandle);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

#if UNITY_6000_0_OR_NEWER
        private class PassData
        {
            public RTHandle Source;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameContext)
        {
            UniversalCameraData cameraData = frameContext.Get<UniversalCameraData>();
            var resourceData = frameContext.Get<UniversalResourceData>();
            var camera = cameraData.camera;
            var pixelArtCamera = PixelartCamera.GetPixelartCamera(camera);
            if (pixelArtCamera == null) return;
            
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(k_PassTag, out var passData))
            {
                builder.AllowPassCulling(false);
                passData.Source = pixelArtCamera.ResultHandle;
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
            }
        }

        private static void ExecutePass(PassData data, RasterGraphContext context)
        {
            Blitter.BlitTexture(context.cmd, data.Source, new Vector4(1, 1, 0, 0), 0, false);
        }
#endif
    }
}
