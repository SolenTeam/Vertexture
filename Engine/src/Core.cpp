#include "Core.h"
#include <algorithm>

VertextureCore::VertextureCore()
    : viewportHandle(nullptr), isRunning(false),
      clearColorR(0.1f), clearColorG(0.1f), clearColorB(0.15f),
      hDC(nullptr), hGLRC(nullptr) {}

VertextureCore::~VertextureCore() { Shutdown(); }

bool VertextureCore::SetupPixelFormat() {
    PIXELFORMATDESCRIPTOR pfd = { 0 };
    pfd.nSize      = sizeof(pfd);
    pfd.nVersion   = 1;
    pfd.dwFlags    = PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER;
    pfd.iPixelType = PFD_TYPE_RGBA;
    pfd.cColorBits = 32;
    pfd.cDepthBits = 24;
    pfd.cStencilBits = 8;

    int pixelFormat = ChoosePixelFormat(hDC, &pfd);
    if (pixelFormat == 0) return false;
    if (!SetPixelFormat(hDC, pixelFormat, &pfd)) return false;

    hGLRC = wglCreateContext(hDC);
    if (hGLRC == nullptr) return false;
    if (!wglMakeCurrent(hDC, hGLRC)) return false;

    glEnable(GL_DEPTH_TEST);
    glEnable(GL_CULL_FACE);
    glCullFace(GL_BACK);
    glEnable(GL_COLOR_MATERIAL);
    glColorMaterial(GL_FRONT_AND_BACK, GL_AMBIENT_AND_DIFFUSE);

    // Simple lighting
    glEnable(GL_LIGHTING);
    glEnable(GL_LIGHT0);
    float lightPos[] = { 5.0f, 10.0f, 7.0f, 1.0f };
    glLightfv(GL_LIGHT0, GL_POSITION, lightPos);
    float lightAmbient[] = { 0.3f, 0.3f, 0.3f, 1.0f };
    glLightfv(GL_LIGHT0, GL_AMBIENT, lightAmbient);
    float lightDiffuse[] = { 0.8f, 0.8f, 0.8f, 1.0f };
    glLightfv(GL_LIGHT0, GL_DIFFUSE, lightDiffuse);

    RECT rect;
    GetClientRect(viewportHandle, &rect);
    int w = rect.right - rect.left;
    int h = rect.bottom - rect.top;
    if (w > 0 && h > 0) glViewport(0, 0, w, h);

    return true;
}

bool VertextureCore::Initialize(HWND hwnd) {
    viewportHandle = hwnd;
    hDC = GetDC(hwnd);
    if (!hDC) return false;
    if (!SetupPixelFormat()) return false;
    isRunning = true;
    return true;
}

void VertextureCore::UpdateCamera() {
    RECT rect;
    GetClientRect(viewportHandle, &rect);
    int width  = rect.right - rect.left;
    int height = rect.bottom - rect.top;
    if (width <= 0 || height <= 0) return;

    glViewport(0, 0, width, height);

    glMatrixMode(GL_PROJECTION);
    glLoadIdentity();
    float aspect = (float)width / (float)height;
    float fH = std::tanf(camera.fov * (float)M_PI / 360.0f) * 1.0f;
    float fW = fH * aspect;
    glFrustum(-fW, fW, -fH, fH, 0.1, 1000.0);

    glMatrixMode(GL_MODELVIEW);
    glLoadIdentity();

    float pitchRad = camera.rotX * (float)M_PI / 180.0f;
    float yawRad   = camera.rotY * (float)M_PI / 180.0f;
    float fx = std::cosf(pitchRad) * std::sinf(yawRad);
    float fy = std::sinf(pitchRad);
    float fz = std::cosf(pitchRad) * std::cosf(yawRad);

    gluLookAt(camera.posX, camera.posY, camera.posZ,
              camera.posX + fx, camera.posY + fy, camera.posZ + fz,
              0.0f, 1.0f, 0.0f);
}

void VertextureCore::DrawCube() {
    glBegin(GL_QUADS);
    glNormal3f(0, 0, 1);
    glVertex3f(-1, -1,  1); glVertex3f( 1, -1,  1);
    glVertex3f( 1,  1,  1); glVertex3f(-1,  1,  1);
    glNormal3f(0, 0, -1);
    glVertex3f(-1, -1, -1); glVertex3f(-1,  1, -1);
    glVertex3f( 1,  1, -1); glVertex3f( 1, -1, -1);
    glNormal3f(-1, 0, 0);
    glVertex3f(-1, -1, -1); glVertex3f(-1, -1,  1);
    glVertex3f(-1,  1,  1); glVertex3f(-1,  1, -1);
    glNormal3f(1, 0, 0);
    glVertex3f( 1, -1,  1); glVertex3f( 1, -1, -1);
    glVertex3f( 1,  1, -1); glVertex3f( 1,  1,  1);
    glNormal3f(0, 1, 0);
    glVertex3f(-1,  1,  1); glVertex3f( 1,  1,  1);
    glVertex3f( 1,  1, -1); glVertex3f(-1,  1, -1);
    glNormal3f(0, -1, 0);
    glVertex3f(-1, -1, -1); glVertex3f( 1, -1, -1);
    glVertex3f( 1, -1,  1); glVertex3f(-1, -1,  1);
    glEnd();
}

