#pragma once
#include <windows.h>
#include <GL/gl.h>
#include <GL/glu.h>
#include <vector>
#include <string>
#include <cmath>

#ifndef M_PI
#define M_PI 3.14159265358979323846
#endif

struct SceneObject {
    std::string type;
    float posX, posY, posZ;
    float rotX, rotY, rotZ;
    float scaleX, scaleY, scaleZ;
};

struct Camera {
    float posX, posY, posZ;
    float rotX, rotY; // Pitch, Yaw
    float fov;
    Camera() : posX(0), posY(2), posZ(8), rotX(-10), rotY(0), fov(60.0f) {}
};

class VertextureCore {
private:
    HWND viewportHandle;
    bool isRunning;
    float clearColorR, clearColorG, clearColorB;
    HDC  hDC;
    HGLRC hGLRC;

    Camera camera;
    std::vector<SceneObject> objects;

    bool SetupPixelFormat();
    void UpdateCamera();
    void DrawCube();
    void DrawSphere();
    void DrawLight();
    void DrawObject(const SceneObject& obj);

public:
    VertextureCore();
    ~VertextureCore();

    bool Initialize(HWND hwnd);
    void RenderFrame();
    void Shutdown();
    void SetClearColor(float r, float g, float b);
    void SetCameraPosition(float x, float y, float z);
    void SetCameraRotation(float pitch, float yaw);
    void SetCameraFOV(float fov);

    void BeginObjectBatch();
    void PushObject(const char* type, float px, float py, float pz,
                    float rx, float ry, float rz,
                    float sx, float sy, float sz);
    void EndObjectBatch();
};
