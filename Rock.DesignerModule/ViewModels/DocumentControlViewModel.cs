using Rock.DesignerModule.Models;
using Rock.DesignerModule.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace Rock.DesignerModule.ViewModels
{
    [Export]
    public class DocumentControlViewModel : ViewModelBase
    {
        private RadPane radPane;
        private RadPaneGroup _radPaneGroup;
        private ContextMenu _paneContextMenu;
        private RadPane currentRadPane;
        public RadPaneGroup RadPaneGroup
        {
            get { return _radPaneGroup; }
            set { _radPaneGroup = value; }
        }
        public ContextMenu PaneContextMenu
        {
            get
            {
                return _paneContextMenu;
            }
            set
            {
                if (value != _paneContextMenu)
                {
                    _paneContextMenu = value;
                    this.OnPropertyChanged("PaneContextMenu");
                }
            }
        }
        public DocumentControlViewModel()
        {
            PaneContextMenu = new ContextMenu();
            MenuItem closeItem = new MenuItem() { Header = "关闭" };
            closeItem.Click += Item_Click;
            _paneContextMenu.Items.Add(closeItem);

            MenuItem closeAllItemButThis = new MenuItem() { Header = "除此之外全部关闭" };
            closeAllItemButThis.Click += Item_Click;
            _paneContextMenu.Items.Add(closeAllItemButThis);

            MenuItem closeAllItem = new MenuItem() { Header = "全部关闭" };
            closeAllItem.Click += Item_Click;
            _paneContextMenu.Items.Add(closeAllItem);
        }

        public void AddPane(string commandName)
        {
            currentRadPane = RadPaneGroup.EnumeratePanes().Where(p => p.Name == commandName).FirstOrDefault();

            if (currentRadPane != null)
            {
                currentRadPane.IsHidden = false;
                currentRadPane.IsSelected = true;
                return;
            }
            else
            {
                switch (commandName)
                {
                    case "AplicationManager":
                        radPane = new RadPane()
                        {
                            Header = "应用程序维护",
                            Name = "AplicationManager",
                            ContextMenuTemplate = null,
                            ContextMenu = PaneContextMenu,
                            Content = new ApplicationManagerView(),
                            IsSelected = true
                        };
                        RadPaneGroup.Items.Add(radPane);
                        break;
                    case "AplicationModuleManager":
                        radPane = new RadPane()
                        {
                            Header = "应用程序模块维护",
                            Name = "AplicationModuleManager",
                            ContextMenuTemplate = null,
                            ContextMenu = PaneContextMenu,
                            Content = new ApplicationModuleManagerView(),
                            IsSelected = true
                        };
                        RadPaneGroup.Items.Add(radPane);
                        break;
                    case "AplicationModuleRelation":
                        radPane = new RadPane()
                        {
                            Header = "应用程序模块关联",
                            Name = "AplicationModuleRelation",
                            ContextMenuTemplate = null,
                            ContextMenu = PaneContextMenu,
                            Content = new ApplicationModuleRelationView(),
                            IsSelected = true
                        };
                        RadPaneGroup.Items.Add(radPane);
                        break;
                    case "NamespaceManager":
                        radPane = new RadPane()
                        {
                            Header = "命名空间维护",
                            Name = "NamespaceManager",
                            ContextMenuTemplate = null,
                            ContextMenu = PaneContextMenu,
                            Content = new NameSpaceManagerView(),
                            IsSelected = true
                        };
                        RadPaneGroup.Items.Add(radPane);
                        break;
                    case "WorkflowDesinger":
                        radPane = new RadPane()
                        {
                            Header = "工作流设计",
                            Name = "WorkflowDesinger",
                            ContextMenuTemplate = null,
                            ContextMenu = PaneContextMenu,
                            Content = new WorkflowDesignerView(),
                            IsSelected = true
                        };
                        RadPaneGroup.Items.Add(radPane);
                        break;
                    case "DesignApplication":
                        radPane = new RadPane()
                        {
                            Header = "应用程序设计",
                            Name = "DesignApplication",
                            ContextMenuTemplate = null,
                            ContextMenu = PaneContextMenu,
                            Content = new DesignerView(),
                            IsSelected = true
                        };
                        RadPaneGroup.Items.Add(radPane);
                        break;
                    default:
                        break;
                }
            }
        }

        public void OpenDesignerView()
        {
            currentRadPane = RadPaneGroup.EnumeratePanes().Where(p => p.Name == "DesignApplication").FirstOrDefault();
            if (currentRadPane != null)
            {
                currentRadPane.IsHidden = false;
                currentRadPane.IsSelected = true;
                currentRadPane.Content = new DesignerView();
            }
            else
            {
                radPane = new RadPane()
                {
                    Header = "应用程序设计",
                    Name = "DesignApplication",
                    ContextMenuTemplate = null,
                    ContextMenu = PaneContextMenu,
                    Content = new DesignerView(),
                    IsSelected = true
                };
                RadPaneGroup.Items.Add(radPane);
            }
        }       
      
        private void Item_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            switch (item.Header.ToString())
            {
                case "关闭":
                    currentRadPane = RadPaneGroup.EnumeratePanes().Where(p => !p.IsHidden && p.IsSelected).FirstOrDefault();
                    RadPaneGroup.RemovePane(currentRadPane);
                    break;
                case "除此之外全部关闭":
                    currentRadPane = RadPaneGroup.EnumeratePanes().Where(p => !p.IsHidden && p.IsSelected).FirstOrDefault();
                    var panesToClose = RadPaneGroup.EnumeratePanes().Where(p => p != currentRadPane);
                    for (int i = panesToClose.Count() - 1; i >= 0; i--)
                    {
                        this.RadPaneGroup.RemovePane(panesToClose.ElementAt(i));
                    }
                    break;
                case "全部关闭":
                    RadPaneGroup.Items.Clear();
                    break;
            }
        }
    }
}
