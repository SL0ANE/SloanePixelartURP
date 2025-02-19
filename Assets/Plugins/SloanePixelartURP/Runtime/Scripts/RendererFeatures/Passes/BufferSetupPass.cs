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
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameContext)
        {
            // Render Graph API 在第一次用到渲染目标时才创建
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
            // Render Graph API 下自动管理渲染目标
        }
#endif
    }
}
