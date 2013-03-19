using System;
using System.Linq;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;

namespace FileIncluder
{
    public partial class UserControl1 : UserControl
    {
        private DTE2 _applicationObject;
        private FileWatcher _fileWatcher;
        private SolutionEvents solutionEvents;

        private delegate void AddListItem(IncludeItem item);

        private AddListItem myDelegate;

        public UserControl1()
        {
            InitializeComponent();

            myDelegate = AddItem;

            treeView1.AfterCheck += TreeView1OnAfterCheck;
        }

        private void TreeView1OnAfterCheck(object sender, TreeViewEventArgs args)
        {
            treeView1.BeginUpdate();
            treeView1.AfterCheck -= TreeView1OnAfterCheck;

            foreach (TreeNode node in args.Node.Nodes)
            {
                node.Checked = args.Node.Checked;
            }

            for (var n = args.Node.Parent; n != null; n = n.Parent)
            {
                n.Checked = n.Nodes.Cast<TreeNode>().All(i => i.Checked);
            }

            treeView1.EndUpdate();
            treeView1.AfterCheck += TreeView1OnAfterCheck;
        }

        public void Init(DTE2 application)
        {
            _applicationObject = application;

            if (_applicationObject.Solution == null || !_applicationObject.Solution.IsOpen)
            {
                solutionEvents = _applicationObject.Events.SolutionEvents;
                solutionEvents.Opened += SolutionEventsOnOpened;
            }
            else
            {
                SolutionEventsOnOpened();
            }
        }

        private void SolutionEventsOnOpened()
        {
            if (_applicationObject.Solution == null) throw new InvalidOperationException("No solution");

            _fileWatcher = new FileWatcher(_applicationObject.Solution);

            _fileWatcher.Changed += item =>
                {
                    if (treeView1.InvokeRequired)
                    {
                        treeView1.Invoke(myDelegate, new object[] {item});
                    }
                    else
                    {
                        AddItem(item);
                    }
                };
        }

        private void AddItem(IncludeItem item)
        {
            var pNode = treeView1.Nodes[item.ProjectId];

            if (pNode == null)
            {
                pNode = new TreeNode {Name = item.ProjectId, Text = item.Project};

                treeView1.Nodes.Add(pNode);
            }
            pNode.Nodes.Add(item.FullPath, item.File).Checked = true;

            pNode.Expand();
        }

        private void btnInclude_Click(object sender, EventArgs e)
        {
            HandleAction(true);
        }

        private void btnIgnore_Click(object sender, EventArgs e)
        {
            HandleAction(false);
        }

        private void HandleAction(bool isInclude)
        {
            try
            {
                treeView1.BeginUpdate();

                foreach (TreeNode project in treeView1.Nodes)
                {
                    foreach (var file in project.Nodes.Cast<TreeNode>().Where(n => n.Checked).ToList())
                    {
                        if (isInclude)
                        {
                            var item = _applicationObject.Solution.Projects.Item(project.Name).ProjectItems.AddFromFile(file.Name);
                            item.Open().Activate();
                        }
                        project.Nodes.Remove(file);
                    }
                }

                foreach (var node in treeView1.Nodes.Cast<TreeNode>().Where(node => node.Nodes.Count == 0).ToList())
                {
                    treeView1.Nodes.Remove(node);
                }

                treeView1.EndUpdate();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }
    }
}
