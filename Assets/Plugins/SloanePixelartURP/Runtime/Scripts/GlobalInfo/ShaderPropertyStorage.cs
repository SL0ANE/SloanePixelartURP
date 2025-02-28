using UnityEngine;

namespace Sloane.PixelartURP
{
    public static class ShaderPropertyStorage
    {
        public static readonly int ViewMatrix = Shader.PropertyToID("unity_MatrixV");
        public static readonly int InvViewMatrix = Shader.PropertyToID("unity_MatrixInvV");
        public static readonly int CameraViewMatrix = Shader.PropertyToID("_CameraMatrixV");
        public static readonly int CameraInvViewMatrix = Shader.PropertyToID("_CameraMatrixInvV");
        public static readonly int CameraViewProjectionMatrix = Shader.PropertyToID("_CameraMatrixVP");
        public static readonly int CameraInvViewProjectionMatrix = Shader.PropertyToID("_CameraMatrixInvVP");
        public static readonly int UnitSize = Shader.PropertyToID("_UnitSize");
    }
}