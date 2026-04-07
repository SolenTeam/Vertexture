#include "Core.h"

VertextureCore* g_Engine = nullptr;

extern "C" {

    __declspec(dllexport) bool InitEngine(HWND viewportHandle) {
        if (g_Engine == nullptr) g_Engine = new VertextureCore();
        return g_Engine->Initialize(viewportHandle);
    }

    __declspec(dllexport) void RenderFrame() {
        if (g_Engine != nullptr) g_Engine->RenderFrame();
    }

    __declspec(dllexport) void ShutdownEngine() {
        if (g_Engine != nullptr) {
            g_Engine->Shutdown();
            delete g_Engine;
            g_Engine = nullptr;
        }
    }

    __declspec(dllexport) void SetClearColor(float r, float g, float b) {
        if (g_Engine != nullptr) g_Engine->SetClearColor(r, g, b);
    }

    __declspec(dllexport) void SetCameraPosition(float x, float y, float z) {
        if (g_Engine != nullptr) g_Engine->SetCameraPosition(x, y, z);
    }

    __declspec(dllexport) void SetCameraRotation(float pitch, float yaw) {
        if (g_Engine != nullptr) g_Engine->SetCameraRotation(pitch, yaw);
    }

    __declspec(dllexport) void SetCameraFOV(float fov) {
        if (g_Engine != nullptr) g_Engine->SetCameraFOV(fov);
    }

    __declspec(dllexport) void BeginObjectBatch() {
        if (g_Engine != nullptr) g_Engine->BeginObjectBatch();
    }

    __declspec(dllexport) void PushObject(
        const char* type,
        float posX, float posY, float posZ,
        float rotX, float rotY, float rotZ,
        float scaleX, float scaleY, float scaleZ) {
        if (g_Engine != nullptr)
            g_Engine->PushObject(type, posX, posY, posZ, rotX, rotY, rotZ, scaleX, scaleY, scaleZ);
    }

    __declspec(dllexport) void EndObjectBatch() {
        if (g_Engine != nullptr) g_Engine->EndObjectBatch();
    }

}
