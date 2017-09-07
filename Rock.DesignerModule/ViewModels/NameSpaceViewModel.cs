using Microsoft.Practices.ServiceLocation;
using Rock.Common;
using Rock.DesignerModule.Interface;
using Rock.DesignerModule.Models;
using Rock.DesignerModule.Service;
using Rock.Orm.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace Rock.DesignerModule.ViewModels
{
    public class NameSpaceViewModel : ViewModelBase
    {
        public ISystemService SystemService
        {
            get { return ServiceLocator.Current.GetInstance<ISystemService>(); }
        }
        public ApplicationDesignService ApplicationDesignService
        {
            get { return ServiceLocator.Current.GetInstance<ApplicationDesignService>(); }
        }       
        private Namespace _namespace;
        private string _editState;

        public NamespaceManagerViewModel NamespaceManagerViewModel
        {
            get { return ServiceLocator.Current.GetInstance<NamespaceManagerViewModel>(); }
        }       

        public Namespace Namespace
        {
            get { return _namespace; }
            set { _namespace = value; }
        }

        public string EditState
        {
            get { return _editState; }
            set { _editState = value; }
        }

        public NameSpaceViewModel()
        {
            this.Namespace = new Namespace();
        }

        public bool AddNamespace()
        {
            DynEntity namespaceDynEntity = new DynEntity("Namespace");
            Namespace.NamespaceID = SystemService.GetNextID("Namespace");
            namespaceDynEntity["NamespaceID"] = Namespace.NamespaceID;
            namespaceDynEntity["NamespaceName"] = Namespace.NamespaceName;
            namespaceDynEntity["Description"] = Namespace.Description;
            try
            {
                SystemService.AddDynEntity(namespaceDynEntity);                
                NamespaceManagerViewModel.NamespaceSource.Add(Namespace);
                if (!NamespaceManagerViewModel.NamespaceSource.Contains(Namespace))
                {
                    ApplicationDesignCache.NamespaceSource.Add(Namespace);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public bool EditNamespace()
        {
            DynEntity namespaceDynEntity = SystemService.GetDynEntityByID("Namespace", Namespace.NamespaceID);
            namespaceDynEntity["NamespaceName"] = Namespace.NamespaceName;
            namespaceDynEntity["Description"] = Namespace.Description;
            SystemService.ModifyDynEntity(namespaceDynEntity);
            return true;
        }
    }
}
