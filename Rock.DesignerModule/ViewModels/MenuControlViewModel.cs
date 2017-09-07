using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.ServiceLocation;
using Rock.DesignerModule.Models;
using Rock.DesignerModule.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Rock.DesignerModule.ViewModels
{
    [Export]
    public class MenuControlViewModel
    {
        public DocumentControlViewModel DocumentControlViewModel
        {
            get { return ServiceLocator.Current.GetInstance<DocumentControlViewModel>(); }
        }
        public DelegateCommand<string> AddPaneCommand { get; private set; }
        public ICommand OpenApplicationCommand { get; private set; }
        public ICommand GenerateStaticEntityCommand { get; private set; }
        //
        public MenuControlViewModel()
        {
            AddPaneCommand = new DelegateCommand<string>(AddPane);
            OpenApplicationCommand = new DelegateCommand<object>(OpenApplication);
            GenerateStaticEntityCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(GenerateStaticEntity);
        }

        public void AddPane(string commandName)
        {
            DocumentControlViewModel.AddPane(commandName);            
        }
        private void OpenApplication(object arg)
        {
            ApplicationOpenWindow applicationOpenWindow = new ApplicationOpenWindow();
            applicationOpenWindow.ShowDialog();
        }
        private void GenerateStaticEntity()
        {
 
        }
    }
}
