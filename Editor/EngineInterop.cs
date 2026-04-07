using System;
using System.Runtime.InteropServices;

namespace VertextureEditor
{
    public static class EngineInterop
    {
        private const string DllName = "VertextureCore.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool InitEngine(IntPtr viewportHandle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void RenderFrame();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ShutdownEngine();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetClearColor(float r, float g, float b);

        // Camera
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCameraPosition(float x, float y, float z);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCameraRotation(float pitch, float yaw);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCameraFOV(float fov);

        // Object batching
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void BeginObjectBatch();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void PushObject(
            [MarshalAs(UnmanagedType.LPStr)] string type,
            float posX, float posY, float posZ,
            float rotX, float rotY, float rotZ,
            float scaleX, float scaleY, float scaleZ);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void EndObjectBatch();
    }
}
