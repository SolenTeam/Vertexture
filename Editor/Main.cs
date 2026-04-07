using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace VertextureEditor
{
    public class MainForm : Form
    {
        private Panel viewportPanel = null!;
        private System.Windows.Forms.Timer? renderTimer;
        private SplitContainer mainSplit = null!;
        private SplitContainer rightSplit = null!;
        private ExplorerPanel explorerPanel = null!;
        private PropertiesPanel propertiesPanel = null!;
        private GameObject? selectedObject;
        private CameraController camera;
        private Stopwatch clock = new();

        // Input state
        private bool isMouseOrbiting = false;
        private Point lastMousePos;

        public MainForm()
        {
            camera = new CameraController();

            this.Text = "Vertexture Editor - v0.1";
            this.Size = new Size(1400, 800);
            this.MinimumSize = new Size(800, 500);

            // Toolbar
            Panel toolbarPanel = new Panel();
            toolbarPanel.Height = 40;
            toolbarPanel.Dock = DockStyle.Top;
            toolbarPanel.BackColor = Color.FromArgb(45, 45, 48);

            Button colorButton = new Button();
            colorButton.Text = "Clear Color";
            colorButton.ForeColor = Color.White;
            colorButton.BackColor = Color.FromArgb(55, 55, 60);
            colorButton.FlatStyle = FlatStyle.Flat;
            colorButton.Width = 120;
            colorButton.Height = 26;
            colorButton.Location = new Point(8, 7);
            colorButton.Click += ColorButton_Click;
            toolbarPanel.Controls.Add(colorButton);

            // Camera info label
            Label camLabel = new Label();
            camLabel.Name = "camLabel";
            camLabel.Text = "WASD: Move | Right-click: Orbit | Scroll: Zoom | Space/Ctrl: Up/Down";
            camLabel.ForeColor = Color.FromArgb(150, 150, 150);
            camLabel.Font = new Font("Segoe UI", 8f, FontStyle.Regular);
            camLabel.AutoSize = true;
            camLabel.Location = new Point(150, 12);
            toolbarPanel.Controls.Add(camLabel);

            // Main split: viewport | right sidebar
            mainSplit = new SplitContainer();
            mainSplit.Dock = DockStyle.Fill;
            mainSplit.SplitterDistance = 1080;
            mainSplit.SplitterWidth = 6;
            mainSplit.BackColor = Color.FromArgb(20, 20, 22);
            mainSplit.Panel1.BackColor = Color.Black;
            mainSplit.Panel1.Padding = new Padding(0);
            mainSplit.Panel2.BackColor = Color.FromArgb(42, 42, 46);
            mainSplit.Panel2.Padding = new Padding(0);
            mainSplit.BorderStyle = BorderStyle.FixedSingle;

            // Right sidebar split: explorer (top) | properties (bottom)
            rightSplit = new SplitContainer();
            rightSplit.Dock = DockStyle.Fill;
            rightSplit.Orientation = Orientation.Horizontal;
            rightSplit.SplitterDistance = 260;
            rightSplit.SplitterWidth = 4;
            rightSplit.BackColor = Color.FromArgb(35, 35, 38);
            rightSplit.Panel1.BackColor = Color.FromArgb(35, 35, 38);
            rightSplit.Panel2.BackColor = Color.FromArgb(35, 35, 38);

            // Explorer
            explorerPanel = new ExplorerPanel();
            explorerPanel.ObjectSelected += ExplorerPanel_ObjectSelected;
            explorerPanel.ObjectAdded += ExplorerPanel_ObjectAdded;
            explorerPanel.ObjectDeleted += ExplorerPanel_ObjectDeleted;
            rightSplit.Panel1.Controls.Add(explorerPanel);

            // Properties
            propertiesPanel = new PropertiesPanel();
            rightSplit.Panel2.Controls.Add(propertiesPanel);

            mainSplit.Panel2.Controls.Add(rightSplit);

            // Viewport
            viewportPanel = new Panel();
            viewportPanel.BackColor = Color.Black;
            viewportPanel.Dock = DockStyle.Fill;
            viewportPanel.TabStop = true;

            viewportPanel.MouseDown += Viewport_MouseDown;
            viewportPanel.MouseMove += Viewport_MouseMove;
            viewportPanel.MouseUp += Viewport_MouseUp;
            viewportPanel.MouseWheel += Viewport_MouseWheel;
            viewportPanel.KeyDown += Viewport_KeyDown;
            viewportPanel.KeyUp += Viewport_KeyUp;

            mainSplit.Panel1.Controls.Add(viewportPanel);

            this.Controls.Add(mainSplit);
            this.Controls.Add(toolbarPanel);

            this.Load += MainForm_Load;
            this.FormClosing += MainForm_FormClosing;
            this.mainSplit.SplitterMoved += MainSplit_SplitterMoved;
        }

        // ===== INPUT =====

        private void Viewport_KeyDown(object? sender, KeyEventArgs e)
        {
            camera.HandleKeyDown(e.KeyCode);
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.A || e.KeyCode == Keys.S ||
                e.KeyCode == Keys.D || e.KeyCode == Keys.Space || e.KeyCode == Keys.ControlKey)
            {
                e.SuppressKeyPress = true; // prevent beep
            }
        }

        private void Viewport_KeyUp(object? sender, KeyEventArgs e)
        {
            camera.HandleKeyUp(e.KeyCode);
        }

        private void Viewport_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                isMouseOrbiting = true;
                lastMousePos = e.Location;
                viewportPanel.Focus();
            }
        }

        private void Viewport_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isMouseOrbiting)
            {
                float dx = e.X - lastMousePos.X;
                float dy = e.Y - lastMousePos.Y;
                camera.Yaw += dx * 0.25f;
                camera.Pitch -= dy * 0.25f;
                camera.Pitch = Math.Clamp(camera.Pitch, -89.0f, 89.0f);
                lastMousePos = e.Location;
            }
        }

        private void Viewport_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) isMouseOrbiting = false;
        }

        private void Viewport_MouseWheel(object? sender, MouseEventArgs e)
        {
            // Scroll moves camera forward/backward along its facing direction
            float yawRad = camera.Yaw * MathF.PI / 180f;
            float pitchRad = camera.Pitch * MathF.PI / 180f;

            float fwdX = MathF.Cos(pitchRad) * MathF.Sin(yawRad);
            float fwdZ = MathF.Cos(pitchRad) * MathF.Cos(yawRad);
            float fwdY = MathF.Sin(pitchRad);

            float dist = e.Delta * 0.01f;
            camera.PositionX += fwdX * dist;
            camera.PositionY += fwdY * dist;
            camera.PositionZ += fwdZ * dist;
        }

        // ===== EXPLORER EVENTS =====

        private void ExplorerPanel_ObjectAdded(string type)
        {
            // Position in front of camera
            float dist = 4f;
            float yawRad = camera.Yaw * MathF.PI / 180f;
            float pitchRad = camera.Pitch * MathF.PI / 180f;

            float fx = MathF.Sin(yawRad) * MathF.Cos(pitchRad);
            float fy = MathF.Sin(pitchRad);
            float fz = MathF.Cos(yawRad) * MathF.Cos(pitchRad);

            float px = camera.PositionX + fx * dist;
            float py = camera.PositionY + fy * dist;
            float pz = camera.PositionZ + fz * dist;

            string name = type switch
            {
                "cube" => $"Cube_{Scene.Objects.Count(o => o.Type == "cube") + 1}",
                "sphere" => $"Sphere_{Scene.Objects.Count(o => o.Type == "sphere") + 1}",
                "light" => $"Light_{Scene.Objects.Count(o => o.Type == "light") + 1}",
                _ => $"Object_{Scene.Objects.Count + 1}"
            };

            var go = Scene.CreateObject(name, type);
            go.PositionX = px;
            go.PositionY = py;
            go.PositionZ = pz;

            explorerPanel.AddNode(go.DisplayName, go.Id, type);
            explorerPanel.SelectNode(go.DisplayName);
            SelectObject(go);
        }

        private void ExplorerPanel_ObjectDeleted(string name, int id)
        {
            var obj = Scene.GetById(id);
            if (obj != null)
            {
                Scene.RemoveObject(obj);
                if (selectedObject == obj)
                {
                    selectedObject = null;
                    _onPropertyChanged = null;
                    propertiesPanel.ShowProperties(null, _onPropertyChanged!);
                }
            }
        }

        private void ExplorerPanel_ObjectSelected(string name, int id)
        {
            var obj = Scene.GetById(id);
            SelectObject(obj);
        }

        private void SelectObject(GameObject? obj)
        {
            selectedObject = obj;
            _onPropertyChanged = OnObjectPropertyChanged;
            propertiesPanel.ShowProperties(obj, _onPropertyChanged);
        }

        private void OnObjectPropertyChanged(GameObject obj)
        {
            // Refresh explorer node name
            if (explorerPanel.SelectedNode?.Tag is int nodeId)
            {
                if (Scene.GetById(nodeId) == obj)
                {
                    explorerPanel.RenameSelectedNode(obj.DisplayName);
                }
            }
        }

        private void MainSplit_SplitterMoved(object? sender, SplitterEventArgs e)
        {
            if (_onPropertyChanged != null)
                propertiesPanel.ShowProperties(selectedObject, _onPropertyChanged);
        }

        private Action<GameObject>? _onPropertyChanged;

        // ===== LIFECYCLE =====

        private void MainForm_Load(object? sender, EventArgs e)
        {
            bool success = EngineInterop.InitEngine(viewportPanel.Handle);
            if (!success) return;

            ApplyCameraToEngine();
            SendObjectsToEngine();

            clock.Start();

            renderTimer = new System.Windows.Forms.Timer();
            renderTimer.Interval = 16;
            renderTimer.Tick += RenderTimer_Tick;
            renderTimer.Start();
        }

        private void RenderTimer_Tick(object? sender, EventArgs e)
        {
            float dt = (float)clock.Elapsed.TotalSeconds;
            clock.Restart();

            camera.UpdateMovement(dt);
            ApplyCameraToEngine();
            SendObjectsToEngine();
            EngineInterop.RenderFrame();
        }

        private void ApplyCameraToEngine()
        {
            EngineInterop.SetCameraPosition(camera.PositionX, camera.PositionY, camera.PositionZ);
            EngineInterop.SetCameraRotation(camera.Pitch, camera.Yaw);
            EngineInterop.SetCameraFOV(camera.FOV);
        }

        private void SendObjectsToEngine()
        {
            EngineInterop.BeginObjectBatch();
            foreach (var obj in Scene.Objects)
            {
                EngineInterop.PushObject(obj.Type,
                    obj.PositionX, obj.PositionY, obj.PositionZ,
                    obj.RotationX, obj.RotationY, obj.RotationZ,
                    obj.ScaleX, obj.ScaleY, obj.ScaleZ);
            }
            EngineInterop.EndObjectBatch();
        }

        private void ColorButton_Click(object? sender, EventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    Color c = colorDialog.Color;
                    EngineInterop.SetClearColor(c.R / 255f, c.G / 255f, c.B / 255f);
                }
            }
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            renderTimer?.Stop();
            EngineInterop.ShutdownEngine();
            clock.Stop();
        }
    }

    // ===== CAMERA CONTROLLER =====

    public class CameraController
    {
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        public float Pitch { get; set; }
        public float Yaw { get; set; }
        public float FOV { get; set; }
        public float MoveSpeed { get; set; } = 3f;

        private readonly HashSet<Keys> pressedKeys = new();

        public CameraController()
        {
            PositionX = 0; PositionY = 2; PositionZ = 8;
            Pitch = -10; Yaw = 0; FOV = 1;
        }

        public void HandleKeyDown(Keys key) => pressedKeys.Add(key);
        public void HandleKeyUp(Keys key) => pressedKeys.Remove(key);

        public void UpdateMovement(float dt)
        {
            if (pressedKeys.Count == 0) return;

            float speed = MoveSpeed * dt;
            if (pressedKeys.Contains(Keys.ShiftKey) || pressedKeys.Contains(Keys.LShiftKey))
                speed *= 3f;

            float pitchRad = Pitch * MathF.PI / 180f;
            float yawRad = Yaw * MathF.PI / 180f;

            // These 3 values are EXACTLY what gluLookAt computes in Core.cpp
            float cosPitch = MathF.Cos(pitchRad);
            float sinPitch = MathF.Sin(pitchRad);
            float cosYaw = MathF.Cos(yawRad);
            float sinYaw = MathF.Sin(yawRad);

            // Forward direction (normalized) — exactly matches Core.cpp lines 82-84
            float fx = cosPitch * sinYaw;
            float fy = sinPitch;
            float fz = cosPitch * cosYaw;

            // Right = normalize(cross(forward, worldUp)) — exactly what gluLookAt uses for the X-axis
            float rightX = cosYaw;
            float rightZ = -sinYaw;

            // Forward on XZ plane only (no vertical WASD unless Space/Ctrl)
            float fwdX = fx;
            float fwdZ = fz;

            if (pressedKeys.Contains(Keys.W)) { PositionX += fwdX * speed;    PositionZ += fwdZ * speed; }
            if (pressedKeys.Contains(Keys.S)) { PositionX -= fwdX * speed;    PositionZ -= fwdZ * speed; }
            if (pressedKeys.Contains(Keys.A)) { PositionX -= rightX * speed;  PositionZ -= rightZ * speed; }
            if (pressedKeys.Contains(Keys.D)) { PositionX += rightX * speed;  PositionZ += rightZ * speed; }
            if (pressedKeys.Contains(Keys.Space)) PositionY += speed;
            if (pressedKeys.Contains(Keys.ControlKey) || pressedKeys.Contains(Keys.LControlKey)) PositionY -= speed;
        }
    }
}