void VertextureCore::DrawSphere() {
    GLUquadric* quad = gluNewQuadric();
    gluSphere(quad, 1.0, 24, 16);
    gluDeleteQuadric(quad);
}

void VertextureCore::DrawLight() {
    // Octahedron for light
    glBegin(GL_TRIANGLES);
    glNormal3f(0, 1, 0);
    glVertex3f(0, 1, 0); glVertex3f(-1, 0, 0); glVertex3f(0, 0, 1);
    glVertex3f(0, 1, 0); glVertex3f(0, 0, 1); glVertex3f(1, 0, 0);
    glVertex3f(0, 1, 0); glVertex3f(1, 0, 0); glVertex3f(0, 0, -1);
    glVertex3f(0, 1, 0); glVertex3f(0, 0, -1); glVertex3f(-1, 0, 0);
    glNormal3f(0, -1, 0);
    glVertex3f(0, -1, 0); glVertex3f(0, 0, 1); glVertex3f(-1, 0, 0);
    glVertex3f(0, -1, 0); glVertex3f(1, 0, 0); glVertex3f(0, 0, 1);
    glVertex3f(0, -1, 0); glVertex3f(0, 0, -1); glVertex3f(1, 0, 0);
    glVertex3f(0, -1, 0); glVertex3f(-1, 0, 0); glVertex3f(0, 0, -1);
    glEnd();
}

void VertextureCore::DrawObject(const SceneObject& obj) {
    glPushMatrix();
    glTranslatef(obj.posX, obj.posY, obj.posZ);
    glRotatef(obj.rotY, 0, 1, 0);
    glRotatef(obj.rotX, 1, 0, 0);
    glRotatef(obj.rotZ, 0, 0, 1);
    glScalef(obj.scaleX, obj.scaleY, obj.scaleZ);

    if (obj.type == "cube") {
        glColor3f(0.3f, 0.6f, 1.0f);
        DrawCube();
    } else if (obj.type == "sphere") {
        glColor3f(1.0f, 0.5f, 0.3f);
        DrawSphere();
    } else if (obj.type == "light") {
        glColor3f(1.0f, 1.0f, 0.4f);
        DrawLight();
    }

    glPopMatrix();
}

void VertextureCore::RenderFrame() {
    if (!isRunning) return;

    UpdateCamera();

    glClearColor(clearColorR, clearColorG, clearColorB, 1.0f);
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

    // Draw ground grid (no lighting)
    glDisable(GL_LIGHTING);
    glColor3f(0.25f, 0.25f, 0.3f);
    glBegin(GL_LINES);
    for (int i = -10; i <= 10; i++) {
        glVertex3f((float)i, 0, -10.0f); glVertex3f((float)i, 0, 10.0f);
        glVertex3f(-10.0f, 0, (float)i); glVertex3f(10.0f, 0, (float)i);
    }
    glEnd();
    glEnable(GL_LIGHTING);

    // Draw all objects
    for (const auto& obj : objects) {
        DrawObject(obj);
    }

    // Draw axes
    glDisable(GL_LIGHTING);
    glBegin(GL_LINES);
    glColor3f(1, 0, 0);
    glVertex3f(0, 0, 0); glVertex3f(3, 0, 0);
    glColor3f(0, 1, 0);
    glVertex3f(0, 0, 0); glVertex3f(0, 3, 0);
    glColor3f(0, 0, 1);
    glVertex3f(0, 0, 0); glVertex3f(0, 0, 3);
    glEnd();
    glEnable(GL_LIGHTING);

    SwapBuffers(hDC);
}

void VertextureCore::Shutdown() {
    isRunning = false;
    if (hGLRC) { wglMakeCurrent(nullptr, nullptr); wglDeleteContext(hGLRC); hGLRC = nullptr; }
    if (hDC) { ReleaseDC(viewportHandle, hDC); hDC = nullptr; }
    viewportHandle = nullptr;
}

void VertextureCore::SetClearColor(float r, float g, float b) {
    clearColorR = r; clearColorG = g; clearColorB = b;
}
void VertextureCore::SetCameraPosition(float x, float y, float z) {
    camera.posX = x; camera.posY = y; camera.posZ = z;
}
void VertextureCore::SetCameraRotation(float pitch, float yaw) {
    camera.rotX = pitch; camera.rotY = yaw;
}
void VertextureCore::SetCameraFOV(float fov) {
    camera.fov = std::clamp(fov, 10.0f, 170.0f);
}

void VertextureCore::BeginObjectBatch() {
    objects.clear();
}

void VertextureCore::PushObject(const char* type, float px, float py, float pz,
                                 float rx, float ry, float rz,
                                 float sx, float sy, float sz) {
    SceneObject obj;
    obj.type = type;
    obj.posX = px; obj.posY = py; obj.posZ = pz;
    obj.rotX = rx; obj.rotY = ry; obj.rotZ = rz;
    obj.scaleX = sx; obj.scaleY = sy; obj.scaleZ = sz;
    objects.push_back(obj);
}

void VertextureCore::EndObjectBatch() {
    // Objects are now ready for rendering
}
