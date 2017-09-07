using Rock.Dyn.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Rock.DesignerModule.Models
{
    public class ApplicationDesignCache
    {
        public static int ApplicationID;
        public static string ApplicationName;

        public static BitmapSource ApplicationImage = new BitmapImage(new Uri("/Rock.DesignerModule;component/Images/Application.png", UriKind.RelativeOrAbsolute));
        public static BitmapSource ModuleClassImage = new BitmapImage(new Uri("/Rock.DesignerModule;component/Images/Module.png", UriKind.RelativeOrAbsolute));
        public static BitmapSource EntityClassImage = new BitmapImage(new Uri("/Rock.DesignerModule;component/Images/EntityClass.png", UriKind.RelativeOrAbsolute));
        public static BitmapSource ControlClassImage = new BitmapImage(new Uri("/Rock.DesignerModule;component/Images/ControlClass.png", UriKind.RelativeOrAbsolute));
        public static BitmapSource AttributeClassImage = new BitmapImage(new Uri("/Rock.DesignerModule;component/Images/AttributeClass.png", UriKind.RelativeOrAbsolute));
        public static BitmapSource RelationClassImage = new BitmapImage(new Uri("/Rock.DesignerModule;component/Images/RelationClass.png", UriKind.RelativeOrAbsolute));
        public static BitmapSource FuncationClassImage = new BitmapImage(new Uri("/Rock.DesignerModule;component/Images/FuncationClass.png", UriKind.RelativeOrAbsolute));

        public static ObservableCollection<Namespace> NamespaceSource = new ObservableCollection<Namespace>();
        public static ObservableCollection<string> StructSource = new ObservableCollection<string>();
        public static ObservableCollection<string> RelationTypeSource = new ObservableCollection<string>();
        public static ObservableCollection<string> SqlTypeSource = new ObservableCollection<string>();
        public static ObservableCollection<string> CollectionTypeSource = new ObservableCollection<string>();
        public static ObservableCollection<string> DynTypeSource = new ObservableCollection<string>();
        public static ObservableCollection<string> ResultTypeSource = new ObservableCollection<string>();
        public static ObservableCollection<string> ScriptTypeSource = new ObservableCollection<string>();
        public static ObservableCollection<string> ValidateTypeSource = new ObservableCollection<string>();
        public static ObservableCollection<string> InputTypeSource = new ObservableCollection<string>();
        public static ObservableCollection<string> GridColSortingSource = new ObservableCollection<string>();
        public static ObservableCollection<string> GridColTypeSource = new ObservableCollection<string>();
        public static ObservableCollection<string> GridColAlignSource = new ObservableCollection<string>();
        public static ObservableCollection<string> QueryFormSource = new ObservableCollection<string>();
        
    }
}
