using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;

namespace Test
{
    class Searcher
    {
        public enum SearchStatus
        {
            idle,
            process,
            pause,
            complete
        }

        private SearchStatus _searchStatus;

        public Searcher()
        {
            _searchStatus = SearchStatus.idle;
        }

        public void ChangeSearchStatus(SearchStatus status)
        {
            
            _searchStatus = status;
            StatusChanged(status);
        }

        public SearchStatus Status()
        {
            return _searchStatus;
        }

        public void Search(object obj)
        {
            
            InputData inputData = (InputData)obj;
            _searchStatus = SearchStatus.process;
            StatusChanged(SearchStatus.process);
            TreeNode startNode = new TreeNode(inputData.startDirectory);
            startNode.Text = inputData.startDirectory;
            startNode.Name = inputData.startDirectory;
            TreeChanged(null, startNode);
            _SearchInDirectory(startNode, inputData.regex);
            _searchStatus = SearchStatus.complete;
            StatusChanged(SearchStatus.complete);
        }

        private Match _SearchInDirectory(TreeNode parentNode, Regex regex = null)
        {
            TreeNode filesInCatalogue = new TreeNode();
            bool isMatch = false;
            string[] filesInDirectory = null;
            string[] cataloguesInDirectory = null;
            try
            {
                filesInDirectory = Directory.GetFiles(parentNode.Name +"\\");
                cataloguesInDirectory = Directory.GetDirectories(parentNode.Name + "\\");
            }
            catch (UnauthorizedAccessException)
            {
                return new Match(isMatch, parentNode);
            }
            List<File> files = new List<File>();
            List<Catalogue> catalogues = new List<Catalogue>();
            
            foreach (string file in filesInDirectory)
            {
                File f = new File(file);
                if (regex.IsMatch(f.fileName))
                {
                    files.Add(f);
                    isMatch = true;
                }
            }
            foreach (TreeNode treeNode in File.ToTreeNodes(files))
                TreeChanged(parentNode, treeNode);
            FilesFounded(filesInDirectory.Length, files.Count);
            foreach (string catalog in cataloguesInDirectory)
            {
                TreeNode catalogNode = new TreeNode(catalog);
                catalogNode.Name = catalog;
                catalogNode.Text = catalog.Remove(0, catalog.LastIndexOf("\\") + 1);
                Match match = _SearchInDirectory(catalogNode, regex);
                if (match.isMatch == true)
                {
                    TreeChanged(parentNode, catalogNode);
                    isMatch = true;
                }
            }
            return new Match(isMatch, parentNode);
            }

        public event Action<SearchStatus> StatusChanged;
        public event Action<int, int> FilesFounded;
        public event Action<TreeNode, TreeNode> TreeChanged;
    }

    class Match
    {
        public bool isMatch;
        public TreeNode TreeNode;
        public Match(bool isMatch, TreeNode treeNode)
        {
            this.isMatch = isMatch;
            this.TreeNode = treeNode;
        }
    }

    class File
    {
        public string fileName;
        public File(string file)
        {
            for (int i = file.Length - 1; i > 0; i--)
            {
                if (file[i].ToString() != "\\")
                    fileName += file[i].ToString();
                else
                    break;
            }
            char[] tName = fileName.ToCharArray();
            Array.Reverse(tName);
            fileName = new string(tName);
        }
        public static TreeNode[] ToTreeNodes(List<File> files)
        {
            TreeNode[] treeNodes = new TreeNode[files.Count];
            for (int i = 0; i < files.Count; i++)
            {
                TreeNode treeNode = new TreeNode(files[i].fileName);
                treeNode.Text = files[i].fileName;
                treeNode.Name = files[i].fileName;
                treeNodes[i] = treeNode;
            }
            return treeNodes;
        }
    }

    class Catalogue
    {
        public string catalogueName;
        public Catalogue(string catalogue)
        {
            for (int i = catalogue.Length - 1; i > 0; i--)
            {
                if (catalogue[i].ToString() != "\\")
                    catalogueName += catalogue[i].ToString();
                else
                    break;
            }
            char[] tName = catalogueName.ToCharArray();
            Array.Reverse(tName);
            catalogueName = new string(tName);
        }
    }
}
