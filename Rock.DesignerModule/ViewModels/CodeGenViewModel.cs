using CSScriptLibrary;
using Microsoft.Practices.ServiceLocation;
using Rock.Common;
using Rock.DesignerModule.Interface;
using Rock.DesignerModule.Models;
using Rock.DesignerModule.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace Rock.DesignerModule.ViewModels
{
    public class CodeGenViewModel : ViewModelBase
    {
        public ISystemService SystemService
        {
            get { return ServiceLocator.Current.GetInstance<ISystemService>(); }
        }
        public ApplicationDesignService ApplicationDesignService
        {
            get { return ServiceLocator.Current.GetInstance<ApplicationDesignService>(); }
        }
        private ObservableCollection<DesignClass> _classDataSource = new ObservableCollection<DesignClass>();
        private string _outputNamespace;
        private string _entityText;
        public ObservableCollection<DesignClass> ClassDataSource
        {
            get { return _classDataSource; }
            set { _classDataSource = value; this.OnPropertyChanged("ClassDataSource"); }
        }
        public string OutputNamespace
        {
            get { return _outputNamespace; }
            set
            {
                if (_outputNamespace != value)
                {
                    _outputNamespace = value;
                    this.OnPropertyChanged("OutputNamespace");
                }
            }
        }
        public string EntityText
        {
            get { return _entityText; }
            set
            {
                if (_entityText != value)
                {
                    _entityText = value;
                    this.OnPropertyChanged("EntityText");
                }
            }
        }
        public ICommand GenerateCodeCommand { get; private set; }
        //
        public CodeGenViewModel()
        {
            OutputNamespace = "Rock.StaticEntities";
            GenerateCodeCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(GenerateCode);
        }

        private void GenerateCode()
        {
            if (ClassDataSource.Count > 0)
            {
                StringBuilder assemblyStringBuilder = new StringBuilder();
                StringBuilder entityStringBuilder = null;
                assemblyStringBuilder.Append("using System;" + Environment.NewLine);
                assemblyStringBuilder.Append("using System.Collections.Generic;" + Environment.NewLine);
                //assemblyStringBuilder.Append("using System.Data;" + Environment.NewLine);
                assemblyStringBuilder.Append("using Rock.Orm.Common.Design;" + Environment.NewLine);
                assemblyStringBuilder.Append("namespace " + OutputNamespace + Environment.NewLine);
                assemblyStringBuilder.Append("{" + Environment.NewLine);
                string baseClassName = " : Rock.Orm.Common.Design.Entity";
                foreach (var designClass in ClassDataSource)
                {
                    entityStringBuilder = new StringBuilder();
                    //[Relation]
                    if (designClass.MainType == 2)
                    {
                        entityStringBuilder.Append("[Relation]");
                        entityStringBuilder.Append(Environment.NewLine);
                    }
                    entityStringBuilder.Append("public interface " + designClass.ClassName + baseClassName);
                    entityStringBuilder.Append(Environment.NewLine);
                    entityStringBuilder.Append("{" + Environment.NewLine);
                    designClass.Properties = ApplicationDesignService.GetClassDesignPropertyCollection(designClass.ClassID);
                    foreach (var property in designClass.Properties)
                    {
                        //构造Atribute  结果 += "   " + attrStr + CSharpDynClass.换行回车符;
                        if (property.IsPrimarykey)
                        {
                            if (string.IsNullOrEmpty(property.StructName))
                            {
                                entityStringBuilder.Append("   [PrimaryKey()]");
                                entityStringBuilder.Append(Environment.NewLine);
                            }    
                            else
                            {
                                entityStringBuilder.Append("   [RelationKey(typeof(" + property.StructName + "))]");
                                entityStringBuilder.Append(Environment.NewLine);
                            }
                        }
                        else
                        {
                            if (!property.IsNullable)
                            {
                                entityStringBuilder.Append("   [NotNull]");
                                entityStringBuilder.Append(Environment.NewLine);
                            }
                        }
                       
                        string dataType = string.Empty;
                        switch (property.DataType)
                        {
                            case "String":
                                dataType = "string";
                                if (property.DbFieldLength > 0)
                                {
                                    entityStringBuilder.Append("   [SqlType(\"nvarchar(" + property.DbFieldLength + "\")]");
                                }
                                else
                                {
                                    entityStringBuilder.Append("   [SqlType(\"nvarchar(MAX\")]");
                                }
                                entityStringBuilder.Append(Environment.NewLine);
                                break;
                            case "I32":
                                if (property.IsNullable)
                                {
                                    dataType = "int?";
                                }
                                else
                                {
                                    dataType = "int";
                                }
                                break;
                            case "Bool":
                                if (property.IsNullable)
                                {
                                    dataType = "bool?";
                                }
                                else
                                {
                                    dataType = "bool";
                                }
                                break;
                            case "DateTime":
                                if (property.IsNullable)
                                {
                                    dataType = "DateTime?";
                                }
                                else
                                {
                                    dataType = "DateTime";
                                }
                                break;
                            case "Decimal":
                                entityStringBuilder.Append("   [SqlType(\"decimal(" + property.DbFieldLength + "," +  property.DecimalDigits + "\")]");
                                entityStringBuilder.Append(Environment.NewLine);
                                if (property.IsNullable)
                                {
                                    dataType = "decimal?";
                                }
                                else
                                {
                                    dataType = "decimal";
                                }
                                break;
                            case "Struct":
                                if (property.RelationType == "一对多")
                                {
                                    entityStringBuilder.Append("   [FriendKey(typeof(" + property.StructName + "))]");
                                    entityStringBuilder.Append(Environment.NewLine);
                                    entityStringBuilder.Append("   [FkReverseQuery(LazyLoad = true)]");
                                    entityStringBuilder.Append(Environment.NewLine);
                                    entityStringBuilder.Append("   [MappingName(\"" + property.DbFieldName + "\")]");
                                    entityStringBuilder.Append(Environment.NewLine);
                                    entityStringBuilder.Append("   [SerializationIgnore]");
                                    entityStringBuilder.Append(Environment.NewLine);

                                }
                                else
                                {                                    
                                    if (designClass.MainType == 0)
                                    {
                                        entityStringBuilder.Append("   [FkQuery(\"" + designClass.ClassName + "ID\",LazyLoad = true)]");
                                        entityStringBuilder.Append(Environment.NewLine);
                                        entityStringBuilder.Append("   [SerializationIgnore]");
                                        entityStringBuilder.Append(Environment.NewLine);

                                    }
                                }
                                dataType = property.StructName;
                                break;
                            case "Binary":
                                dataType = "byte[]";
                                break;
                            case "I16":
                                if (property.IsNullable)
                                {
                                    dataType = "short?";
                                }
                                else
                                {
                                    dataType = "short";
                                }
                                break;

                            case "I64":
                                if (property.IsNullable)
                                {
                                    dataType = "long?";
                                }
                                else
                                {
                                    dataType = "long";
                                }
                                break;
                            case "Byte":
                                if (property.IsNullable)
                                {
                                    dataType = "byte?";
                                }
                                else
                                {
                                    dataType = "byte";
                                }
                                break;
                            case "Double":
                                if (property.IsNullable)
                                {
                                    dataType = "double?";
                                }
                                else
                                {
                                    dataType = "double";
                                }
                                break;
                            default:
                                throw new Exception("数据类型错误:" + property.DataType);
                        }

                        switch (property.CollectionType)
                        {
                            case "None":
                                if (property.DataType == "Struct")
                                {
                                    entityStringBuilder.Append("   " + property.PropertyName + " " + property.PropertyName + "    { get; set; }");
                                }
                                else
                                {
                                    entityStringBuilder.Append("    " + dataType + " " + property.PropertyName + "    { get; set; }");
                                }
                                break;
                            case "List":
                                entityStringBuilder.Append("    " + property.StructName + "[] " + property.PropertyName + "    { get; set; }");
                                break;
                            default:
                                break;
                        }
                        entityStringBuilder.Append(Environment.NewLine);
                    }
                    entityStringBuilder.Append("}" + Environment.NewLine);
                    assemblyStringBuilder.Append(entityStringBuilder.ToString());
                }
                assemblyStringBuilder.Append("}");

                Assembly ass = CSScript.LoadCode(assemblyStringBuilder.ToString());
                CodeGenHelper codeGenHelper = new CodeGenHelper(OutputNamespace);
                EntityText = codeGenHelper.GenEntitiesEx(ass);
            }
        }
    }
}
