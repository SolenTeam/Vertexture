using System;
using System.Drawing;
using System.Windows.Forms;

namespace VertextureEditor
{
    public class PropertiesPanel : UserControl
    {
        private Panel contentPanel = null!;
        private ToolStrip toolStrip = null!;

        private const int ROW_HEIGHT = 26;
        private const int LABEL_WIDTH = 110;
        private const int PAD_LEFT = 8;
        private const int PAD_RIGHT = 8;

        private Action<GameObject>? _onPropertyChanged;

        public PropertiesPanel()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(35, 35, 38);
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(0);

            toolStrip = new ToolStrip();
            toolStrip.Dock = DockStyle.Top;
            toolStrip.BackColor = Color.FromArgb(45, 45, 48);
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.RenderMode = ToolStripRenderMode.System;
            toolStrip.Padding = new Padding(4, 0, 2, 0);

            ToolStripLabel label = new ToolStripLabel("Properties");
            label.ForeColor = Color.FromArgb(180, 180, 180);
            label.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            toolStrip.Items.Add(label);

            contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = Color.FromArgb(38, 38, 42);
            contentPanel.AutoScroll = true;
            contentPanel.Padding = new Padding(4, 4, 4, 4);

            this.Controls.Add(contentPanel);
            this.Controls.Add(toolStrip);
        }

        public void ShowProperties(GameObject? obj, Action<GameObject> onPropertyChanged)
        {
            _onPropertyChanged = onPropertyChanged;
            contentPanel.Controls.Clear();

            if (obj == null)
            {
                var lbl = new Label
                {
                    Text = "No object selected",
                    ForeColor = Color.FromArgb(100, 100, 100),
                    Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                contentPanel.Controls.Add(lbl);
                return;
            }

            int yOffset = 0;
            int innerWidth = contentPanel.ClientSize.Width - PAD_LEFT - PAD_RIGHT;

            AddSectionHeader("General", ref yOffset);
            AddPropRow("Name", obj.Name, (v) => { obj.Name = v; _onPropertyChanged?.Invoke(obj); }, innerWidth, ref yOffset);
            AddPropRow("Type", obj.Type, null, innerWidth, ref yOffset, true);
            AddPropRow("ID", obj.Id.ToString(), null, innerWidth, ref yOffset, true);

            AddSectionHeader("Transform", ref yOffset);
            AddVectorRow("Position", obj.PositionX, obj.PositionY, obj.PositionZ, (x, y, z) =>
            { obj.PositionX = x; obj.PositionY = y; obj.PositionZ = z; _onPropertyChanged?.Invoke(obj); }, innerWidth, ref yOffset);
            AddVectorRow("Rotation", obj.RotationX, obj.RotationY, obj.RotationZ, (x, y, z) =>
            { obj.RotationX = x; obj.RotationY = y; obj.RotationZ = z; _onPropertyChanged?.Invoke(obj); }, innerWidth, ref yOffset);
            AddVectorRow("Scale", obj.ScaleX, obj.ScaleY, obj.ScaleZ, (x, y, z) =>
            { obj.ScaleX = x; obj.ScaleY = y; obj.ScaleZ = z; _onPropertyChanged?.Invoke(obj); }, innerWidth, ref yOffset);
        }

        private void AddSectionHeader(string title, ref int y)
        {
            var header = new Label
            {
                Text = title,
                ForeColor = Color.FromArgb(140, 140, 145),
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                Location = new Point(PAD_LEFT, y),
                Size = new Size(contentPanel.ClientSize.Width - PAD_LEFT - PAD_RIGHT, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };
            contentPanel.Controls.Add(header);
            y += 24;
        }

        private void AddPropRow(string label, string value, Action<string>? onChange, int innerWidth, ref int y, bool readOnly = false)
        {
            var lbl = new Label
            {
                Text = label,
                ForeColor = Color.FromArgb(170, 170, 175),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                Location = new Point(PAD_LEFT, y),
                Size = new Size(LABEL_WIDTH, ROW_HEIGHT),
                TextAlign = ContentAlignment.MiddleLeft
            };
            contentPanel.Controls.Add(lbl);

            var txt = new TextBox
            {
                Text = value,
                Font = new Font("Consolas", 8.5f, FontStyle.Regular),
                Location = new Point(PAD_LEFT + LABEL_WIDTH, y),
                Size = new Size(innerWidth - LABEL_WIDTH, 22),
                ForeColor = readOnly ? Color.FromArgb(120, 120, 120) : Color.White,
                BackColor = readOnly ? Color.FromArgb(28, 28, 30) : Color.FromArgb(45, 45, 48),
                BorderStyle = readOnly ? BorderStyle.None : BorderStyle.FixedSingle,
                ReadOnly = readOnly,
                TextAlign = HorizontalAlignment.Right
            };

            if (!readOnly && onChange != null)
            {
                txt.Leave += (s, e) => onChange(txt.Text);
                txt.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { onChange(txt.Text); e.Handled = true; e.SuppressKeyPress = true; } };
            }
            contentPanel.Controls.Add(txt);
            y += ROW_HEIGHT + 2;
        }

        private void AddVectorRow(string label, float x, float yVal, float z, Action<float, float, float> onChange, int innerWidth, ref int y)
        {
            var lbl = new Label
            {
                Text = label,
                ForeColor = Color.FromArgb(170, 170, 175),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                Location = new Point(PAD_LEFT, y),
                Size = new Size(LABEL_WIDTH, ROW_HEIGHT),
                TextAlign = ContentAlignment.MiddleLeft
            };
            contentPanel.Controls.Add(lbl);

            int fieldWidth = (innerWidth - LABEL_WIDTH) / 3 - 4;
            int startX = PAD_LEFT + LABEL_WIDTH;

            var txtX = CreateFloatField(x, startX, y, fieldWidth);
            var txtY = CreateFloatField(yVal, startX + fieldWidth + 4, y, fieldWidth);
            var txtZ = CreateFloatField(z, startX + (fieldWidth + 4) * 2, y, fieldWidth);

            void Apply()
            {
                if (float.TryParse(txtX.Text, out float vx) &&
                    float.TryParse(txtY.Text, out float vy) &&
                    float.TryParse(txtZ.Text, out float vz))
                {
                    onChange(vx, vy, vz);
                }
            }

            txtX.Leave += (s, e) => Apply();
            txtY.Leave += (s, e) => Apply();
            txtZ.Leave += (s, e) => Apply();
            txtX.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { Apply(); e.Handled = true; e.SuppressKeyPress = true; } };
            txtY.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { Apply(); e.Handled = true; e.SuppressKeyPress = true; } };
            txtZ.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { Apply(); e.Handled = true; e.SuppressKeyPress = true; } };

            contentPanel.Controls.Add(txtX);
            contentPanel.Controls.Add(txtY);
            contentPanel.Controls.Add(txtZ);
            y += ROW_HEIGHT + 2;
        }

        private TextBox CreateFloatField(float value, int x, int y, int width)
        {
            return new TextBox
            {
                Text = value.ToString("F2"),
                Font = new Font("Consolas", 8.5f, FontStyle.Regular),
                Location = new Point(x, y),
                Size = new Size(width, 22),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(45, 45, 48),
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = HorizontalAlignment.Center
            };
        }
    }
}
