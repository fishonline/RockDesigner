//===================================================================================
using System;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Rock.DesignerModule.Models;
using Rock.Dyn.Core;
using Rock.DesignerModule.Service;
using Microsoft.Practices.ServiceLocation;
using Rock.DesignerModule.ViewModels;
namespace Rock.DesignerModule
{

    [ModuleExport(typeof(DesignModule))]
    public class DesignModule : IModule
    {
        private readonly IRegionViewRegistry regionViewRegistry;
        public ApplicationDesignService ApplicationDesignService
        {
            get { return ServiceLocator.Current.GetInstance<ApplicationDesignService>(); }
        }
        public DocumentControlViewModel DocumentControlViewModel
        {
            get { return ServiceLocator.Current.GetInstance<DocumentControlViewModel>(); }
        } 

        [ImportingConstructor]
        public DesignModule(IRegionViewRegistry registry)
        {
            this.regionViewRegistry = registry;   
        }

        public void Initialize()
        {            
            regionViewRegistry.RegisterViewWithRegion("MenuControl", typeof(Views.MenuControl));
            regionViewRegistry.RegisterViewWithRegion("DocumentControl", typeof(Views.DocumentControl));
            #region 初始化基本参照缓存
            ApplicationDesignCache.CollectionTypeSource.Add("None");
            ApplicationDesignCache.CollectionTypeSource.Add("List");

            ApplicationDesignCache.DynTypeSource.Add("Binary");
            ApplicationDesignCache.DynTypeSource.Add("Bool");
            ApplicationDesignCache.DynTypeSource.Add("I16");
            ApplicationDesignCache.DynTypeSource.Add("I32");
            ApplicationDesignCache.DynTypeSource.Add("I64");
            ApplicationDesignCache.DynTypeSource.Add("String");
            ApplicationDesignCache.DynTypeSource.Add("Byte");
            ApplicationDesignCache.DynTypeSource.Add("Double");
            ApplicationDesignCache.DynTypeSource.Add("Decimal");
            ApplicationDesignCache.DynTypeSource.Add("DateTime");
            ApplicationDesignCache.DynTypeSource.Add("Struct");

            ApplicationDesignCache.ResultTypeSource.Add("Void");
            ApplicationDesignCache.ResultTypeSource.Add("Binary");
            ApplicationDesignCache.ResultTypeSource.Add("Bool");
            ApplicationDesignCache.ResultTypeSource.Add("I16");
            ApplicationDesignCache.ResultTypeSource.Add("I32");
            ApplicationDesignCache.ResultTypeSource.Add("I64");
            ApplicationDesignCache.ResultTypeSource.Add("String");
            ApplicationDesignCache.ResultTypeSource.Add("Byte");
            ApplicationDesignCache.ResultTypeSource.Add("Double");
            ApplicationDesignCache.ResultTypeSource.Add("Decimal");
            ApplicationDesignCache.ResultTypeSource.Add("DateTime");
            ApplicationDesignCache.ResultTypeSource.Add("Struct");            

            ApplicationDesignCache.RelationTypeSource.Add("无");
            ApplicationDesignCache.RelationTypeSource.Add("一对多");
            ApplicationDesignCache.RelationTypeSource.Add("多对一");

            ApplicationDesignCache.SqlTypeSource.Add("nvarchar");
            ApplicationDesignCache.SqlTypeSource.Add("int");
            ApplicationDesignCache.SqlTypeSource.Add("datetime");
            ApplicationDesignCache.SqlTypeSource.Add("date");
            ApplicationDesignCache.SqlTypeSource.Add("bit");
            ApplicationDesignCache.SqlTypeSource.Add("decimal");
            ApplicationDesignCache.SqlTypeSource.Add("bigint");
            ApplicationDesignCache.SqlTypeSource.Add("binary");
            ApplicationDesignCache.SqlTypeSource.Add("char");
            ApplicationDesignCache.SqlTypeSource.Add("float");
            ApplicationDesignCache.SqlTypeSource.Add("nchar");
            ApplicationDesignCache.SqlTypeSource.Add("smalldatetime");
            ApplicationDesignCache.SqlTypeSource.Add("smallint");
            ApplicationDesignCache.SqlTypeSource.Add("time");
            ApplicationDesignCache.SqlTypeSource.Add("tinyint");
            ApplicationDesignCache.SqlTypeSource.Add("uniqueidentifier");
            ApplicationDesignCache.SqlTypeSource.Add("varbinary");
            ApplicationDesignCache.SqlTypeSource.Add("varchar");

            ApplicationDesignCache.ScriptTypeSource.Add("Dll");

            ApplicationDesignCache.ValidateTypeSource.Add("None");
            ApplicationDesignCache.ValidateTypeSource.Add("Number");
            ApplicationDesignCache.ValidateTypeSource.Add("Date");

            ApplicationDesignCache.InputTypeSource.Add("TextBox");
            ApplicationDesignCache.InputTypeSource.Add("Date");
            ApplicationDesignCache.InputTypeSource.Add("Combox");
            ApplicationDesignCache.InputTypeSource.Add("CheckBox");
            ApplicationDesignCache.InputTypeSource.Add("MultiTextBox");
            ApplicationDesignCache.InputTypeSource.Add("Hidden");

            ApplicationDesignCache.GridColSortingSource.Add("str");
            ApplicationDesignCache.GridColSortingSource.Add("int");
            ApplicationDesignCache.GridColSortingSource.Add("date");
            ApplicationDesignCache.GridColSortingSource.Add("na");

            ApplicationDesignCache.GridColTypeSource.Add("ro");
            ApplicationDesignCache.GridColTypeSource.Add("ch");
            ApplicationDesignCache.GridColTypeSource.Add("ed");

            ApplicationDesignCache.GridColAlignSource.Add("left");
            ApplicationDesignCache.GridColAlignSource.Add("center");
            ApplicationDesignCache.GridColAlignSource.Add("right");
            ApplicationDesignCache.GridColAlignSource.Add("justify");

            ApplicationDesignCache.QueryFormSource.Add("Fuzzy");
            ApplicationDesignCache.QueryFormSource.Add("Date");
            ApplicationDesignCache.QueryFormSource.Add("Value");
            ApplicationDesignCache.QueryFormSource.Add("Combox");
            ApplicationDesignCache.QueryFormSource.Add("Tree");
            ApplicationDesignCache.QueryFormSource.Add("Quick");            

            ApplicationDesignCache.NamespaceSource = ApplicationDesignService.GetAllNamespaceCollection();
            ApplicationDesignCache.StructSource = ApplicationDesignService.GetAllEntityStructCollection();

            ApplicationDesignCache.ApplicationID = 0;
            ApplicationDesignCache.ApplicationName = "设计器";

            DocumentControlViewModel.OpenDesignerView();

            #endregion 初始化基本参照缓存
        }
    }
}
