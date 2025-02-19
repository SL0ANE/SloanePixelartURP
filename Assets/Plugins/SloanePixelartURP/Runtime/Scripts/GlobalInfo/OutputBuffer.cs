using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Sloane.PixelartURP
{
    // 部分参考了URP延迟渲染的组织
    // https://docs.unity3d.com/Manual/RenderTech-DeferredShading.html
    public enum TargetBuffer
    {
        Depth = 0,
        AlbedoProperty = 1,    // 漫反射颜色, 环境光遮蔽
        SpecularProperty = 2,    // 高光颜色, 光滑度
        LightingProperty = 3,    // 全局光照以及自发光，着色索引
        MiscProperty = 4,    // RGBA32，每个数据占8位，理论上能记录16组数据。默认着色器的分配是：优先级，主光源级数，dither灰度, 法线边缘阈值, 边缘增减级数, 布尔信息（0：是否应用描边）
        Normal = 5,    // 根据顶点法线和法线贴图计算出的法线
        NormalExact = 6,    // 根据位置变化率计算出的法线
        UV = 7,    // 根据优先级整出的UV偏移
        ConnectivityDetail = 8,
        ConnectivityResult = 9,
        Diffuse = 10,
        Specular = 11,
        RimLight = 12,
        Max,
    }

    // 记录每个阶段的结尾Buffer索引
    public enum TargetBufferStage
    {
        Start = -1,
        MarkerDepth = 0,
        MarkerRawData = 6,    // 多个渲染目标得到的原始数据
        MarkerPriority = 7,    // 优先级
        MarkerConnectionDetail = 8,
        MarkerConnectionResult = 9,
        MarkerShading = 12,
        Max,
    }

    public static class TargetBufferUtil
    {
        private static List<string> m_TargetBufferName;
        private static List<int> m_TargetBufferShaderProperty;
        private static bool m_Initialize = false;

        private static void Initialize()
        {
            if (m_Initialize) return;
            m_Initialize = true;

            m_TargetBufferName = new List<string>();

            m_TargetBufferShaderProperty = new List<int>();
            for (int i = 0; i < (int)TargetBuffer.Max; i++)
            {
                string enumName = Enum.GetName(typeof(TargetBuffer), (TargetBuffer)i);
                m_TargetBufferName.Add(enumName);
                m_TargetBufferShaderProperty.Add(Shader.PropertyToID($"_{enumName}Buffer"));
                // Debug.Log($"_{enumName}Buffer");
            }
        }

        public static string GetBufferName(TargetBuffer targetBuffer)
        {
            Initialize();
            return m_TargetBufferName[(int)targetBuffer];
        }

        public static int GetBufferShaderProperty(TargetBuffer targetBuffer)
        {
            Initialize();
            return m_TargetBufferShaderProperty[(int)targetBuffer];
        }

        public static RenderTextureDescriptor GetDescriptor(PixelartCameraData cameraData, TargetBuffer targetBuffer)
        {
            var sourceResolution = cameraData.SourceResolution;
            var targetResolution = cameraData.TargetResolution;
            switch (targetBuffer)
            {
                case TargetBuffer.Depth:
                    return new RenderTextureDescriptor(sourceResolution.x, sourceResolution.y)
                    {
                        depthBufferBits = 24,
                        graphicsFormat = GraphicsFormat.None,
                        volumeDepth = 1,
                        msaaSamples = 1,
                        dimension = TextureDimension.Tex2D
                    };
                case TargetBuffer.AlbedoProperty:
                case TargetBuffer.SpecularProperty:
                case TargetBuffer.LightingProperty:
                case TargetBuffer.Normal:
                case TargetBuffer.NormalExact:
                    return new RenderTextureDescriptor(sourceResolution.x, sourceResolution.y)
                    {
                        depthBufferBits = 0,
                        enableRandomWrite = true,
                        graphicsFormat = GraphicsFormat.R16G16B16A16_SNorm,
                        volumeDepth = 1,
                        msaaSamples = 1,
                        sRGB = true,
                        dimension = TextureDimension.Tex2D
                    };
                case TargetBuffer.MiscProperty:
                    return new RenderTextureDescriptor(sourceResolution.x, sourceResolution.y)
                    {
                        depthBufferBits = 0,
                        enableRandomWrite = true,
                        graphicsFormat = GraphicsFormat.R32G32B32A32_SFloat,
                        volumeDepth = 1,
                        msaaSamples = 1,
                        sRGB = true,
                        dimension = TextureDimension.Tex2D
                    };
                case TargetBuffer.UV:
                    return new RenderTextureDescriptor(targetResolution.x, targetResolution.y)
                    {
                        depthBufferBits = 0,
                        enableRandomWrite = true,
                        graphicsFormat = GraphicsFormat.R8G8B8A8_SNorm,
                        volumeDepth = 1,
                        msaaSamples = 1,
                        dimension = TextureDimension.Tex2D
                    };
                case TargetBuffer.ConnectivityDetail:
                    return new RenderTextureDescriptor(sourceResolution.x, sourceResolution.y)
                    {
                        depthBufferBits = 0,
                        enableRandomWrite = true,
                        graphicsFormat = GraphicsFormat.R8G8B8A8_SNorm,
                        volumeDepth = 1,
                        msaaSamples = 1,
                        dimension = TextureDimension.Tex2D
                    };
                case TargetBuffer.ConnectivityResult:
                    return new RenderTextureDescriptor(targetResolution.x, targetResolution.y)
                    {
                        depthBufferBits = 0,
                        enableRandomWrite = true,
                        graphicsFormat = GraphicsFormat.R8G8B8A8_SNorm,
                        volumeDepth = 1,
                        msaaSamples = 1,
                        dimension = TextureDimension.Tex2D
                    };
                case TargetBuffer.Diffuse:
                case TargetBuffer.Specular:
                case TargetBuffer.RimLight:
                    return new RenderTextureDescriptor(sourceResolution.x, sourceResolution.y)
                    {
                        depthBufferBits = 0,
                        enableRandomWrite = true,
                        graphicsFormat = GraphicsFormat.R16G16B16A16_SNorm,
                        volumeDepth = 1,
                        msaaSamples = 1,
                        dimension = TextureDimension.Tex2D
                    };
            }

            return new RenderTextureDescriptor();
        }
    }
}