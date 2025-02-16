using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Sloane.PixelartURP
{
    public enum TargetBuffer
    {
        Depth = 0,
        Albedo = 1,
        Normal = 2,    // 根据顶点法线和法线贴图计算出的法线
        NormalExact = 3,    // 根据位置变化率计算出的法线
        EmissiveProperty = 4,    // 自发光颜色与强度
        SpecularProperty = 5,    // 高光颜色与强度
        PhysicalProperty = 6,   // 金属度，光滑度，遮蔽度
        ShapeProperty = 7,    // 优先级，法线边缘阈值
        PaletteProperty = 8,    // 主光源级数，dither灰度, 边缘增减级数, 布尔信息（0：是否应用描边）
        UV = 9,    // 根据优先级整出的UV偏移
        ConnectivityDetail = 10,
        ConnectivityResult = 11,
        Diffuse = 12,
        Specular = 13,
        GlobalIllumination = 14,
        RimLight = 15,
        Max,
    }

    // 记录每个阶段的结尾Buffer索引
    public enum TargetBufferStage
    {
        Start = -1,
        MarkerDepth = 0,
        MarkerRawData = 8,    // 多个渲染目标得到的原始数据
        MarkerPriority = 9,    // 优先级
        MarkerConnectionDetail = 10,
        MarkerConnectionResult = 11,
        MarkerShading = 15,
        MarkerResult = 16,
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
                case TargetBuffer.Albedo:
                case TargetBuffer.Normal:
                case TargetBuffer.NormalExact:
                case TargetBuffer.EmissiveProperty:
                case TargetBuffer.SpecularProperty:
                case TargetBuffer.PhysicalProperty:
                case TargetBuffer.ShapeProperty:
                case TargetBuffer.PaletteProperty:
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
                case TargetBuffer.GlobalIllumination:
                case TargetBuffer.RimLight:
                    return new RenderTextureDescriptor(targetResolution.x, targetResolution.y)
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