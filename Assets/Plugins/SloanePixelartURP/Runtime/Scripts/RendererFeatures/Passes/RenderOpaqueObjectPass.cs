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
    public class RenderOpaqueObjectPass : ScriptableRenderPass
    {
        public struct Settings
        {
            public LayerMask LayerMask;
        }

        private static readonly string k_PassTag = "Render Opaque Object";
        public static readonly ShaderTagId TargetShaderPass = new ShaderTagId("PixelartOpaque");
        public static readonly RenderTargetIdentifier[] OpaqueBuffersIdentifiers = new RenderTargetIdentifier[(int)TargetBufferStage.MarkerRawData - (int)TargetBufferStage.MarkerDepth]
        {
            TargetBufferUtil.GetBufferShaderProperty(TargetBuffer.AlbedoProperty),
            TargetBufferUtil.GetBufferShaderProperty(TargetBuffer.SpecularProperty),
            TargetBufferUtil.GetBufferShaderProperty(TargetBuffer.LightingProperty),
            TargetBufferUtil.GetBufferShaderProperty(TargetBuffer.MiscProperty),
            TargetBufferUtil.GetBufferShaderProperty(TargetBuffer.Normal),
            TargetBufferUtil.GetBufferShaderProperty(TargetBuffer.NormalExact),
        };

        public static readonly RenderTargetIdentifier DepthBufferIdentifier = TargetBufferUtil.GetBufferShaderProperty(TargetBuffer.Depth);

        private FilteringSettings m_FilteringSettings;
        public RenderOpaqueObjectPass(Settings settings)
        {
            m_FilteringSettings = new FilteringSettings(RenderQueueRange.opaque, settings.LayerMask);
        }

        [Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;
            var pixelartCamera = PixelartCamera.GetPixelartCamera(camera, PixelartCamera.CameraTarget.CastCamera);
            if (pixelartCamera == null) return;

            var cmd = CommandBufferPool.Get(k_PassTag);

            SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
            DrawingSettings drawingSettings = CreateDrawingSettings(TargetShaderPass, ref renderingData, sortingCriteria);

            using (new ProfilingScope(cmd, new ProfilingSampler(k_PassTag)))
            {
                cmd.SetRenderTarget(pixelartCamera.ResultTexture);
                cmd.ClearRenderTarget(false, true, camera.backgroundColor, 1);
                cmd.SetRenderTarget(OpaqueBuffersIdentifiers, DepthBufferIdentifier);
                cmd.ClearRenderTarget(true, true, Color.clear, 1);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings);

                for (int i = (int)TargetBufferStage.Start + 1; i <= (int)TargetBufferStage.MarkerRawData; i++)
                {
                    cmd.SetGlobalTexture(TargetBufferUtil.GetBufferShaderProperty((TargetBuffer)i), TargetBufferUtil.GetBufferShaderProperty((TargetBuffer)i));
                }
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

#if UNITY_6000_0_OR_NEWER
        private class PassData
        {
            internal PixelartCamera pixelartCamera;
            internal UniversalCameraData cameraData;
            internal RendererListHandle RendererList;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameContext)
        {
            UniversalResourceData resourceData = frameContext.Get<UniversalResourceData>();
            UniversalRenderingData renderingData = frameContext.Get<UniversalRenderingData>();
            UniversalCameraData cameraData = frameContext.Get<UniversalCameraData>();
            UniversalLightData lightData = frameContext.Get<UniversalLightData>();

            var camera = cameraData.camera;
            var pixelartCamera = PixelartCamera.GetPixelartCamera(camera, PixelartCamera.CameraTarget.CastCamera);
            if (pixelartCamera == null) return;

            using (var builder = renderGraph.AddRasterRenderPass<PassData>(k_PassTag, out var passData))
            {
                UniversalRenderer renderer = (UniversalRenderer)cameraData.renderer;

                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true);

                for (int i = (int)TargetBufferStage.MarkerDepth + 1; i <= (int)TargetBufferStage.MarkerRawData; i++)
                {
                    TargetBuffer target = (TargetBuffer)i;
                    int index = i - ((int)TargetBufferStage.MarkerDepth + 1);
                    TextureHandle bufferHandle = UniversalRenderer.CreateRenderGraphTexture(renderGraph, TargetBufferUtil.GetDescriptor(pixelartCamera.CameraData, target), TargetBufferUtil.GetBufferName(target), false);
                    builder.SetRenderAttachment(bufferHandle, index, AccessFlags.Write);
                    pixelartCamera.SetTextureHandle(target, bufferHandle);
                    builder.SetGlobalTextureAfterPass(bufferHandle, TargetBufferUtil.GetBufferShaderProperty(target));
                }

                TextureHandle depthHandle = UniversalRenderer.CreateRenderGraphTexture(renderGraph, TargetBufferUtil.GetDescriptor(pixelartCamera.CameraData, TargetBuffer.Depth), TargetBufferUtil.GetBufferName(TargetBuffer.Depth), false);
                builder.SetRenderAttachmentDepth(depthHandle, AccessFlags.Write);
                pixelartCamera.SetTextureHandle((int)TargetBufferStage.MarkerDepth, depthHandle);
                builder.SetGlobalTextureAfterPass(depthHandle, TargetBufferUtil.GetBufferShaderProperty(TargetBuffer.Depth));

                SortingCriteria sortingCriteria = cameraData.defaultOpaqueSortFlags;
                DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(TargetShaderPass, renderingData, cameraData, lightData, sortingCriteria);
                var param = new RendererListParams(renderingData.cullResults, drawingSettings, m_FilteringSettings);
                passData.RendererList = renderGraph.CreateRendererList(param);
                builder.UseRendererList(passData.RendererList);

                passData.pixelartCamera = pixelartCamera;
                passData.cameraData = cameraData;

                builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) =>
                {
                    ExecutePass(rgContext.cmd, data);
                });
            }
        }

        private static void ExecutePass(RasterCommandBuffer cmd, PassData passData)
        {
            using (new ProfilingScope(cmd, new ProfilingSampler(k_PassTag)))
            {
                float unitSize = passData.pixelartCamera.UnitSize;

                Matrix4x4 viewMatrix = passData.cameraData.GetViewMatrix();
                viewMatrix.m03 = Mathf.Round(viewMatrix.m03 / unitSize) * unitSize;
                viewMatrix.m13 = Mathf.Round(viewMatrix.m13 / unitSize) * unitSize;
                var proj = passData.cameraData.GetProjectionMatrix();
                var viewProjMat = proj * viewMatrix;

                cmd.SetGlobalMatrix(ShaderPropertyStorage.CameraViewMatrix, viewMatrix);
                cmd.SetGlobalMatrix(ShaderPropertyStorage.CameraInvViewMatrix, viewMatrix.inverse);
                cmd.SetGlobalMatrix(ShaderPropertyStorage.CameraViewProjectionMatrix, viewProjMat);
                cmd.SetGlobalMatrix(ShaderPropertyStorage.CameraInvViewProjectionMatrix, viewProjMat.inverse);

                cmd.SetGlobalFloat(ShaderPropertyStorage.UnitSize, unitSize);

                cmd.DrawRendererList(passData.RendererList);
            }
        }
#endif
    }
}
