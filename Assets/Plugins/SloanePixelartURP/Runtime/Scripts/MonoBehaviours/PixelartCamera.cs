using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Sloane.PixelartURP
{
    [Serializable]
    public struct PixelartCameraData
    {
        public static readonly PixelartCameraData Default = new PixelartCameraData
        {
            TargetResolution = new Vector2Int(320, 180),
            DownSamplingScale = 2,
        };

        public Vector2Int SourceResolution => TargetResolution * DownSamplingRate;
        public int DownSamplingRate => DownSamplingScale * 2 + 1;    // 实际渲染比率
        public Vector2Int TargetResolution;
        [Range(1, 4)]
        public int DownSamplingScale;
    }

    [ExecuteAlways]
    public class PixelartCamera : MonoBehaviour
    {
        protected static Dictionary<Camera, PixelartCamera> m_CameraMap = new Dictionary<Camera, PixelartCamera>();
        public static Dictionary<Camera, PixelartCamera> CameraMap => m_CameraMap;
        protected static Dictionary<Camera, PixelartCamera> m_CastCameraMap = new Dictionary<Camera, PixelartCamera>();
        public static Dictionary<Camera, PixelartCamera> CastCameraMap => m_CastCameraMap;

        [SerializeField]
        private PixelartCameraData m_CameraData = PixelartCameraData.Default;
        public PixelartCameraData CameraData => m_CameraData;

        [SerializeField, HideInInspector]
        private Camera m_ThisCamera;
        [SerializeField, HideInInspector]
        private UniversalAdditionalCameraData m_ThisCameraURPData;
        [SerializeField, HideInInspector]
        private PixelartCastCamera m_PixelartCastCamera;
        private UniversalAdditionalCameraData CastCameraURPData => m_PixelartCastCamera.CameraURPData;
        private Camera CastCamera => m_PixelartCastCamera.Camera;
        [SerializeField]
        private RenderTexture m_ResultTexture;
        private RTHandle m_ResultHandle;
        public RenderTexture ResultTexture => m_ResultTexture;
        public RTHandle ResultHandle => m_ResultHandle;
        public float UnitSize => CastCamera.orthographicSize * 2.0f / CameraData.TargetResolution.y;

#if UNITY_6000_0_OR_NEWER
        private TextureHandle[] m_TargetBufferHandles = new TextureHandle[(int)TargetBuffer.Max];
        public TextureHandle GetTextureHandle(TargetBuffer target)
        {
            return m_TargetBufferHandles[(int)target];
        }

        public void SetTextureHandle(TargetBuffer target, TextureHandle handle)
        {
            m_TargetBufferHandles[(int)target] = handle;
        }
#endif

        public enum CameraTarget
        {
            MainCamera,
            CastCamera
        }

        public static PixelartCamera GetPixelartCamera(Camera camera, CameraTarget target = CameraTarget.MainCamera)
        {
            var pixelartCamera = target == CameraTarget.MainCamera ? (CameraMap.ContainsKey(camera) ? CameraMap[camera] : null) : (CastCameraMap.ContainsKey(camera) ? CastCameraMap[camera] : null);

            if (pixelartCamera == null)
            {
                if (target == CameraTarget.MainCamera)
                {
                    pixelartCamera = camera.gameObject.GetComponent<PixelartCamera>();
                }
                else
                {
                    pixelartCamera = camera.gameObject.GetComponent<PixelartCastCamera>()?.ParentCamera;
                }

                if (pixelartCamera != null) pixelartCamera.RegisterCamera();
            }

            return pixelartCamera;
        }

        private void Awake()
        {
            Initialize();

            RegisterCamera();
        }

        private void RegisterCamera()
        {
            m_CameraMap.Add(m_ThisCamera, this);
            m_CastCameraMap.Add(CastCamera, this);
        }

        private void OnDestroy()
        {
            m_CameraMap.Remove(m_ThisCamera);
            m_CastCameraMap.Remove(CastCamera);

            if (CastCamera != null) DestroyImmediate(m_PixelartCastCamera.gameObject);
            ReleaseResultRenderTexture();
        }
        private void Initialize()
        {
            InitializeThisCamera();
            InitializeCastCamera();
            InitializeResultRenderTexture();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            InitializeResultRenderTexture();
        }
#endif

        private void InitializeResultRenderTexture()
        {
            ReleaseResultRenderTexture();

            Vector2Int resolution = m_CameraData.TargetResolution;
            var desc = new RenderTextureDescriptor(resolution.x, resolution.y)
            {
                depthBufferBits = 24,
                enableRandomWrite = true,
                graphicsFormat = SystemInfo.GetGraphicsFormat(DefaultFormat.HDR),
                sRGB = false,
                volumeDepth = 1,
                msaaSamples = 1,
                dimension = TextureDimension.Tex2D
            };

            m_ResultTexture = new RenderTexture(desc)
            {
                name = "Pixelart Result Texture",
            };
            m_ResultTexture.Create();

            m_ResultHandle = RTHandles.Alloc(m_ResultTexture);

            CastCamera.targetTexture = m_ResultTexture;
        }

        private void ReleaseResultRenderTexture()
        {
            if (m_ResultTexture != null)
            {
                if(CastCamera != null) CastCamera.targetTexture = null;
                m_ResultTexture.Release();
                DestroyImmediate(m_ResultTexture);
                m_ResultTexture = null;
            }

            if (m_ResultHandle != null)
            {
                m_ResultHandle.Release();
                m_ResultHandle = null;
            }
        }

        private void InitializeThisCamera()
        {
            if (m_ThisCamera == null)
            {
                m_ThisCamera = GetComponent<Camera>();
                m_ThisCameraURPData = m_ThisCamera.GetUniversalAdditionalCameraData();
            }

            m_ThisCamera.clearFlags = CameraClearFlags.Nothing;
            m_ThisCamera.cullingMask = 0;
            m_ThisCamera.farClipPlane = 0.02f;
            m_ThisCamera.nearClipPlane = 0.01f;

            m_ThisCameraURPData.SetRenderer((int)PixelartRenderer.ResultCamera);
        }

        private void InitializeCastCamera()
        {
            if (m_PixelartCastCamera == null)
            {
                GameObject castCameraObject = new GameObject("Cast Camera");
                castCameraObject.transform.SetParent(transform, false);
                castCameraObject.AddComponent<Camera>();
                m_PixelartCastCamera = castCameraObject.AddComponent<PixelartCastCamera>();
                m_PixelartCastCamera.Initialize(this);
                CastCamera.orthographicSize = 6.125f;    // Celeste
            }

            CastCamera.orthographic = true;
            CastCamera.depth = -64;

            CastCameraURPData.SetRenderer((int)PixelartRenderer.CastCamera);
        }
    }
}
