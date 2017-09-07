using Rock.DesignerModule.Models;
using Rock.DesignerModule.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telerik.Windows.DragDrop;

namespace Rock.DesignerModule.Views
{
    /// <summary>
    /// ApplicationModuleRelationView.xaml 的交互逻辑
    /// </summary>   
    public partial class ApplicationModuleRelationView : UserControl
    {
        public ApplicationModuleRelationViewModel ViewModel = new ApplicationModuleRelationViewModel();
        public ApplicationModuleRelationView()
        {
            InitializeComponent();
            this.DataContext = ViewModel;

            DragDropManager.AddDragDropCompletedHandler(listBox1, new DragDropCompletedEventHandler(ListDropComplete), true);
            DragDropManager.AddDragDropCompletedHandler(listBox2, new DragDropCompletedEventHandler(ListDropComplete), true);
            DragDropManager.AddDragOverHandler(radGridView, new Telerik.Windows.DragDrop.DragEventHandler(ListDragOver), true);
        }
        private void ListDragOver(object sender, Telerik.Windows.DragDrop.DragEventArgs e)
        {
            e.Handled = true;
            e.Effects = DragDropEffects.None;
        }

        private void ListDropComplete(object sender, DragDropCompletedEventArgs e)
        {
            if (e.Effects != DragDropEffects.None)
            {
                var formats = DragDropPayloadManager.GetFormats(e.Data) as string[];
                if (formats.Length > 0)
                {
                    var options = DragDropPayloadManager.GetDataFromObject(e.Data, formats[0]) as IList;

                    if (options.Count > 0)
                    {
                        ApplicationModule optianModule = options[0] as ApplicationModule;
                        if (sender == listBox1)
                        {
                            ViewModel.AppUnRelationModule(optianModule.ModuleID);
                        }
                        else if (sender == listBox2)
                        {
                            ViewModel.AppRelationModule(optianModule.ModuleID);
                        }
                    }
                }
            }
        }
    }
}

