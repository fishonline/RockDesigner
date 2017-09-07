using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Rock.DesignerModule.Models
{
    public class TreeNode : INotifyPropertyChanged
    {
        private bool _isExpanded;
        private bool _isSelected;
        private bool _isChecked;
        private bool _isEnabled;
        private int _treeNodeID;
        private string _name;
        private string _type;
        private ImageSource _image;
        private object _tag;
        private int _level;
        private TreeNode _parent;
        //private Visibility _isShowText = Visibility.Visible;
        //private Visibility _isShowEdit = Visibility.Collapsed;
        private string _comment;

        private ObservableCollection<TreeNode> _children;

        public TreeNode()
        {
            _children = new ObservableCollection<TreeNode>();
        }

        public int TreeNodeID
        {
            get
            {
                return this._treeNodeID;
            }
            set
            {
                if (this._treeNodeID != value)
                {
                    this._treeNodeID = value;
                    NotifyPropertyChanged("TreeNodeID");
                }
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                if (this._name != value)
                {
                    this._name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        public TreeNode Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                NotifyPropertyChanged("Parent");
            }
        }

        public int Level
        {
            get { return _level; }

            set
            {
                _level = value;
                NotifyPropertyChanged("Level");
            }
        }

        public string Type
        {
            get
            {
                return this._type;
            }
            set
            {
                if (this._type != value)
                {
                    this._type = value;
                    NotifyPropertyChanged("Type");
                }
            }
        }

        public ImageSource Image
        {
            get
            {
                return this._image;
            }
            set
            {
                if (this._image != value)
                {
                    this._image = value;
                    NotifyPropertyChanged("Image");
                }
            }
        }

        public bool IsSelected
        {
            get
            {
                return this._isSelected;
            }
            set
            {
                if (this._isSelected != value)
                {
                    this._isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
            }
        }

        public bool IsChecked
        {
            get
            {
                return this._isChecked;
            }
            set
            {
                if (this._isChecked != value)
                {
                    this._isChecked = value;
                    NotifyPropertyChanged("IsChecked");
                }
            }
        }

        public bool IsExpanded
        {
            get
            {
                return this._isExpanded;
            }
            set
            {
                if (this._isExpanded != value)
                {
                    this._isExpanded = value;
                    NotifyPropertyChanged("IsExpanded");
                }
            }
        }

        public bool IsEnabled
        {
            get
            {
                return this._isEnabled;
            }
            set
            {
                if (this._isEnabled != value)
                {
                    this._isEnabled = value;
                    NotifyPropertyChanged("IsEnabled");
                }
            }
        }

        public object Tag
        {
            get
            {
                return this._tag;
            }
            set
            {
                if (this._tag != value)
                {
                    this._tag = value;
                    NotifyPropertyChanged("Tag");
                }
            }
        }

        public ObservableCollection<TreeNode> Children
        {
            get
            {
                return this._children;
            }
            set
            {
                if (this._children != value)
                {
                    this._children = value;
                    NotifyPropertyChanged("Children");
                }
            }
        }

        //[Browsable(false)]
        //public Visibility IsShowText
        //{
        //    get { return _isShowText; }
        //    set
        //    {
        //        _isShowText = value;
        //        NotifyPropertyChanged("IsShowText");
        //    }
        //}

        //[Browsable(false)]
        //public Visibility IsShowEdit
        //{
        //    get { return _isShowEdit; }
        //    set
        //    {
        //        _isShowEdit = value;
        //        NotifyPropertyChanged("IsShowEdit");
        //    }
        //}

        [Browsable(false)]
        public string Comment
        {
            get { return _comment; }
            set
            {
                _comment = value;
                NotifyPropertyChanged("Comment");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
