using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

namespace Test
{
    public partial class Form1 : Form
    {
        private Searcher _searcher;
        private Thread _calculating;
        public Form1()
        {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            InitializeComponent();
            button2.Enabled = false;
            _searcher = new Searcher();
            _searcher.StatusChanged += _searcher_StatusChanged;
            _searcher.TreeChanged += _searcher_TreeChanged;
            _searcher.FilesFounded += _searcher_FilesFounded;
            _calculating = new Thread(new ParameterizedThreadStart(_searcher.Search));
            label4.Text = "";
            label5.Text = "";
            StreamReader streamReader = new StreamReader("C:/Users/Mikhail/source/repos/Test/LastSearch.txt");
            string lastSearch = streamReader.ReadToEnd();
            streamReader.Close();
            textBox1.Text = lastSearch.Remove(lastSearch.LastIndexOf("\r\n"));
            textBox2.Text = lastSearch.Remove(0,lastSearch.LastIndexOf("\r\n")+2);
        }

        private void _searcher_FilesFounded(int arg1, int arg2)
        {
            Action action = () =>
            {
                if (label4.Text == "")
                    label4.Text = arg1.ToString();
                else
                {
                    int filesAll = int.Parse(label4.Text) + arg1;
                    label4.Text = filesAll.ToString();
                }
                if (label5.Text == "")
                    label5.Text = arg2.ToString();
                else
                {
                    int filesAll = int.Parse(label5.Text) + arg2;
                    label5.Text = filesAll.ToString();
                }
            };
            Invoke(action);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(textBox1.Text))
            {
                Object inputData;
                Regex regex;
                treeView1.Nodes.Clear();
                label4.Text = "";
                label5.Text = "";
                try
                {
                    if (textBox2.Text == "")
                        regex = new Regex(textBox2.Text);
                    else
                        regex = new Regex("@" + textBox2.Text);
                }
                catch (ArgumentException)
                {
                    MessageBox.Show("Неверный формат регулярного выражения");
                    return;
                }
                StreamWriter streamWriter = new StreamWriter("C:/Users/Mikhail/source/repos/Test/LastSearch.txt", false);
                streamWriter.WriteLine(textBox1.Text);
                streamWriter.WriteLine(textBox2.Text);
                streamWriter.Close();
                inputData = new InputData(textBox1.Text, regex);
                _searcher.ChangeSearchStatus(Searcher.SearchStatus.process);
                if (_calculating.IsAlive)
                {
                    if (_calculating.ThreadState == ThreadState.Suspended)
                        _calculating.Resume();
                    _calculating.Abort();
                    _calculating.Join();
                    _searcher = new Searcher();
                    _searcher.StatusChanged += _searcher_StatusChanged;
                    _searcher.TreeChanged += _searcher_TreeChanged;
                    _calculating = new Thread(new ParameterizedThreadStart(_searcher.Search));
                }
                _calculating.Start(inputData);
                //_searcher.Search(inputData);
            }
            else
            {
                MessageBox.Show("Ошибка! Каталог не существует!");
                return;
            }
        }

        private void _searcher_TreeChanged(TreeNode parentNode, TreeNode childNode)
        {
            Action action = () =>
            {
                if (parentNode == null)
                {
                    treeView1.Nodes.Add(childNode);
                }
                else
                {
                    if (treeView1.Nodes.Find(parentNode.Name,true).Length != 0)
                    {
                        if (treeView1.Nodes.Find(childNode.Name,true).Length == 0)
                        {
                            TreeNode treeNodeToAdd = treeView1.Nodes.Find(parentNode.Name, true)[0];
                            treeNodeToAdd.Nodes.Add(childNode);
                        }
                    }
                    else
                    {
                        _RecursiveBuildATree(parentNode, childNode);
                    }
                }
            };
            Invoke(action);
        }

        private void _searcher_StatusChanged(Searcher.SearchStatus obj)
        {
            Searcher.SearchStatus status = obj;
            if (status == Searcher.SearchStatus.process)
            {
                button1.Enabled = false;
                button2.Text = "Остановить поиск";
                button2.Enabled = true;
            }
            else if (status == Searcher.SearchStatus.idle)
            {
                button1.Enabled = true;
                button2.Text = "Остановить поиск";
                button2.Enabled = false;
            }
            else if (status == Searcher.SearchStatus.pause)
            {
                button1.Enabled = true;
                button2.Text = "Продолжить поиск";
                _calculating.Suspend();
                button2.Enabled = true;
            }
            else if (status == Searcher.SearchStatus.complete)
            {
                button1.Enabled = true;
                button2.Text = "Остановить поиск";
                if (_calculating.IsAlive)
                {
                    if (_calculating.ThreadState == ThreadState.Suspended)
                    {
                        _calculating.Resume();
                    }
                    _calculating.Abort();
                    _calculating.Join();
                }
                button2.Enabled = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (_searcher.Status() == Searcher.SearchStatus.process)
            {
                _searcher.ChangeSearchStatus(Searcher.SearchStatus.pause);
            }
            else if (_searcher.Status() == Searcher.SearchStatus.pause)
            {
                _searcher.ChangeSearchStatus(Searcher.SearchStatus.process);
                _calculating.Resume();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_calculating.IsAlive)
            {
                if (_calculating.ThreadState == ThreadState.Suspended)
                {
                    _calculating.Resume();
                }
                _calculating.Abort();
                _calculating.Join();
            }
        }

        private void _RecursiveBuildATree(TreeNode parentNode, TreeNode childNode)
        {
            string parentPath = parentNode.Name;
            string parentOfParentPath = parentPath.Remove(parentPath.LastIndexOf("\\"));
            if (treeView1.Nodes.Find(parentOfParentPath, true).Length != 0)
            {
                TreeNode treeNode = treeView1.Nodes.Find(parentOfParentPath, true)[0];
                parentNode.Nodes.Add(childNode);
                treeNode.Nodes.Add(parentNode);
            }
            else
            {
                parentNode.Nodes.Add(childNode);
                TreeNode treeNode = new TreeNode(parentOfParentPath);
                treeNode.Name = parentOfParentPath;
                treeNode.Text = parentOfParentPath.Remove(0, parentOfParentPath.LastIndexOf("\\") + 1);
                _RecursiveBuildATree(treeNode, parentNode);
            }
        }
    }

    public class DoubleBufferedTreeView : TreeView
    {
        protected override void OnHandleCreated(EventArgs e)
        {
            SendMessage(this.Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)TVS_EX_DOUBLEBUFFER);
            base.OnHandleCreated(e);
        }
        // Pinvoke:
        private const int TVM_SETEXTENDEDSTYLE = 0x1100 + 44;
        private const int TVM_GETEXTENDEDSTYLE = 0x1100 + 45;
        private const int TVS_EX_DOUBLEBUFFER = 0x0004;
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
    }

    class InputData:Object
    {
        public string startDirectory;
        public Regex regex;
        public InputData(string startDirectory, Regex regex)
        {
            this.startDirectory = startDirectory;
            this.regex = regex;
        }
    }
}
