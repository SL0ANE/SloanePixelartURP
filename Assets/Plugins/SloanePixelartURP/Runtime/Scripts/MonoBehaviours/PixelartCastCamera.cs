using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Sloane.PixelartURP
{
    [ExecuteAlways]
    public class PixelartCastCamera : MonoBehaviour
    {
        [SerializeField]
        private PixelartCamera m_ParentCamera;
        public PixelartCamera ParentCamera => m_ParentCamera;
        [SerializeField, HideInInspector]
        private Camera m_Camera;
        public Camera Camera => m_Camera;
        [SerializeField, HideInInspector]
        private UniversalAdditionalCameraData m_CameraURPData;
        public UniversalAdditionalCameraData CameraURPData => m_CameraURPData;

        public void Initialize(PixelartCamera parentCamera)
        {
            m_ParentCamera = parentCamera;
            m_Camera = GetComponent<Camera>();
            m_CameraURPData = m_Camera.GetUniversalAdditionalCameraData();
        }
    }
}