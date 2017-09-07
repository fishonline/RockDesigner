using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace Rock.DesignerModule.Models
{
    public class RadPaneViewModel : INotifyPropertyChanged, IDisposable
    {
        private bool _isSelected;
        private ContentControl _contentControl;
        private ContextMenu _contextMenu;
        public ObservableCollection<RadPaneViewModel> RadPanes;
        private string _name;

        private string _header;

        public string Header
        {
            get { return _header; }
            set
            {
                _header = value;
                this.OnPropertyChanged("Header");
            }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public ContentControl ContentControl
        {
            get
            {
                return _contentControl;
            }
            set
            {
                if (_contentControl != value)
                {
                    _contentControl = value;
                    OnPropertyChanged("ContentControl");
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
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }

        public ContextMenu ContextMenu
        {
            get
            {
                return _contextMenu;
            }
            set
            {
                if (value != _contextMenu)
                {
                    _contextMenu = value;
                    this.OnPropertyChanged("ContextMenu");
                }
            }
        }

        //public object Tag { get; set; }
        public DelegateCommand RemoveItemCommand { get; set; }

        public RadPaneViewModel()
        {
            RemoveItemCommand = new DelegateCommand(
                delegate
                {
                    RadPanes.Remove(this);
                });
            //this.RadPanes = tabs;
            //TabName = tabName;
            InitContextMenu();
        }

        ~RadPaneViewModel()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            if (this._contentControl is IDisposable)
            {
                IDisposable control = this._contentControl as IDisposable;
                control.Dispose();
            }
        }

        private void InitContextMenu()
        {
            _contextMenu = new ContextMenu();

            MenuItem closeItem = new MenuItem() { Header = "关闭" };
            closeItem.Click += Item_Click;
            _contextMenu.Items.Add(closeItem);

            MenuItem closeAllItemButThis = new MenuItem() { Header = "除此之外全部关闭" };
            closeAllItemButThis.Click += Item_Click;
            _contextMenu.Items.Add(closeAllItemButThis);

            MenuItem closeAllItem = new MenuItem() { Header = "全部关闭" };
            closeAllItem.Click += Item_Click;
            _contextMenu.Items.Add(closeAllItem);
        }

        private void Item_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            //switch (item.Header.ToString())
            //{
            //    case "关闭":
            //        RadPanes.Remove(this);
            //        break;
            //    case "除此之外全部关闭":
            //        var list = RadPanes.Where(p => p != this);
            //        for (int i = list.Count() - 1; i >= 0; i--)
            //        {
            //            RadPanes.Remove(list.ElementAt(i));
            //        }
            //        break;
            //    case "全部关闭":
            //        RadPanes.Clear();
            //        break;
            //}
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
