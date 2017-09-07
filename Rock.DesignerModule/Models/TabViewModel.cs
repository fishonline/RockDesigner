using Microsoft.Practices.ServiceLocation;
using Rock.DesignerModule.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace Rock.DesignerModule.Models
{
    public class TabViewModel : INotifyPropertyChanged, IDisposable
    {
        private bool _isSelected;        
        private UserControl _contentControl;
        private ContextMenu _tabContextMenu;
        private ObservableCollection<TabViewModel> Tabs;
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

        public string TabName { get; private set; }

        public UserControl ContentControl
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

        public ContextMenu TabContextMenu
        {
            get
            {
                return _tabContextMenu;
            }
            set
            {
                if (value != _tabContextMenu)
                {
                    _tabContextMenu = value;
                    this.OnPropertyChanged("TabContextMenu");
                }
            }
        }

        public object Tag { get; set; }
        public DelegateCommand RemoveItemCommand { get; set; }

        public TabViewModel(ObservableCollection<TabViewModel> tabs, string tabName)
        {
            RemoveItemCommand = new DelegateCommand(
                delegate
                {
                    Tabs.Remove(this);
                });
            this.Tabs = tabs;
            TabName = tabName;            
            InitContextMenu();
        }

        ~TabViewModel()
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
            _tabContextMenu = new ContextMenu();

            MenuItem closeItem = new MenuItem() { Header = "关闭" };
            closeItem.Click += Item_Click;
            _tabContextMenu.Items.Add(closeItem);

            MenuItem closeAllItemButThis = new MenuItem() { Header = "除此之外全部关闭" };
            closeAllItemButThis.Click += Item_Click;
            _tabContextMenu.Items.Add(closeAllItemButThis);

            MenuItem closeAllItem = new MenuItem() { Header = "全部关闭" };
            closeAllItem.Click += Item_Click;
            _tabContextMenu.Items.Add(closeAllItem);
        }

        private void Item_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            switch (item.Header.ToString())
            {
                case "关闭":
                    Tabs.Remove(this);
                    break;
                case "除此之外全部关闭":
                    var list = Tabs.Where(p => p != this);
                    for (int i = list.Count() - 1; i >= 0; i--)
                    {
                        Tabs.Remove(list.ElementAt(i));
                    }
                    break;
                case "全部关闭":
                    Tabs.Clear();
                    break;
            }
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
