using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_6000_0_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#endif
using UnityEngine.Rendering.Universal;

namespace Sloane.PixelartURP
{
    public class BufferSetupPass : ScriptableRenderPass
    {
        private static readonly string k_PassTag = "Buffer Setup";
        [Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;
            var pixelArtCamera = PixelartCamera.GetPixelartCamera(camera, PixelartCamera.CameraTarget.CastCamera);
            if (pixelArtCamera == null) return;

            var cmd = CommandBufferPool.Get(k_PassTag);

            using (new ProfilingScope(cmd, new ProfilingSampler(k_PassTag)))
            {
                for (int i = 0; i < (int)TargetBuffer.Max; i++)
                {
                    TargetBuffer target = (TargetBuffer)i;
                    cmd.GetTemporaryRT(TargetBufferUtil.GetBufferShaderProperty(target), TargetBufferUtil.GetDescriptor(pixelArtCamera.CameraData, target), FilterMode.Point);
                }
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

#if UNITY_6000_0_OR_NEWER
        private class PassData
        {
            public TextureHandle[] targetBufferHandles = new TextureHandle[(int)TargetBuffer.Max];
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameContext)
        {
            UniversalCameraData cameraData = frameContext.Get<UniversalCameraData>();
            var camera = cameraData.camera;
            var pixelArtCamera = PixelartCamera.GetPixelartCamera(camera, PixelartCamera.CameraTarget.CastCamera);
            using (var builder = renderGraph.AddUnsafePass<PassData>(k_PassTag, out var passData))
            {
                if (pixelArtCamera == null) return;

                builder.AllowPassCulling(false);
                
                for (int i = 0; i < (int)TargetBuffer.Max; i++)
                {
                    TargetBuffer target = (TargetBuffer)i;
                    passData.targetBufferHandles[i] = UniversalRenderer.CreateRenderGraphTexture(renderGraph, TargetBufferUtil.GetDescriptor(pixelArtCamera.CameraData, target), TargetBufferUtil.GetBufferName(target), false);
                    builder.UseTexture(passData.targetBufferHandles[i], AccessFlags.None);
                }

                builder.AllowPassCulling(false);
                builder.SetRenderFunc((PassData data, UnsafeGraphContext context) => ExecutePass(data, context));
            }
        }

        private static void ExecutePass(PassData data, UnsafeGraphContext context)
        {
            for (int i = 0; i < (int)TargetBuffer.Max; i++)
            {
                TargetBuffer target = (TargetBuffer)i;
                context.cmd.SetGlobalTexture(TargetBufferUtil.GetBufferShaderProperty(target), data.targetBufferHandles[i]);
            }
        }
#endif
    }

    public class BufferCleanupPass : ScriptableRenderPass
    {
        private static readonly string k_PassTag = "Buffer Cleanup";
        [Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;
            var pixelArtCamera = PixelartCamera.GetPixelartCamera(camera, PixelartCamera.CameraTarget.CastCamera);
            if (pixelArtCamera == null) return;

            var cmd = CommandBufferPool.Get(k_PassTag);

            using (new ProfilingScope(cmd, new ProfilingSampler(k_PassTag)))
            {
                for (int i = 0; i < (int)TargetBuffer.Max; i++)
                {
                    TargetBuffer target = (TargetBuffer)i;
                    cmd.ReleaseTemporaryRT(TargetBufferUtil.GetBufferShaderProperty(target));
                }
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

#if UNITY_6000_0_OR_NEWER
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameContext)
        {

        }
#endif
    }
}
