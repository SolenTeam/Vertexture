using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace VertextureEditor
{
    public class ExplorerPanel : UserControl
    {
        private TreeView hierarchyTree = null!;
        private Panel toolbarPanel = null!;

        public event Action<string>? ObjectAdded;     // type: "cube", "sphere", "light"
        public event Action<string, int>? ObjectSelected; // name, id
        public event Action<string, int>? ObjectDeleted;  // name, id

        public ExplorerPanel()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(42, 42, 46);
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(0);

            // Toolbar
            toolbarPanel = new Panel();
            toolbarPanel.Dock = DockStyle.Top;
            toolbarPanel.Height = 30;
            toolbarPanel.BackColor = Color.FromArgb(50, 50, 54);
            toolbarPanel.Padding = new Padding(4, 3, 4, 0);

            Button btnAdd = new Button();
            btnAdd.Text = "+ Add";
            btnAdd.ForeColor = Color.White;
            btnAdd.BackColor = Color.FromArgb(60, 60, 65);
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Font = new Font("Segoe UI", 8.25f, FontStyle.Regular);
            btnAdd.Size = new Size(56, 22);
            btnAdd.Location = new Point(4, 3);

            ContextMenuStrip addMenu = new ContextMenuStrip();
            addMenu.BackColor = Color.FromArgb(45, 45, 48);
            addMenu.ForeColor = Color.White;
            addMenu.Renderer = new ToolStripProfessionalRenderer(new DarkColorTable());

            addMenu.Items.Add("Cube").Click += (s, e) => ObjectAdded?.Invoke("cube");
            addMenu.Items.Add("Sphere").Click += (s, e) => ObjectAdded?.Invoke("sphere");
            addMenu.Items.Add("Light").Click += (s, e) => ObjectAdded?.Invoke("light");

            btnAdd.Click += (s, e) => addMenu.Show(btnAdd, 0, btnAdd.Height);

            Button btnDelete = new Button();
            btnDelete.Text = "Delete";
            btnDelete.ForeColor = Color.FromArgb(220, 100, 100);
            btnDelete.BackColor = Color.FromArgb(50, 50, 54);
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Font = new Font("Segoe UI", 8.25f, FontStyle.Regular);
            btnDelete.Size = new Size(56, 22);
            btnDelete.Location = new Point(64, 3);
            btnDelete.Click += BtnDelete_Click;

            toolbarPanel.Controls.Add(btnAdd);
            toolbarPanel.Controls.Add(btnDelete);

            // Tree View
            hierarchyTree = new TreeView();
            hierarchyTree.Dock = DockStyle.Fill;
            hierarchyTree.BackColor = Color.FromArgb(38, 38, 42);
            hierarchyTree.ForeColor = Color.FromArgb(220, 220, 220);
            hierarchyTree.BorderStyle = BorderStyle.None;
            hierarchyTree.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
            hierarchyTree.HideSelection = false;
            hierarchyTree.ShowLines = false;
            hierarchyTree.FullRowSelect = true;
            hierarchyTree.Indent = 16;
            hierarchyTree.AfterSelect += HierarchyTree_AfterSelect;

            this.Controls.Add(hierarchyTree);
            this.Controls.Add(toolbarPanel);
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (hierarchyTree.SelectedNode?.Tag is int id)
            {
                string name = hierarchyTree.SelectedNode.Text;
                ObjectDeleted?.Invoke(name, id);
                hierarchyTree.Nodes.Remove(hierarchyTree.SelectedNode);
            }
        }

        private void HierarchyTree_AfterSelect(object? sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is int id)
            {
                ObjectSelected?.Invoke(e.Node.Text, id);
            }
        }

        // Public methods called from MainForm
        public void AddNode(string displayName, int id, string type)
        {
            TreeNode node = new TreeNode(displayName) { Tag = id };
            hierarchyTree.Nodes.Add(node);
        }

        public void SelectNode(string displayName)
        {
            foreach (TreeNode node in hierarchyTree.Nodes)
            {
                if (node.Text == displayName)
                {
                    hierarchyTree.SelectedNode = node;
                    break;
                }
            }
        }

        public void RenameSelectedNode(string newName)
        {
            if (hierarchyTree.SelectedNode != null)
            {
                hierarchyTree.SelectedNode.Text = newName;
            }
        }

        public TreeNode? SelectedNode => hierarchyTree.SelectedNode;
    }

    internal class DarkColorTable : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground => Color.FromArgb(45, 45, 48);
        public override Color MenuItemBorder => Color.FromArgb(60, 60, 65);
        public override Color MenuItemSelected => Color.FromArgb(60, 60, 65);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(60, 60, 65);
        public override Color MenuItemSelectedGradientEnd => Color.FromArgb(60, 60, 65);
    }
}
