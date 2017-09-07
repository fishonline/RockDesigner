#if DEBUG
#define DBC_CHECK_ALL
#else
#define DBC_CHECK_PRECONDITION
#endif
using Microsoft.Practices.ServiceLocation;
using Rock.Common;
using Rock.DesignerModule.Interface;
using Rock.DesignerModule.Models;
using Rock.Dyn.Core;
using Rock.Orm.Common;
using Rock.Orm.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Rock.DesignerModule.Service
{
    [Export]
    public class ApplicationDesignService
    {
        public ISystemService SystemService
        {
            get { return ServiceLocator.Current.GetInstance<ISystemService>(); }
        }

        #region 应用程序模块维护
        //获取所有应用程序
        public List<DynEntity> GetAllApplicationCollection()
        {
            List<DynEntity> ApplicationCollection = new List<DynEntity>();
            DynEntity[] ApplicationEntities = GatewayFactory.Default.FindArray("Application");
            for (int i = 0; i < ApplicationEntities.Length; i++)
            {
                ApplicationCollection.Add(ApplicationEntities[i]);
            }
            return ApplicationCollection;
        }

        //是否可以删除Application
        public bool CanDeleteApplication(int applicationID)
        {
            string strSql = "select COUNT(*) from ApplicationModule where ApplicationID = " + applicationID;
            return Convert.ToInt32(SystemService.ExecuteScalar(strSql)) == 0;
        }
        public List<DynEntity> GetAllApplictionModuleCollection()
        {
            List<DynEntity> ApplictionModuleCollection = new List<DynEntity>();
            DynEntity[] ApplictionModuleEntities = GatewayFactory.Default.FindArray("Module");
            for (int i = 0; i < ApplictionModuleEntities.Length; i++)
            {
                ApplictionModuleCollection.Add(ApplictionModuleEntities[i]);
            }
            return ApplictionModuleCollection;
        }

        public bool CanDeleteAplicationModule(int moduleID)
        {
            string strSql = "select COUNT(*) from ApplicationModule where ModuleID = " + moduleID;
            return Convert.ToInt32(SystemService.ExecuteScalar(strSql)) == 0;
        }

        public List<DynEntity> GetAplicationModulesByAplicationID(int applicationID)
        {
            List<DynEntity> ApplictionModules = new List<DynEntity>();

            DynEntity[] applictionModules = GatewayFactory.Default.FindArray("ApplicationModule", _.P("ApplicationModule", "ApplicationID") == applicationID);
            List<DynEntity> ApplictionModuleEntities = new List<DynEntity>();
            foreach (var applictionModule in applictionModules)
            {
                ApplictionModuleEntities.Add(GatewayFactory.Default.Find("Module", _.P("Module", "ModuleID") == (int)applictionModule["ModuleID"]));
            }

            if (ApplictionModuleEntities.Count != 0)
            {
                foreach (var ApplictionModuleEntity in ApplictionModuleEntities)
                {
                    ApplictionModules.Add(ApplictionModuleEntity);
                }
            }

            return ApplictionModules;
        }

        #endregion 应用程序模块维护

        #region 命名空间维护

        //是否可以删除Application
        public bool CanDeleteNamespace(int namepaceID)
        {
            string strSql = "select COUNT(*) from DynClass where NamespaceID = " + namepaceID;
            return Convert.ToInt32(SystemService.ExecuteScalar(strSql)) == 0;
        }
        #endregion 命名空间维护

        #region 应用程序设计
        public List<DynEntity> GetModuleClassEntityCollection(int moduleID)
        {
            List<DynEntity> moduleClassEntityCollection = new List<DynEntity>();
            DynEntity[] moduleClassEntities = GatewayFactory.Default.FindArray("DynClass", _.P("DynClass", "ModuleID") == moduleID && _.P("DynClass", "MainType") != 3, _.P("DynClass", "MainType").Asc);
            for (int i = 0; i < moduleClassEntities.Length; i++)
            {
                moduleClassEntityCollection.Add(moduleClassEntities[i]);
            }
            return moduleClassEntityCollection;
        }
        public ObservableCollection<DesignClass> GetModuleClassCollection(int moduleID)
        {
            ObservableCollection<DesignClass> moduleClassCollection = new ObservableCollection<DesignClass>();
            DesignClass designClassModel;
            string strSql = "select DynClassID, DynClassName, DisplayName, BaseClassName, MainType, Description from DynClass where ModuleID = " + moduleID + " and MainType <> 3 ";
            DataTable dataTable = SystemService.GetDataTable(strSql);
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                designClassModel = new DesignClass();
                designClassModel.ClassID = Convert.ToInt32(dataTable.Rows[i]["DynClassID"]);
                designClassModel.ClassName = dataTable.Rows[i]["DynClassName"].ToString();
                if (dataTable.Rows[i]["DisplayName"] != null)
                {
                    designClassModel.DisplayName = dataTable.Rows[i]["DisplayName"].ToString();
                }
                if (dataTable.Rows[i]["BaseClassName"] != null)
                {
                    designClassModel.BaseClassName = dataTable.Rows[i]["BaseClassName"].ToString();
                }
                designClassModel.MainType = Convert.ToInt32(dataTable.Rows[i]["MainType"]);
                if (dataTable.Rows[i]["Description"] != null)
                {
                    designClassModel.Description = dataTable.Rows[i]["Description"].ToString();
                }

                switch (designClassModel.MainType)
                {
                    case 0:
                        designClassModel.MainTypeImage = ApplicationDesignCache.EntityClassImage;
                        break;
                    case 1:
                        designClassModel.MainTypeImage = ApplicationDesignCache.ControlClassImage;
                        break;
                    case 4:
                        designClassModel.MainTypeImage = ApplicationDesignCache.FuncationClassImage;
                        break;
                    case 5:
                        designClassModel.MainTypeImage = ApplicationDesignCache.RelationClassImage;
                        break;
                    default:
                        break;
                }

                moduleClassCollection.Add(designClassModel);
            }
            return moduleClassCollection;
        }
        public ObservableCollection<Namespace> GetAllNamespaceCollection()
        {
            ObservableCollection<Namespace> namespaceCollection = new ObservableCollection<Namespace>();

            DynEntity[] namespaceEntityClasses = GatewayFactory.Default.FindArray("Namespace");

            if (namespaceEntityClasses != null)
            {
                foreach (var namespaceEntityClass in namespaceEntityClasses)
                {
                    Namespace namespaceModel = new Namespace();
                    namespaceModel.NamespaceID = Convert.ToInt32(namespaceEntityClass["NamespaceID"]);
                    namespaceModel.NamespaceName = namespaceEntityClass["NamespaceName"].ToString();
                    namespaceModel.Description = namespaceEntityClass["Description"] as string;
                    if (namespaceModel.NamespaceName != "Rock.Core.Entities")
                    {
                        namespaceCollection.Add(namespaceModel);
                    }
                }
            }

            return namespaceCollection;
        }
        public ObservableCollection<string> GetAllEntityStructCollection()
        {
            ObservableCollection<string> entityStructCollection = new ObservableCollection<string>();

            entityStructCollection.Add(string.Empty);
            entityStructCollection.Add("Entity");

            string strSql = "select DynClassName from DynClass where MainType in (0, 2) order by DynClassName ";
            DataTable dataTable = SystemService.GetDataTable(strSql);
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {

                if (dataTable.Rows[i]["DynClassName"] != DBNull.Value)
                {
                    entityStructCollection.Add(dataTable.Rows[i]["DynClassName"] as string);
                }
            }

            return entityStructCollection;
        }

        public ObservableCollection<DesignProperty> GetClassDesignPropertyCollection(int classID)
        {
            ObservableCollection<DesignProperty> designPropertyCollection = new ObservableCollection<DesignProperty>();
            DynEntity[] dynEntityProperties = GatewayFactory.Default.FindArray("DynProperty", _.P("DynProperty", "DynClassID") == classID);
            foreach (var dynProperty in dynEntityProperties)
            {
                DesignProperty designProperty = new DesignProperty();
                designProperty.PropertyID = Convert.ToInt32(dynProperty["DynPropertyID"]);
                designProperty.PropertyName = dynProperty["DynPropertyName"] as string;
                designProperty.DisplayName = dynProperty["DisplayName"] as string;
                designProperty.Description = dynProperty["Description"] as string;
                designProperty.CollectionType = dynProperty["CollectionType"] as string;
                designProperty.DataType = dynProperty["Type"] as string;
                designProperty.StructName = dynProperty["StructName"] as string;
                designProperty.RelationType = "无";
                designProperty.IsNullable = true;
                designProperty.IsQueryProperty = Convert.ToBoolean(dynProperty["IsQueryProperty"]);
                designProperty.IsArray = Convert.ToBoolean(dynProperty["IsArray"]);
                designProperty.State = "normal";
                designProperty.IsPropertyChanged = false;
                designProperty.OriginalDataType = dynProperty["Type"] as string;
                string dynPropertyAtrributeStr = dynProperty["Attributes"] as string;

                List<DynObject> dynPropertyAttributes = DynObjectTransverter.JsonToDynObjectList(dynPropertyAtrributeStr);
                foreach (var item in dynPropertyAttributes)
                {
                    switch (item.DynClass.Name)
                    {
                        case "PrimaryKey":
                            designProperty.IsPrimarykey = true;
                            break;
                        case "NotNull":
                            designProperty.IsNullable = false;
                            break;
                        case "SqlType":
                            string trueType = item["Type"] as string;
                            int startIndex = trueType.IndexOf("(");
                            int middleIndex = trueType.IndexOf(",");
                            int endIndex = trueType.IndexOf(")");
                            if (startIndex < 0)
                            {
                                designProperty.SqlType = trueType;
                            }
                            else
                            {
                                designProperty.SqlType = trueType.Substring(0, startIndex);
                                if (designProperty.SqlType == "decimal")
                                {
                                    if (startIndex > 0 && middleIndex > 0 && endIndex > 0)
                                    {
                                        designProperty.DbFieldLength = Convert.ToInt32(trueType.Substring(startIndex + 1, middleIndex - startIndex - 1));
                                        designProperty.DecimalDigits = Convert.ToInt32(trueType.Substring(middleIndex + 1, endIndex - middleIndex - 1));
                                    }
                                }
                                else
                                {
                                    if (startIndex > 0 && endIndex > 0)
                                    {
                                        string length = trueType.Substring(startIndex + 1, endIndex - startIndex - 1);
                                        if (length.ToUpper() == "MAX")
                                        {
                                            designProperty.DbFieldLength = 0;
                                        }
                                        else
                                        {
                                            designProperty.DbFieldLength = Convert.ToInt32(length);
                                        }
                                    }
                                }
                            }
                            break;
                        case "PersistIgnore":
                            designProperty.IsPersistable = false;
                            break;
                        case "MappingName":
                            designProperty.DbFieldName = item["Name"] as string;
                            break;
                        case "FkReverseQuery":
                            designProperty.RelationType = "一对多";
                            break;
                        case "FkQuery":
                            designProperty.RelationType = "多对一";
                            break;
                        case "DesignInfo":
                            DesignInfo designInfo = new DesignInfo();
                            designInfo.GridColAlign = item["GridColAlign"].ToString();
                            designInfo.GridColSorting = item["GridColSorting"].ToString();
                            designInfo.GridColType = item["GridColType"].ToString();
                            if (item["GridHeader"] == null)
                            {
                                designInfo.GridHeader = "";
                            }
                            else
                            {
                                designInfo.GridHeader = item["GridHeader"].ToString();
                            }
                            designInfo.GridWidth = (int)item["GridWidth"];
                            designInfo.InputType = item["InputType"].ToString();
                            designInfo.IsRequired = (bool)item["IsRequired"];
                            designInfo.IsReadOnly = (bool)item["IsReadOnly"];
                            if (item["QueryForm"] == null)
                            {
                                designInfo.QueryForm = "Fuzzy";
                            }
                            else
                            {
                                designInfo.QueryForm = item["QueryForm"].ToString();
                            }
                            designInfo.ReferType = item["ReferType"] as string;
                            designInfo.ValidateType = item["ValidateType"].ToString();
                            designInfo.State = "normal";
                            designInfo.IsPropertyChanged = false;
                            designProperty.UIDesignInfo = designInfo;
                            break;
                    }
                }
                designPropertyCollection.Add(designProperty);
            }
            return designPropertyCollection;
        }
        public ObservableCollection<DesignMethod> GetClassDesignMethodCollection(int classID)
        {
            ObservableCollection<DesignMethod> designMethodCollection = new ObservableCollection<DesignMethod>();
            DynEntity[] dynEntityMethods = GatewayFactory.Default.FindArray("DynMethod", _.P("DynMethod", "DynClassID") == classID);
            foreach (var dynEntityMethod in dynEntityMethods)
            {
                DesignMethod designMethod = new DesignMethod();
                designMethod.MethodID = Convert.ToInt32(dynEntityMethod["DynMethodID"]);
                designMethod.MethodName = dynEntityMethod["DynMethodName"] as string;
                designMethod.DisplayName = dynEntityMethod["DisplayName"] as string;
                designMethod.Description = dynEntityMethod["Description"] as string;
                designMethod.State = "normal";
                designMethod.IsMethodChanged = false;
                //Attribute处理
                List<DynObject> dynMethodAttributes = DynObjectTransverter.JsonToDynObjectList(dynEntityMethod["Attributes"] as string);
                foreach (var dynMethodAttribute in dynMethodAttributes)
                {
                    switch (dynMethodAttribute.DynClass.Name)
                    {
                        case "OperationContract":
                            designMethod.IsOperationProtocol = true;
                            break;
                    }
                }
                //参数处理
                string methodObjectString = dynEntityMethod["Definition"] as string;
                DynObject dynObjectMethod = DynObjectTransverter.JsonToDynObject(methodObjectString);
                List<DynObject> dynObjectParamenters = dynObjectMethod["Parameters"] as List<DynObject>;
                foreach (var dynObjectParamenter in dynObjectParamenters)
                {
                    DesignMethodParameter methodParameter = new DesignMethodParameter();
                    methodParameter.ParameterName = dynObjectParamenter["ParameterName"] as string;
                    methodParameter.CollectionType = dynObjectParamenter["CollectionType"] as string;
                    methodParameter.DataType = dynObjectParamenter["Type"] as string;
                    methodParameter.StructName = dynObjectParamenter["StructName"] as string;
                    methodParameter.Description = dynObjectParamenter["Description"] as string;
                    methodParameter.State = "normal";
                    methodParameter.IsParameterChanged = false;
                    designMethod.Parameters.Add(methodParameter);
                }
                //返回结果
                DynObject resultParamenter = dynObjectMethod["Result"] as DynObject;
                designMethod.ResultCollectionType = resultParamenter["CollectionType"] as string;
                designMethod.ResultDataType = resultParamenter["Type"] as string;
                designMethod.ResultStructName = resultParamenter["StructName"] as string;

                designMethodCollection.Add(designMethod);
            }

            return designMethodCollection;
        }
        public DesignMethod GetDesignMethod(int methodID)
        {
            DynEntity dynEntityMethod = GatewayFactory.Default.Find("DynMethod", methodID);
            DesignMethod designMethod = new DesignMethod();
            designMethod.MethodID = Convert.ToInt32(dynEntityMethod["DynMethodID"]);
            designMethod.MethodName = dynEntityMethod["DynMethodName"] as string;
            designMethod.DisplayName = dynEntityMethod["DisplayName"] as string;
            designMethod.Description = dynEntityMethod["Description"] as string;
            designMethod.State = "normal";
            designMethod.IsMethodChanged = false;
            //参数处理
            string methodObjectString = dynEntityMethod["Definition"] as string;
            DynObject dynObjectMethod = DynObjectTransverter.JsonToDynObject(methodObjectString);
            List<DynObject> dynObjectParamenters = dynObjectMethod["Parameters"] as List<DynObject>;
            foreach (var dynObjectParamenter in dynObjectParamenters)
            {
                DesignMethodParameter methodParameter = new DesignMethodParameter();
                methodParameter.ParameterName = dynObjectParamenter["ParameterName"] as string;
                methodParameter.CollectionType = dynObjectParamenter["CollectionType"] as string;
                methodParameter.DataType = dynObjectParamenter["Type"] as string;
                methodParameter.StructName = dynObjectParamenter["StructName"] as string;
                methodParameter.Description = dynObjectParamenter["Description"] as string;
                methodParameter.State = "normal";
                methodParameter.IsParameterChanged = false;
                designMethod.Parameters.Add(methodParameter);
            }
            //返回结果
            DynObject resultParamenter = dynObjectMethod["Result"] as DynObject;
            designMethod.ResultCollectionType = resultParamenter["CollectionType"] as string;
            designMethod.ResultDataType = resultParamenter["Type"] as string;
            designMethod.ResultStructName = resultParamenter["StructName"] as string;
            return designMethod;
        }
        public DesignProperty GetDesignProperty(int propertyID)
        {
            DynEntity dynProperty = GatewayFactory.Default.Find("DynProperty", propertyID);
            DesignProperty designProperty = new DesignProperty();
            designProperty.PropertyID = Convert.ToInt32(dynProperty["DynPropertyID"]);
            designProperty.PropertyName = dynProperty["DynPropertyName"] as string;
            designProperty.DisplayName = dynProperty["DisplayName"] as string;
            designProperty.Description = dynProperty["Description"] as string;
            designProperty.CollectionType = dynProperty["CollectionType"] as string;
            designProperty.DataType = dynProperty["Type"] as string;
            designProperty.StructName = dynProperty["StructName"] as string;
            designProperty.RelationType = "无";
            designProperty.IsNullable = true;
            designProperty.IsQueryProperty = Convert.ToBoolean(dynProperty["IsQueryProperty"]);
            designProperty.IsArray = Convert.ToBoolean(dynProperty["IsArray"]);
            designProperty.State = "normal";
            designProperty.IsPropertyChanged = false;
            designProperty.OriginalDataType = dynProperty["Type"] as string;
            string dynPropertyAtrributeStr = dynProperty["Attributes"] as string;

            List<DynObject> dynPropertyAttributes = DynObjectTransverter.JsonToDynObjectList(dynPropertyAtrributeStr);
            foreach (var item in dynPropertyAttributes)
            {
                switch (item.DynClass.Name)
                {
                    case "PrimaryKey":
                        designProperty.IsPrimarykey = true;
                        break;
                    case "NotNull":
                        designProperty.IsNullable = false;
                        break;
                    case "SqlType":
                        string trueType = item["Type"] as string;
                        int startIndex = trueType.IndexOf("(");
                        int middleIndex = trueType.IndexOf(",");
                        int endIndex = trueType.IndexOf(")");
                        if (startIndex < 0)
                        {
                            designProperty.SqlType = trueType;
                        }
                        else
                        {
                            designProperty.SqlType = trueType.Substring(0, startIndex);
                            if (designProperty.SqlType == "decimal")
                            {
                                if (startIndex > 0 && middleIndex > 0 && endIndex > 0)
                                {
                                    designProperty.DbFieldLength = Convert.ToInt32(trueType.Substring(startIndex + 1, middleIndex - startIndex - 1));
                                    designProperty.DecimalDigits = Convert.ToInt32(trueType.Substring(middleIndex + 1, endIndex - middleIndex - 1));
                                }
                            }
                            else
                            {
                                if (startIndex > 0 && endIndex > 0)
                                {
                                    string length = trueType.Substring(startIndex + 1, endIndex - startIndex - 1);
                                    if (length.ToUpper() == "MAX")
                                    {
                                        designProperty.DbFieldLength = 0;
                                    }
                                    else
                                    {
                                        designProperty.DbFieldLength = Convert.ToInt32(length);
                                    }
                                }
                            }
                        }
                        break;
                    case "PersistIgnore":
                        designProperty.IsPersistable = false;
                        break;
                    case "MappingName":
                        designProperty.DbFieldName = item["Name"] as string;
                        break;
                    case "FkReverseQuery":
                        designProperty.RelationType = "一对多";
                        break;
                    case "FkQuery":
                        designProperty.RelationType = "多对一";
                        break;
                    case "DesignInfo":
                        DesignInfo designInfo = new DesignInfo();
                        designInfo.GridColAlign = item["GridColAlign"] as string;
                        designInfo.GridColSorting = item["GridColSorting"] as string;
                        designInfo.GridColType = item["GridColType"] as string;
                        designInfo.GridHeader = item["GridHeader"] as string;
                        designInfo.GridWidth = (int)item["GridWidth"];
                        designInfo.InputType = item["InputType"].ToString();
                        designInfo.IsRequired = (bool)item["IsRequired"];
                        designInfo.IsReadOnly = (bool)item["IsReadOnly"];
                        designInfo.QueryForm = item["QueryForm"] as string;
                        designInfo.ReferType = item["ReferType"] as string;
                        designInfo.ValidateType = item["ValidateType"].ToString();
                        designInfo.State = "normal";
                        designInfo.IsPropertyChanged = false;
                        designProperty.UIDesignInfo = designInfo;
                        break;
                }
            }
            return designProperty;
        }

        public DynEntity GetClassEntity(int classID)
        {
            return GatewayFactory.Default.Find("DynClass", classID);
        }
        //添加新的类型对象
        public void AddDesignClass(DesignClass designClass)
        {
            Check.Require(designClass.ModuleID > 0, "类型对象的模块ID不为空");
            Check.Require(designClass.NamespaceID > 0, "类型对象的命名空间ID不为空");
            //检查类名不能重复
            string strSql = "select COUNT(*) from DynClass where DynClassName = '" + designClass.ClassName + "'";
            Check.Require(Convert.ToInt32(SystemService.ExecuteScalar(strSql)) == 0, "类型对象的名称在数据库中已经存在,不允许重复,请检查!");
            //检查属性是否重名
            List<string> propertyNames = new List<string>();
            foreach (var property in designClass.Properties)
            {
                Check.Require(!propertyNames.Contains(property.PropertyName), "一个类中不能有两个同名的属性,请检查!");
                propertyNames.Add(property.PropertyName);
            }

            DataTransactionContext dataTransactionContext = new DataTransactionContext();

            designClass.ClassID = SystemService.GetNextID("DynClass");
            //对象实体处理
            DynEntity classEntity = new DynEntity("DynClass");
            classEntity["DynClassID"] = designClass.ClassID;
            classEntity["DynClassName"] = designClass.ClassName;
            classEntity["DisplayName"] = designClass.DisplayName;
            classEntity["ModuleID"] = designClass.ModuleID;
            classEntity["NamespaceID"] = designClass.NamespaceID;
            classEntity["BaseClassName"] = designClass.BaseClassName;
            classEntity["MainType"] = designClass.MainType;
            classEntity["Description"] = designClass.Description;

            //对象Attribute处理
            List<DynObject> classAttributes = new List<DynObject>();
            if (designClass.IsPersistable)
            {
                DynObject persistableAttribute = new DynObject("Persistable");
                classAttributes.Add(persistableAttribute);
            }
            if (designClass.MainType == 2)
            {
                DynObject relationAttribute = new DynObject("Relation");
                classAttributes.Add(relationAttribute);
            }
            if (designClass.IsServiceProtocol)
            {
                DynObject serviceContractAttribute = new DynObject("ServiceContract");
                classAttributes.Add(serviceContractAttribute);
            }
            classEntity["Attributes"] = DynObjectTransverter.DynObjectListToJson(classAttributes);
            designClass.State = "normal";
            dataTransactionContext.SavedDynEntities.Add(classEntity);

            //服务接口处理
            DynEntity InterfaceEntity = null;
            if (designClass.IsServiceProtocol)
            {
                classEntity["InterfaceNames"] = "I" + designClass.ClassName;
                designClass.InterfaceName = "I" + designClass.ClassName;
                //检查接口是否存在
                strSql = "select COUNT(*) from DynClass where DynClassName = 'I" + designClass.ClassName + "'";
                Check.Require(Convert.ToInt32(SystemService.ExecuteScalar(strSql)) == 0, "控制类对应的接口已经存在,不允许重复,请检查!");
                //构造接口类实体
                InterfaceEntity = new DynEntity("DynClass");
                InterfaceEntity["DynClassID"] = SystemService.GetNextID("DynClass");
                InterfaceEntity["DynClassName"] = designClass.InterfaceName;
                InterfaceEntity["DisplayName"] = designClass.DisplayName;
                InterfaceEntity["BaseClassName"] = null;
                InterfaceEntity["Description"] = designClass.Description;
                InterfaceEntity["ModuleID"] = designClass.ModuleID;
                InterfaceEntity["InterfaceNames"] = null;
                InterfaceEntity["Attributes"] = null;
                InterfaceEntity["MainType"] = 3;
                InterfaceEntity["NamespaceID"] = designClass.NamespaceID;
                dataTransactionContext.SavedDynEntities.Add(InterfaceEntity);
            }

            //数据库表结构处理
            int columnLength = 0;
            StringBuilder commondSB = new StringBuilder();
            List<string> pkColumnName = new List<string>();
            string tableName = string.Empty;
            if (designClass.IsPersistable)
            {
                //类型数据库的处理
                tableName = designClass.ClassName;
                string baseClassName = designClass.BaseClassName;
                //判断数据库表是否存在
                bool isTableExist = GatewayFactory.Default.Db.ExecuteDataSet(CommandType.Text, string.Format("select name from sysobjects where id = object_id('{0}');", tableName)).Tables[0].Rows.Count > 0;
                Check.Require(!isTableExist, "数据库中有这个表了");
                commondSB.Append("create table [");
                commondSB.Append(tableName);
                commondSB.Append("]");
                commondSB.Append(" (");
                if (baseClassName != "Entity")
                {
                    columnLength++;
                    commondSB.Append("[" + baseClassName + "ID" + "]");
                    commondSB.Append(" ");
                    commondSB.Append("int");
                    commondSB.Append(" ");
                    commondSB.Append("NOT NULL");
                    commondSB.Append(",");
                    pkColumnName.Add(baseClassName + "ID");
                }
            }
            //方法处理
            foreach (var designMethode in designClass.Methodes)
            {
                //实体方法
                DynEntity methodEntity = new DynEntity("DynMethod");
                designMethode.MethodID = SystemService.GetNextID("DynMethod");
                methodEntity["DynMethodID"] = designMethode.MethodID;
                methodEntity["DynMethodName"] = designMethode.MethodName;
                methodEntity["DisplayName"] = designMethode.DisplayName;
                methodEntity["Description"] = designMethode.Description;
                methodEntity["ScriptType"] = designMethode.ScriptType;
                methodEntity["Body"] = designMethode.Body;
                methodEntity["DynClassID"] = designClass.ClassID;

                DynObject methodDynObject = new DynObject("Method");

                methodDynObject["MethodID"] = methodEntity["DynMethodID"];
                methodDynObject["MethodName"] = designMethode.MethodName;
                methodDynObject["DisplayName"] = designMethode.DisplayName;
                methodDynObject["Description"] = designMethode.Description;
                methodDynObject["ScriptType"] = designMethode.ScriptType;
                methodDynObject["Body"] = designMethode.Body;
                methodDynObject["ClassID"] = designClass.ClassID;

                //方法Attribute处理
                List<DynObject> attributes = new List<DynObject>();
                if (designMethode.IsOperationProtocol)
                {
                    DynObject attribute = new DynObject("OperationContract");
                    attribute["Name"] = "default";
                    attributes.Add(attribute);
                }
                methodEntity["Attributes"] = DynObjectTransverter.DynObjectListToJson(attributes);
                methodDynObject["Attributes"] = methodEntity["Attributes"];
                //方法参数处理
                List<DynObject> parameters = new List<DynObject>();
                for (int i = 0; i < designMethode.Parameters.Count; i++)
                {
                    DesignMethodParameter parameter = designMethode.Parameters[i];
                    DynObject parameterDynObject = new DynObject("Parameter");
                    parameterDynObject["ParameterID"] = i;
                    parameterDynObject["ParameterName"] = parameter.ParameterName;
                    parameterDynObject["CollectionType"] = parameter.CollectionType;
                    parameterDynObject["Type"] = parameter.DataType;
                    parameterDynObject["StructName"] = parameter.StructName;
                    parameterDynObject["Description"] = parameter.Description;
                    parameters.Add(parameterDynObject);
                    parameter.State = "normal";
                }
                methodDynObject["Parameters"] = parameters;
                //方法返回值处理
                DynObject methodResult = new DynObject("Parameter");
                methodResult["ParameterID"] = 0;
                methodResult["ParameterName"] = "Result";
                methodResult["CollectionType"] = designMethode.ResultCollectionType;
                methodResult["Type"] = designMethode.ResultDataType;
                methodResult["StructName"] = designMethode.ResultStructName;
                methodDynObject["Result"] = methodResult;
                methodEntity["Definition"] = DynObjectTransverter.DynObjectToJson(methodDynObject);
                designMethode.State = "normal";
                designMethode.IsMethodChanged = false;
                dataTransactionContext.SavedDynEntities.Add(methodEntity);

                //接口方法
                if (designMethode.IsOperationProtocol)
                {
                    DynEntity interfaceMethodEntity = new DynEntity("DynMethod");
                    interfaceMethodEntity["DynMethodID"] = SystemService.GetNextID("DynMethod");
                    interfaceMethodEntity["DynMethodName"] = designMethode.MethodName;
                    interfaceMethodEntity["DisplayName"] = designMethode.DisplayName;
                    interfaceMethodEntity["Description"] = designMethode.Description;
                    interfaceMethodEntity["DynClassID"] = InterfaceEntity["DynClassID"];
                    interfaceMethodEntity["Body"] = designMethode.Body;
                    interfaceMethodEntity["ScriptType"] = designMethode.ScriptType;
                    //这个属性用作存放实体方法的ID,以建立对应关系,便于维护
                    interfaceMethodEntity["SourceMethodID"] = methodEntity["DynMethodID"];

                    DynObject interfaceMethodDynObject = new DynObject("Method");

                    interfaceMethodDynObject["MethodID"] = interfaceMethodEntity["DynMethodID"];
                    interfaceMethodDynObject["MethodName"] = designMethode.MethodName;
                    interfaceMethodDynObject["DisplayName"] = designMethode.DisplayName;
                    interfaceMethodDynObject["Description"] = designMethode.Description;
                    interfaceMethodDynObject["ScriptType"] = designMethode.ScriptType;
                    interfaceMethodDynObject["Body"] = designMethode.Body;
                    interfaceMethodDynObject["ClassID"] = InterfaceEntity["DynClassID"];
                    interfaceMethodDynObject["SourceMethodID"] = methodEntity["DynMethodID"];
                    interfaceMethodDynObject["Parameters"] = parameters;
                    interfaceMethodDynObject["Result"] = methodResult;
                    interfaceMethodEntity["Definition"] = DynObjectTransverter.DynObjectToJson(interfaceMethodDynObject);
                    dataTransactionContext.SavedDynEntities.Add(interfaceMethodEntity);
                }
            }

            //属性处理            
            List<string> friendKeys = new List<string>();
            foreach (var designProperty in designClass.Properties)
            {
                DynEntity propertyEntity = new DynEntity("DynProperty");
                designProperty.PropertyID = SystemService.GetNextID("DynProperty");
                propertyEntity["DynPropertyID"] = designProperty.PropertyID;
                propertyEntity["DynClassID"] = designClass.ClassID;
                propertyEntity["DynPropertyName"] = designProperty.PropertyName;
                propertyEntity["DisplayName"] = designProperty.DisplayName;
                propertyEntity["Description"] = designProperty.Description;
                propertyEntity["IsQueryProperty"] = designProperty.IsQueryProperty;
                propertyEntity["IsArray"] = designProperty.IsArray;
                propertyEntity["CollectionType"] = designProperty.CollectionType;
                propertyEntity["Type"] = designProperty.DataType;
                propertyEntity["StructName"] = designProperty.StructName;

                //属性Attribute处理
                List<DynObject> propertyAttributes = new List<DynObject>();
                DynObject attribute = null;

                //属性数据库的处理
                if (designClass.IsPersistable && designProperty.IsPersistable)
                {
                    if (!string.IsNullOrEmpty(designProperty.DbFieldName))
                    {
                        attribute = new DynObject("MappingName");
                        attribute["Name"] = designProperty.DbFieldName;
                        propertyAttributes.Add(attribute);
                    }
                    if (designProperty.IsPrimarykey)
                    {
                        attribute = new DynObject("PrimaryKey");
                        propertyAttributes.Add(attribute);
                    }
                    if (!designProperty.IsNullable)
                    {
                        attribute = new DynObject("NotNull");
                        propertyAttributes.Add(attribute);
                    }
                    if (designProperty.RelationType == "一对多")
                    {
                        attribute = new DynObject("FkReverseQuery");
                        propertyAttributes.Add(attribute);
                        if (!designProperty.IsNullable)
                        {
                            attribute = new DynObject("FriendKey");
                            attribute["RelatedEntityType"] = designProperty.StructName;
                            propertyAttributes.Add(attribute);
                            friendKeys.Add("ALTER  TABLE  [" + designClass.ClassName + "] ADD CONSTRAINT [FK_" + designClass.ClassName + "_" + designProperty.DbFieldName + "] FOREIGN KEY ([" + designProperty.DbFieldName + "]) REFERENCES [" + designProperty.StructName + "](" + designProperty.DbFieldName + "); ");
                        }
                    }

                    if (designProperty.RelationType == "多对多")
                    {
                        attribute = new DynObject("RelationKey");
                        attribute["RelatedType"] = designProperty.StructName;
                        propertyAttributes.Add(attribute);
                        friendKeys.Add("ALTER  TABLE  [" + designClass.ClassName + "] ADD CONSTRAINT [FK_" + designClass.ClassName + "_" + designProperty.DbFieldName + "] FOREIGN KEY ([" + designProperty.DbFieldName + "]) REFERENCES [" + designProperty.StructName + "](" + designProperty.DbFieldName + "); ");
                    }
                    string sqlTrueType;
                    DynObject sqlTypeAttribute = new DynObject("SqlType");
                    Check.Require(!string.IsNullOrEmpty(designProperty.SqlType), "属性SqlType类型不能为空");
                    switch (designProperty.SqlType)
                    {
                        case "char":
                        case "nchar":
                        case "binary":
                        case "time":
                            Check.Require(designProperty.DbFieldLength > 0, "类型:" + designProperty.SqlType + "的长度必须大于0");
                            sqlTrueType = designProperty.SqlType + "(" + designProperty.DbFieldLength + ")";
                            break;
                        case "nvarchar":
                        case "varbinary":
                        case "varchar":
                            if (designProperty.DbFieldLength > 0)
                            {
                                sqlTrueType = designProperty.SqlType + "(" + designProperty.DbFieldLength + ")";
                            }
                            else
                            {
                                sqlTrueType = designProperty.SqlType + "(MAX)";
                            }
                            break;
                        case "decimal":
                            sqlTrueType = designProperty.SqlType + "(" + designProperty.DbFieldLength + "," + designProperty.DecimalDigits + ")";
                            break;
                        default:
                            sqlTrueType = designProperty.SqlType;
                            break;
                    }
                    sqlTypeAttribute["Type"] = sqlTrueType;
                    propertyAttributes.Add(sqlTypeAttribute);
                    //designProperty.SqlType = sqlTrueType;

                    columnLength++;
                    string columnName;
                    if (string.IsNullOrEmpty(designProperty.DbFieldName))
                    {
                        columnName = designProperty.PropertyName;
                    }
                    else
                    {
                        columnName = designProperty.DbFieldName;
                    }

                    commondSB.Append("[" + columnName + "]");
                    commondSB.Append(" ");
                    commondSB.Append(sqlTrueType);
                    commondSB.Append(" ");
                    if (!designProperty.IsNullable)
                    {
                        commondSB.Append("NOT NULL");
                    }
                    commondSB.Append(",");
                    if (designProperty.IsPrimarykey)
                    {
                        pkColumnName.Add(columnName);
                    }

                    //DesignInfo界面Attribute的处理
                    if (designProperty.UIDesignInfo != null)
                    {
                        attribute = new DynObject("DesignInfo");
                        attribute["GridColAlign"] = designProperty.UIDesignInfo.GridColAlign;
                        attribute["GridColSorting"] = designProperty.UIDesignInfo.GridColSorting;
                        attribute["GridColType"] = designProperty.UIDesignInfo.GridColType;
                        attribute["GridHeader"] = designProperty.UIDesignInfo.GridHeader;
                        attribute["GridWidth"] = designProperty.UIDesignInfo.GridWidth;
                        attribute["InputType"] = designProperty.UIDesignInfo.InputType;
                        attribute["IsRequired"] = designProperty.UIDesignInfo.IsRequired;
                        attribute["IsReadOnly"] = designProperty.UIDesignInfo.IsReadOnly;
                        attribute["ValidateType"] = designProperty.UIDesignInfo.ValidateType;
                        attribute["QueryForm"] = designProperty.UIDesignInfo.QueryForm;
                        attribute["ReferType"] = designProperty.UIDesignInfo.ReferType;
                        propertyAttributes.Add(attribute);
                    }
                }
                else
                {
                    if (designProperty.RelationType == "多对一")
                    {
                        attribute = new DynObject("FkQuery");
                        attribute["RelatedManyToOneQueryPropertyName"] = designClass.ClassName;
                        propertyAttributes.Add(attribute);
                    }
                    attribute = new DynObject("PersistIgnore");
                    propertyAttributes.Add(attribute);
                }

                propertyEntity["Attributes"] = DynObjectTransverter.DynObjectListToJson(propertyAttributes);
                designProperty.State = "normal";
                designProperty.IsPropertyChanged = false;
                dataTransactionContext.SavedDynEntities.Add(propertyEntity);
            }

            //添加主键
            if (designClass.IsPersistable)
            {
                if (pkColumnName.Count > 0)
                {
                    commondSB.Append("PRIMARY KEY (");
                    foreach (var pkProperty in pkColumnName)
                    {
                        commondSB.Append("[" + pkProperty + "]");
                        commondSB.Append(",");
                    }

                    if (commondSB[commondSB.Length - 1] == ',')
                    {
                        commondSB.Length = commondSB.Length - 1;
                    }
                    commondSB.Append(")");
                }

                commondSB.Append(")");
                commondSB.Append(";");

                Check.Require(columnLength > 0, "创建表失败 无法创建无列表 请检查!");

                string commandString = commondSB.ToString();
                dataTransactionContext.SqlCommandStrings.Add(commandString);

                //添加外键               
                foreach (var item in friendKeys)
                {
                    dataTransactionContext.SqlCommandStrings.Add(item);
                }
            }
            //注册到ObjType表
            if (designClass.IsPersistable)
            {
                DynEntity objTypeEntity = new DynEntity("ObjType");
                objTypeEntity["TypeID"] = SystemService.GetNextID("ObjType");
                objTypeEntity["Name"] = tableName;
                if (string.IsNullOrEmpty(designClass.DisplayName))
                {
                    objTypeEntity["CnName"] = tableName;
                }
                else
                {
                    objTypeEntity["CnName"] = designClass.DisplayName;
                }
                objTypeEntity["Description"] = designClass.Description;
                objTypeEntity["AppLevel"] = "User";
                objTypeEntity["IsDymatic"] = false;
                objTypeEntity["NextID"] = 1;
                dataTransactionContext.SavedDynEntities.Add(objTypeEntity);
            }
            //开启事务
            using (TransactionScope trans = new TransactionScope())
            {
                dataTransactionContext.Commit();
                trans.Complete();
            }
        }

        //仅修改类本身不修改其属性和方法
        public void EditDesignClass(DesignClass designClass)
        {
            Check.Require(designClass.ModuleID > 0, "类型对象的模块ID不为空");
            Check.Require(designClass.NamespaceID > 0, "类型对象的命名空间ID不为空");

            DynEntity originalClassEntity = GatewayFactory.Default.Find("DynClass", designClass.ClassID);
            Check.Require(originalClassEntity != null, "当前控制类在数据库中不存在无法修改,请检查");
            originalClassEntity["NamespaceID"] = designClass.NamespaceID;
            originalClassEntity["DisplayName"] = designClass.DisplayName;
            originalClassEntity["Description"] = designClass.Description;

            //控制类要处理对应的接口
            if (designClass.MainType == 1)
            {
                DynEntity interfaceEntity = GatewayFactory.Default.Find("DynClass", _.P("DynClass", "DynClassName") == "I" + designClass.ClassName);
                Check.Require(interfaceEntity != null, "控制类对应的接口类不能为空,请检查");
                interfaceEntity["NamespaceID"] = designClass.NamespaceID;
                interfaceEntity["DisplayName"] = designClass.DisplayName;
                interfaceEntity["Description"] = designClass.Description;
                //开启事务
                using (TransactionScope trans = new TransactionScope())
                {
                    GatewayFactory.Default.Save(originalClassEntity);
                    GatewayFactory.Default.Save(interfaceEntity);
                    trans.Complete();
                }
            }
            else
            {
                GatewayFactory.Default.Save(originalClassEntity);
            }
        }
        //仅修改类的属性和方法不修改类本身
        public void ModifyDesignClass(DesignClass designClass)
        {
            DataTransactionContext dataTransactionContext = new DataTransactionContext();
            //方法的处理
            foreach (var designMethode in designClass.Methodes)
            {
                //正常状态的方法需要判断参数是否发生改变,如果改变按照修改的方式处理
                if (designMethode.State == "normal")
                {
                    foreach (var item in designMethode.Parameters)
                    {
                        if (item.State != "normal")
                        {
                            DynEntity modifiedMethodeEntity = GatewayFactory.Default.Find("DynMethod", designMethode.MethodID);
                            modifiedMethodeEntity["DynMethodName"] = designMethode.MethodName;
                            modifiedMethodeEntity["DisplayName"] = designMethode.DisplayName;
                            modifiedMethodeEntity["Description"] = designMethode.Description;
                            modifiedMethodeEntity["ScriptType"] = designMethode.ScriptType;
                            modifiedMethodeEntity["Body"] = designMethode.Body;
                            modifiedMethodeEntity["DynClassID"] = designClass.ClassID;

                            DynObject methodDynObject = new DynObject("Method");

                            methodDynObject["MethodID"] = modifiedMethodeEntity["DynMethodID"];
                            methodDynObject["MethodName"] = designMethode.MethodName;
                            methodDynObject["DisplayName"] = designMethode.DisplayName;
                            methodDynObject["Description"] = designMethode.Description;
                            methodDynObject["ScriptType"] = designMethode.ScriptType;
                            methodDynObject["Body"] = designMethode.Body;
                            methodDynObject["ClassID"] = designClass.ClassID;

                            //方法Attribute处理
                            List<DynObject> attributes = new List<DynObject>();
                            if (designMethode.IsOperationProtocol)
                            {
                                DynObject attribute = new DynObject("OperationContract");
                                attribute["Name"] = "default";
                                attributes.Add(attribute);
                            }
                            modifiedMethodeEntity["Attributes"] = DynObjectTransverter.DynObjectListToJson(attributes);
                            methodDynObject["Attributes"] = modifiedMethodeEntity["Attributes"];
                            //方法参数处理
                            List<DynObject> parameters = new List<DynObject>();
                            for (int i = 0; i < designMethode.Parameters.Count; i++)
                            {
                                DesignMethodParameter parameter = designMethode.Parameters[i];
                                DynObject parameterDynObject = new DynObject("Parameter");
                                parameterDynObject["ParameterID"] = i;
                                parameterDynObject["ParameterName"] = parameter.ParameterName;
                                parameterDynObject["CollectionType"] = parameter.CollectionType;
                                parameterDynObject["Type"] = parameter.DataType;
                                parameterDynObject["StructName"] = parameter.StructName;
                                parameterDynObject["Description"] = parameter.Description;
                                parameters.Add(parameterDynObject);
                                parameter.State = "normal";
                                parameter.IsParameterChanged = false;
                            }
                            methodDynObject["Parameters"] = parameters;
                            //方法返回值处理
                            DynObject methodResult = new DynObject("Parameter");
                            methodResult["ParameterID"] = 0;
                            methodResult["ParameterName"] = "Result";
                            methodResult["CollectionType"] = designMethode.ResultCollectionType;
                            methodResult["Type"] = designMethode.ResultDataType;
                            methodResult["StructName"] = designMethode.ResultStructName;
                            methodDynObject["Result"] = methodResult;
                            modifiedMethodeEntity["Definition"] = DynObjectTransverter.DynObjectToJson(methodDynObject);
                            designMethode.State = "normal";
                            designMethode.IsMethodChanged = false;
                            dataTransactionContext.SavedDynEntities.Add(modifiedMethodeEntity);

                            //处理接口方法
                            if (designMethode.IsOperationProtocol)
                            {
                                DynEntity interfaceMethodeEntity = GatewayFactory.Default.Find("DynMethod", _.P("DynMethod", "SourceMethodID") == designMethode.MethodID);
                                Check.Require(interfaceMethodeEntity != null, "方法[" + designMethode.MethodName + "]所对应的接口方法不存在,请检查!");
                                interfaceMethodeEntity["DynMethodName"] = designMethode.MethodName;
                                interfaceMethodeEntity["DisplayName"] = designMethode.DisplayName;
                                interfaceMethodeEntity["Description"] = designMethode.Description;
                                interfaceMethodeEntity["ScriptType"] = designMethode.ScriptType;
                                interfaceMethodeEntity["Body"] = designMethode.Body;
                                interfaceMethodeEntity["Definition"] = modifiedMethodeEntity["Definition"];
                                dataTransactionContext.SavedDynEntities.Add(interfaceMethodeEntity);
                            }
                            break;
                        }
                    }
                }
                //新增方法的处理
                if (designMethode.State == "added")
                {
                    DynEntity addedMethodEntity = new DynEntity("DynMethod");
                    designMethode.MethodID = SystemService.GetNextID("DynMethod");
                    addedMethodEntity["DynMethodID"] = designMethode.MethodID;
                    addedMethodEntity["DynMethodName"] = designMethode.MethodName;
                    addedMethodEntity["DisplayName"] = designMethode.DisplayName;
                    addedMethodEntity["Description"] = designMethode.Description;
                    addedMethodEntity["ScriptType"] = designMethode.ScriptType;
                    addedMethodEntity["Body"] = designMethode.Body;
                    addedMethodEntity["DynClassID"] = designClass.ClassID;

                    DynObject methodDynObject = new DynObject("Method");

                    methodDynObject["MethodID"] = addedMethodEntity["DynMethodID"];
                    methodDynObject["MethodName"] = designMethode.MethodName;
                    methodDynObject["DisplayName"] = designMethode.DisplayName;
                    methodDynObject["Description"] = designMethode.Description;
                    methodDynObject["ScriptType"] = designMethode.ScriptType;
                    methodDynObject["Body"] = designMethode.Body;
                    methodDynObject["ClassID"] = designClass.ClassID;

                    //方法Attribute处理
                    List<DynObject> attributes = new List<DynObject>();
                    if (designMethode.IsOperationProtocol)
                    {
                        DynObject attribute = new DynObject("OperationContract");
                        attribute["Name"] = "default";
                        attributes.Add(attribute);
                    }
                    addedMethodEntity["Attributes"] = DynObjectTransverter.DynObjectListToJson(attributes);
                    methodDynObject["Attributes"] = addedMethodEntity["Attributes"];
                    //方法参数处理
                    List<DynObject> parameters = new List<DynObject>();
                    for (int i = 0; i < designMethode.Parameters.Count; i++)
                    {
                        DesignMethodParameter parameter = designMethode.Parameters[i];
                        DynObject parameterDynObject = new DynObject("Parameter");
                        parameterDynObject["ParameterID"] = i;
                        parameterDynObject["ParameterName"] = parameter.ParameterName;
                        parameterDynObject["CollectionType"] = parameter.CollectionType;
                        parameterDynObject["Type"] = parameter.DataType;
                        parameterDynObject["StructName"] = parameter.StructName;
                        parameterDynObject["Description"] = parameter.Description;
                        parameters.Add(parameterDynObject);
                        parameter.State = "normal";
                        parameter.IsParameterChanged = false;
                    }
                    methodDynObject["Parameters"] = parameters;
                    //方法返回值处理
                    DynObject methodResult = new DynObject("Parameter");
                    methodResult["ParameterID"] = 0;
                    methodResult["ParameterName"] = "Result";
                    methodResult["CollectionType"] = designMethode.ResultCollectionType;
                    methodResult["Type"] = designMethode.ResultDataType;
                    methodResult["StructName"] = designMethode.ResultStructName;
                    methodDynObject["Result"] = methodResult;
                    addedMethodEntity["Definition"] = DynObjectTransverter.DynObjectToJson(methodDynObject);
                    designMethode.State = "normal";
                    designMethode.IsMethodChanged = false;
                    dataTransactionContext.SavedDynEntities.Add(addedMethodEntity);
                    //接口方法
                    if (designMethode.IsOperationProtocol)
                    {
                        //获取接口类型的classID
                        string sqlstr = "select DynClassID from DynClass where DynClassName = 'I" + designClass.ClassName + "'";
                        string interfaceClassID = SystemService.ExecuteScalar(sqlstr);
                        Check.Require(interfaceClassID != "", "方法对应的接口类型不存在,请检查!");

                        DynEntity interfaceMethodEntity = new DynEntity("DynMethod");
                        interfaceMethodEntity["DynMethodID"] = SystemService.GetNextID("DynMethod");
                        interfaceMethodEntity["DynMethodName"] = designMethode.MethodName;
                        interfaceMethodEntity["DisplayName"] = designMethode.DisplayName;
                        interfaceMethodEntity["Description"] = designMethode.Description;
                        interfaceMethodEntity["DynClassID"] = interfaceClassID;
                        interfaceMethodEntity["Body"] = designMethode.Body;
                        interfaceMethodEntity["ScriptType"] = designMethode.ScriptType;
                        //这个属性用作存放实体方法的ID,以建立对应关系,便于维护
                        interfaceMethodEntity["SourceMethodID"] = designMethode.MethodID;

                        DynObject interfaceMethodDynObject = new DynObject("Method");

                        interfaceMethodDynObject["MethodID"] = interfaceMethodEntity["DynMethodID"];
                        interfaceMethodDynObject["MethodName"] = designMethode.MethodName;
                        interfaceMethodDynObject["DisplayName"] = designMethode.DisplayName;
                        interfaceMethodDynObject["Description"] = designMethode.Description;
                        interfaceMethodDynObject["ScriptType"] = designMethode.ScriptType;
                        interfaceMethodDynObject["Body"] = designMethode.Body;
                        interfaceMethodDynObject["ClassID"] = Convert.ToInt32(interfaceClassID);
                        interfaceMethodDynObject["SourceMethodID"] = designMethode.MethodID;
                        interfaceMethodDynObject["Parameters"] = parameters;
                        interfaceMethodDynObject["Result"] = methodResult;
                        interfaceMethodEntity["Definition"] = DynObjectTransverter.DynObjectToJson(interfaceMethodDynObject);
                        dataTransactionContext.SavedDynEntities.Add(interfaceMethodEntity);
                    }

                }
                //修改方法的处理
                if (designMethode.State == "modified")
                {
                    DynEntity modifiedMethodeEntity = GatewayFactory.Default.Find("DynMethod", designMethode.MethodID);
                    modifiedMethodeEntity["DynMethodName"] = designMethode.MethodName;
                    modifiedMethodeEntity["DisplayName"] = designMethode.DisplayName;
                    modifiedMethodeEntity["Description"] = designMethode.Description;
                    modifiedMethodeEntity["ScriptType"] = designMethode.ScriptType;
                    modifiedMethodeEntity["Body"] = designMethode.Body;
                    modifiedMethodeEntity["DynClassID"] = designClass.ClassID;

                    DynObject methodDynObject = new DynObject("Method");

                    methodDynObject["MethodID"] = modifiedMethodeEntity["DynMethodID"];
                    methodDynObject["MethodName"] = designMethode.MethodName;
                    methodDynObject["DisplayName"] = designMethode.DisplayName;
                    methodDynObject["Description"] = designMethode.Description;
                    methodDynObject["ScriptType"] = designMethode.ScriptType;
                    methodDynObject["Body"] = designMethode.Body;
                    methodDynObject["ClassID"] = designClass.ClassID;

                    //方法Attribute处理
                    List<DynObject> attributes = new List<DynObject>();
                    if (designMethode.IsOperationProtocol)
                    {
                        DynObject attribute = new DynObject("OperationContract");
                        attribute["Name"] = "default";
                        attributes.Add(attribute);
                    }
                    modifiedMethodeEntity["Attributes"] = DynObjectTransverter.DynObjectListToJson(attributes);
                    methodDynObject["Attributes"] = modifiedMethodeEntity["Attributes"];
                    //方法参数处理
                    List<DynObject> parameters = new List<DynObject>();
                    for (int i = 0; i < designMethode.Parameters.Count; i++)
                    {
                        DesignMethodParameter parameter = designMethode.Parameters[i];
                        DynObject parameterDynObject = new DynObject("Parameter");
                        parameterDynObject["ParameterID"] = i;
                        parameterDynObject["ParameterName"] = parameter.ParameterName;
                        parameterDynObject["CollectionType"] = parameter.CollectionType;
                        parameterDynObject["Type"] = parameter.DataType;
                        parameterDynObject["StructName"] = parameter.StructName;
                        parameterDynObject["Description"] = parameter.Description;
                        parameters.Add(parameterDynObject);
                        parameter.State = "normal";
                        parameter.IsParameterChanged = false;
                    }
                    methodDynObject["Parameters"] = parameters;
                    //方法返回值处理
                    DynObject methodResult = new DynObject("Parameter");
                    methodResult["ParameterID"] = 0;
                    methodResult["ParameterName"] = "Result";
                    methodResult["CollectionType"] = designMethode.ResultCollectionType;
                    methodResult["Type"] = designMethode.ResultDataType;
                    methodResult["StructName"] = designMethode.ResultStructName;
                    methodDynObject["Result"] = methodResult;
                    modifiedMethodeEntity["Definition"] = DynObjectTransverter.DynObjectToJson(methodDynObject);
                    designMethode.State = "normal";
                    designMethode.IsMethodChanged = false;
                    dataTransactionContext.SavedDynEntities.Add(modifiedMethodeEntity);
                    //处理接口方法
                    if (designMethode.IsOperationProtocol)
                    {
                        DynEntity interfaceMethodeEntity = GatewayFactory.Default.Find("DynMethod", _.P("DynMethod", "SourceMethodID") == designMethode.MethodID);
                        Check.Require(interfaceMethodeEntity != null, "方法[" + designMethode.MethodName + "]所对应的接口方法不存在,请检查!");
                        interfaceMethodeEntity["DynMethodName"] = designMethode.MethodName;
                        interfaceMethodeEntity["DisplayName"] = designMethode.DisplayName;
                        interfaceMethodeEntity["Description"] = designMethode.Description;
                        interfaceMethodeEntity["ScriptType"] = designMethode.ScriptType;
                        interfaceMethodeEntity["Body"] = designMethode.Body;
                        interfaceMethodeEntity["Definition"] = modifiedMethodeEntity["Definition"];
                        dataTransactionContext.SavedDynEntities.Add(interfaceMethodeEntity);
                    }
                }
                //删除方法的处理
                foreach (var deletedMethode in designClass.DeletedMethodes)
                {
                    dataTransactionContext.SqlCommandStrings.Add("delete from DynMethod where DynMethodID = " + deletedMethode.MethodID);
                    //删除对应的接口方法
                    dataTransactionContext.SqlCommandStrings.Add("delete from DynMethod where SourceMethodID = " + deletedMethode.MethodID);
                }
                designClass.DeletedMethodes.Clear();
            }

            //删除的属性处理
            foreach (var designProperty in designClass.DeletedProperties)
            {
                //删除属性实体
                dataTransactionContext.DeletedDynPropertyEntities.Add(designProperty.PropertyID, "DynProperty");
                //删除数据库相关
                if (designClass.IsPersistable && designProperty.IsPersistable)
                {
                    if (designProperty.RelationType == "无")
                    {
                        dataTransactionContext.SqlCommandStrings.Add(string.Format("alter table [{0}] drop column [{1}];", designClass.ClassName, designProperty.PropertyName));
                    }
                    else
                    {
                        //判断是否存在外键
                        string strSql = " select COUNT(*) from  sys.foreign_key_columns f join sys.objects o on f.constraint_object_id=o.object_id  where f.parent_object_id=object_id('" + designClass.ClassName + "') and name = 'FK_" + designClass.ClassName + "_" + designProperty.DbFieldName + "'";
                        if (Convert.ToInt32(SystemService.ExecuteScalar(strSql)) > 0)
                        {
                            dataTransactionContext.SqlCommandStrings.Add("Alter table [" + designClass.ClassName + "] drop constraint [FK_" + designClass.ClassName + "_" + designProperty.DbFieldName + "] ;");
                        }
                        dataTransactionContext.SqlCommandStrings.Add(string.Format("alter table [{0}] drop column [{1}];", designClass.ClassName, designProperty.DbFieldName));
                    }
                }
            }
            designClass.DeletedProperties.Clear();
            //新增修改的属性处理   
            List<string> friendKeys = new List<string>();
            foreach (var designProperty in designClass.Properties)
            {
                //新增列的处理      
                if (designProperty.State == "added")
                {
                    DynEntity propertyEntity = new DynEntity("DynProperty");
                    designProperty.PropertyID = SystemService.GetNextID("DynProperty");
                    propertyEntity["DynPropertyID"] = designProperty.PropertyID;
                    propertyEntity["DynClassID"] = designClass.ClassID;
                    propertyEntity["DynPropertyName"] = designProperty.PropertyName;
                    propertyEntity["DisplayName"] = designProperty.DisplayName;
                    propertyEntity["Description"] = designProperty.Description;
                    propertyEntity["IsQueryProperty"] = designProperty.IsQueryProperty;
                    propertyEntity["IsArray"] = designProperty.IsArray;
                    propertyEntity["CollectionType"] = designProperty.CollectionType;
                    propertyEntity["Type"] = designProperty.DataType;
                    propertyEntity["StructName"] = designProperty.StructName;

                    //属性Attribute处理
                    string friendKeyString = string.Empty;
                    List<DynObject> propertyAttributes = new List<DynObject>();
                    DynObject attribute = null;
                    if (designClass.IsPersistable && designProperty.IsPersistable)
                    {
                        if (!string.IsNullOrEmpty(designProperty.DbFieldName))
                        {
                            attribute = new DynObject("MappingName");
                            attribute["Name"] = designProperty.DbFieldName;
                            propertyAttributes.Add(attribute);
                        }

                        if (!designProperty.IsNullable)
                        {
                            attribute = new DynObject("NotNull");
                            propertyAttributes.Add(attribute);
                        }
                        if (designProperty.RelationType == "一对多")
                        {
                            attribute = new DynObject("FkReverseQuery");
                            propertyAttributes.Add(attribute);

                            if (!designProperty.IsNullable)
                            {
                                attribute = new DynObject("FriendKey");
                                attribute["RelatedEntityType"] = designProperty.StructName;
                                propertyAttributes.Add(attribute);
                                friendKeyString = "ALTER  TABLE  [" + designClass.ClassName + "] ADD CONSTRAINT [FK_" + designClass.ClassName + "_" + designProperty.DbFieldName + "] FOREIGN KEY ([" + designProperty.DbFieldName + "]) REFERENCES [" + designProperty.StructName + "](" + designProperty.DbFieldName + "); ";
                                friendKeys.Add(friendKeyString);
                            }
                        }

                        string sqlType;
                        DynObject sqlTypeAttribute = new DynObject("SqlType");
                        Check.Require(!string.IsNullOrEmpty(designProperty.SqlType), "属性SqlType类型不能为空");
                        switch (designProperty.SqlType)
                        {
                            case "char":
                            case "nchar":
                            case "binary":
                            case "time":
                                Check.Require(designProperty.DbFieldLength > 0, "类型:" + designProperty.SqlType + "的长度必须大于0");
                                sqlType = designProperty.SqlType + "(" + designProperty.DbFieldLength + ")";
                                break;
                            case "nvarchar":
                            case "varbinary":
                            case "varchar":
                                if (designProperty.DbFieldLength > 0)
                                {
                                    sqlType = designProperty.SqlType + "(" + designProperty.DbFieldLength + ")";
                                }
                                else
                                {
                                    sqlType = designProperty.SqlType + "(MAX)";
                                }
                                break;
                            case "decimal":
                                sqlType = designProperty.SqlType + "(" + designProperty.DbFieldLength + "," + designProperty.DecimalDigits + ")";
                                break;
                            default:
                                sqlType = designProperty.SqlType;
                                break;
                        }
                        sqlTypeAttribute["Type"] = sqlType;
                        propertyAttributes.Add(sqlTypeAttribute);
                        //designProperty.SqlType = sqlType;

                        //数据库新增列的处理
                        if (designProperty.IsPersistable)
                        {
                            string columnName;
                            if (string.IsNullOrEmpty(designProperty.DbFieldName))
                            {
                                columnName = designProperty.PropertyName;
                            }
                            else
                            {
                                columnName = designProperty.DbFieldName;
                            }
                            string commandString = string.Format("alter table [{0}] add [{1}] {2} {3};", designClass.ClassName, columnName, sqlType, (!designProperty.IsNullable ? "Not null" : ""));
                            dataTransactionContext.SqlCommandStrings.Add(commandString);
                        }

                        //DesignInfo界面Attribute的处理
                        if (designProperty.UIDesignInfo != null)
                        {
                            attribute = new DynObject("DesignInfo");
                            attribute["GridColAlign"] = designProperty.UIDesignInfo.GridColAlign;
                            attribute["GridColSorting"] = designProperty.UIDesignInfo.GridColSorting;
                            attribute["GridColType"] = designProperty.UIDesignInfo.GridColType;
                            attribute["GridHeader"] = designProperty.UIDesignInfo.GridHeader;
                            attribute["GridWidth"] = designProperty.UIDesignInfo.GridWidth;
                            attribute["InputType"] = designProperty.UIDesignInfo.InputType;
                            attribute["IsRequired"] = designProperty.UIDesignInfo.IsRequired;
                            attribute["IsReadOnly"] = designProperty.UIDesignInfo.IsReadOnly;
                            attribute["QueryForm"] = designProperty.UIDesignInfo.QueryForm;
                            attribute["ReferType"] = designProperty.UIDesignInfo.ReferType;
                            attribute["ValidateType"] = designProperty.UIDesignInfo.ValidateType;
                            propertyAttributes.Add(attribute);
                        }
                    }
                    else
                    {
                        if (designProperty.RelationType == "多对一")
                        {
                            attribute = new DynObject("FkQuery");
                            attribute["RelatedManyToOneQueryPropertyName"] = designClass.ClassName;
                            propertyAttributes.Add(attribute);
                        }
                        attribute = new DynObject("PersistIgnore");
                        propertyAttributes.Add(attribute);
                    }
                    designProperty.State = "normal";
                    designProperty.IsPropertyChanged = false;
                    propertyEntity["Attributes"] = DynObjectTransverter.DynObjectListToJson(propertyAttributes);
                    dataTransactionContext.SavedDynEntities.Add(propertyEntity);

                }
                //修改列的处理不包含原本就是一对多关系的列
                if (designProperty.State == "modified")
                {
                    DynEntity propertyEntity = GatewayFactory.Default.Find("DynProperty", designProperty.PropertyID);
                    bool propertyNameChanged = false;
                    string originalPropertyName = propertyEntity["DynPropertyName"] as string;
                    propertyNameChanged = (originalPropertyName != designProperty.PropertyName);
                    //属性Attribute处理
                    string friendKeyString = string.Empty;
                    List<DynObject> propertyAttributes = new List<DynObject>();
                    DynObject attribute = null;

                    if (!string.IsNullOrEmpty(designProperty.DbFieldName))
                    {
                        attribute = new DynObject("MappingName");
                        attribute["Name"] = designProperty.DbFieldName;
                        propertyAttributes.Add(attribute);
                    }

                    if (!designProperty.IsNullable)
                    {
                        attribute = new DynObject("NotNull");
                        propertyAttributes.Add(attribute);
                    }
                    if (!designProperty.IsPersistable)
                    {
                        attribute = new DynObject("PersistIgnore");
                        propertyAttributes.Add(attribute);
                    }
                    //说明是由一般类型的属性改成了一对多的关系
                    if (designProperty.RelationType == "一对多")
                    {
                        attribute = new DynObject("FkReverseQuery");
                        propertyAttributes.Add(attribute);

                        if (!designProperty.IsNullable)
                        {
                            attribute = new DynObject("FriendKey");
                            attribute["RelatedEntityType"] = designProperty.StructName;
                            propertyAttributes.Add(attribute);
                            friendKeyString = "ALTER  TABLE  [" + designClass.ClassName + "] ADD CONSTRAINT FK_" + designClass.ClassName + "_" + designProperty.DbFieldName + " FOREIGN KEY ([" + designProperty.DbFieldName + "]) REFERENCES [" + designProperty.StructName + "](" + designProperty.DbFieldName + "); ";
                            friendKeys.Add(friendKeyString);
                        }

                        string sqlType;
                        DynObject sqlTypeAttribute = new DynObject("SqlType");
                        Check.Require(!string.IsNullOrEmpty(designProperty.SqlType), "属性SqlType类型不能为空");
                        switch (designProperty.SqlType)
                        {
                            case "char":
                            case "nchar":
                            case "binary":
                            case "time":
                                Check.Require(designProperty.DbFieldLength > 0, "类型:" + designProperty.SqlType + "的长度必须大于0");
                                sqlType = designProperty.SqlType + "(" + designProperty.DbFieldLength + ")";
                                break;
                            case "nvarchar":
                            case "varbinary":
                            case "varchar":
                                if (designProperty.DbFieldLength > 0)
                                {
                                    sqlType = designProperty.SqlType + "(" + designProperty.DbFieldLength + ")";
                                }
                                else
                                {
                                    sqlType = designProperty.SqlType + "(MAX)";
                                }
                                break;
                            case "decimal":
                                sqlType = designProperty.SqlType + "(" + designProperty.DbFieldLength + "," + designProperty.DecimalDigits + ")";
                                break;
                            default:
                                sqlType = designProperty.SqlType;
                                break;
                        }
                        sqlTypeAttribute["Type"] = sqlType;
                        propertyAttributes.Add(sqlTypeAttribute);
                        //designProperty.SqlType = sqlType;

                        //数据库修改列的处理
                        string columnName;
                        if (string.IsNullOrEmpty(designProperty.DbFieldName))
                        {
                            columnName = designProperty.PropertyName;
                        }
                        else
                        {
                            columnName = designProperty.DbFieldName;
                        }
                        //先修改数据类型再判断是否修改列名
                        dataTransactionContext.SqlCommandStrings.Add(string.Format("alter table [{0}] alter column [{1}] {2} {3};", designClass.ClassName, originalPropertyName, sqlType, (!designProperty.IsNullable ? "Not null" : "")));
                        //判断是否需要修改列名
                        if (propertyNameChanged)
                        {
                            propertyEntity["DynPropertyName"] = designProperty.PropertyName;
                            dataTransactionContext.SqlCommandStrings.Add(string.Format("EXEC  sp_rename   '{0}.{1}' , '{2}'  ;", designClass.ClassName, originalPropertyName, columnName));
                        }
                    }
                    else
                    {
                        //数据库修改列的处理
                        if (designProperty.IsPersistable)
                        {
                            string sqlType;
                            DynObject sqlTypeAttribute = new DynObject("SqlType");
                            Check.Require(!string.IsNullOrEmpty(designProperty.SqlType), "属性SqlType类型不能为空");
                            switch (designProperty.SqlType)
                            {
                                case "char":
                                case "nchar":
                                case "binary":
                                case "time":
                                    Check.Require(designProperty.DbFieldLength > 0, "类型:" + designProperty.SqlType + "的长度必须大于0");
                                    sqlType = designProperty.SqlType + "(" + designProperty.DbFieldLength + ")";
                                    break;
                                case "nvarchar":
                                case "varbinary":
                                case "varchar":
                                    if (designProperty.DbFieldLength > 0)
                                    {
                                        sqlType = designProperty.SqlType + "(" + designProperty.DbFieldLength + ")";
                                    }
                                    else
                                    {
                                        sqlType = designProperty.SqlType + "(MAX)";
                                    }
                                    break;
                                case "decimal":
                                    sqlType = designProperty.SqlType + "(" + designProperty.DbFieldLength + "," + designProperty.DecimalDigits + ")";
                                    break;
                                default:
                                    sqlType = designProperty.SqlType;
                                    break;
                            }
                            sqlTypeAttribute["Type"] = sqlType;
                            propertyAttributes.Add(sqlTypeAttribute);
                            //designProperty.SqlType = sqlType;

                            string columnName;
                            if (string.IsNullOrEmpty(designProperty.DbFieldName))
                            {
                                columnName = designProperty.PropertyName;
                            }
                            else
                            {
                                columnName = designProperty.DbFieldName;
                            }
                            //先修改数据类型再判断是否修改列名
                            dataTransactionContext.SqlCommandStrings.Add(string.Format("alter table [{0}] alter column [{1}] {2} {3};", designClass.ClassName, originalPropertyName, sqlType, (!designProperty.IsNullable ? "Not null" : "")));
                            //判断是否需要修改列名
                            if (propertyNameChanged)
                            {
                                propertyEntity["DynPropertyName"] = designProperty.PropertyName;
                                dataTransactionContext.SqlCommandStrings.Add(string.Format("EXEC  sp_rename   '{0}.{1}' , '{2}'  ;", designClass.ClassName, originalPropertyName, columnName));
                            }

                            //DesignInfo界面Attribute的处理
                            if (designProperty.UIDesignInfo != null)
                            {
                                attribute = new DynObject("DesignInfo");
                                attribute["GridColAlign"] = designProperty.UIDesignInfo.GridColAlign;
                                attribute["GridColSorting"] = designProperty.UIDesignInfo.GridColSorting;
                                attribute["GridColType"] = designProperty.UIDesignInfo.GridColType;
                                attribute["GridHeader"] = designProperty.UIDesignInfo.GridHeader;
                                attribute["GridWidth"] = designProperty.UIDesignInfo.GridWidth;
                                attribute["InputType"] = designProperty.UIDesignInfo.InputType;
                                attribute["IsRequired"] = designProperty.UIDesignInfo.IsRequired;
                                attribute["IsReadOnly"] = designProperty.UIDesignInfo.IsReadOnly;
                                attribute["QueryForm"] = designProperty.UIDesignInfo.QueryForm;
                                attribute["ReferType"] = designProperty.UIDesignInfo.ReferType;
                                attribute["ValidateType"] = designProperty.UIDesignInfo.ValidateType;
                                propertyAttributes.Add(attribute);
                            }
                        }
                        else
                        {
                            if (propertyNameChanged)
                            {
                                propertyEntity["DynPropertyName"] = designProperty.PropertyName;
                            }
                        }
                    }

                    propertyEntity["Attributes"] = DynObjectTransverter.DynObjectListToJson(propertyAttributes);
                    propertyEntity["StructName"] = designProperty.StructName;
                    propertyEntity["DisplayName"] = designProperty.DisplayName;
                    propertyEntity["Description"] = designProperty.Description;
                    propertyEntity["Type"] = designProperty.DataType;
                    propertyEntity["CollectionType"] = designProperty.CollectionType;

                    dataTransactionContext.SavedDynEntities.Add(propertyEntity);
                }
                designProperty.State = "normal";
                designProperty.IsPropertyChanged = false;

            }
            //添加外键
            foreach (var item in friendKeys)
            {
                dataTransactionContext.SqlCommandStrings.Add(item);
            }
            //开启事务
            using (TransactionScope trans = new TransactionScope())
            {
                dataTransactionContext.Commit();
                trans.Complete();
            }
        }
        public void ModifyUIDesignInfo(int propertyID, DesignInfo designInfo)
        {
            DynEntity propertyEntity = GatewayFactory.Default.Find("DynProperty", propertyID);
            string dynPropertyAtrributeStr = propertyEntity["Attributes"] as string;
            List<DynObject> dynPropertyAttributes = DynObjectTransverter.JsonToDynObjectList(dynPropertyAtrributeStr);
            bool hasDesignInfo = false;
            foreach (var attribute in dynPropertyAttributes)
            {
                switch (attribute.DynClass.Name)
                {
                    case "DesignInfo":
                        attribute["GridColAlign"] = designInfo.GridColAlign;
                        attribute["GridColSorting"] = designInfo.GridColSorting;
                        attribute["GridColType"] = designInfo.GridColType;
                        attribute["GridHeader"] = designInfo.GridHeader;
                        attribute["GridWidth"] = designInfo.GridWidth;
                        attribute["InputType"] = designInfo.InputType;
                        attribute["IsRequired"] = designInfo.IsRequired;
                        attribute["IsReadOnly"] = designInfo.IsReadOnly;
                        attribute["QueryForm"] = designInfo.QueryForm;
                        attribute["ReferType"] = designInfo.ReferType;
                        attribute["ValidateType"] = designInfo.ValidateType;
                        hasDesignInfo = true;
                        break;
                }
            }
            if (!hasDesignInfo)
            {
                DynObject attribute = new DynObject("DesignInfo");
                attribute["GridColAlign"] = designInfo.GridColAlign;
                attribute["GridColSorting"] = designInfo.GridColSorting;
                attribute["GridColType"] = designInfo.GridColType;
                attribute["GridHeader"] = designInfo.GridHeader;
                attribute["GridWidth"] = designInfo.GridWidth;
                attribute["InputType"] = designInfo.InputType;
                attribute["IsRequired"] = designInfo.IsRequired;
                attribute["IsReadOnly"] = designInfo.IsReadOnly;
                attribute["QueryForm"] = designInfo.QueryForm;
                attribute["ReferType"] = designInfo.ReferType;
                attribute["ValidateType"] = designInfo.ValidateType;
                dynPropertyAttributes.Add(attribute);
            }
            propertyEntity["Attributes"] = DynObjectTransverter.DynObjectListToJson(dynPropertyAttributes);
            GatewayFactory.Default.Save(propertyEntity);
        }
        public DesignInfo GetUIDesignInfo(int propertyID)
        {
            DynEntity propertyEntity = GatewayFactory.Default.Find("DynProperty", propertyID);
            string dynPropertyAtrributeStr = propertyEntity["Attributes"] as string;
            List<DynObject> dynPropertyAttributes = DynObjectTransverter.JsonToDynObjectList(dynPropertyAtrributeStr);
            DesignInfo designInfo = null;
            foreach (var attribute in dynPropertyAttributes)
            {
                switch (attribute.DynClass.Name)
                {
                    case "DesignInfo":
                        designInfo = new DesignInfo();
                        designInfo.GridColAlign = attribute["GridColAlign"].ToString();
                        designInfo.GridColSorting = attribute["GridColSorting"].ToString();
                        designInfo.GridColType = attribute["GridColType"].ToString();
                        designInfo.GridHeader = attribute["GridHeader"].ToString();
                        designInfo.GridWidth = (int)attribute["GridWidth"];
                        designInfo.InputType = attribute["InputType"].ToString();
                        designInfo.IsRequired = (bool)attribute["IsRequired"];
                        designInfo.IsReadOnly = (bool)attribute["IsReadOnly"];
                        designInfo.QueryForm = attribute["QueryForm"] as string;
                        designInfo.ReferType = attribute["ReferType"] as string;
                        designInfo.ValidateType = attribute["ValidateType"].ToString();
                        designInfo.State = "normal";
                        designInfo.IsPropertyChanged = false;
                        break;
                }
            }
            return designInfo;
        }
        public bool CanDeleteClass(string className)
        {
            string strSql = "select COUNT(*) from DynProperty where StructName = '" + className + "'";
            return Convert.ToInt32(SystemService.ExecuteScalar(strSql)) == 0;
        }
        public void DeleteClass(DesignClass designClass)
        {
            DataTransactionContext dataTransactionContext = new DataTransactionContext();
            //删除属性实体
            dataTransactionContext.SqlCommandStrings.Add("delete from DynProperty where DynClassID = " + designClass.ClassID);
            //删除类型实体
            dataTransactionContext.SqlCommandStrings.Add("delete from DynClass where DynClassID = " + designClass.ClassID);
            //删除方法实体
            if (designClass.IsServiceProtocol)
            {
                //获取接口类型的classID
                string sqlstr = "select DynClassID from DynClass where DynClassName = 'I" + designClass.ClassName + "'";
                string interfaceClassID = SystemService.ExecuteScalar(sqlstr);
                Check.Require(interfaceClassID != "", "方法对应的接口类型不存在,请检查!");
                dataTransactionContext.SqlCommandStrings.Add("delete from DynClass where DynClassID = " + interfaceClassID);
                dataTransactionContext.SqlCommandStrings.Add("delete from DynMethod where DynClassID = " + designClass.ClassID);
                dataTransactionContext.SqlCommandStrings.Add("delete from DynMethod where DynClassID = " + interfaceClassID);
            }
            //删除ObjType
            string strSql = "select TypeID from ObjType where Name = '" + designClass.ClassName + "'";
            string typeID = SystemService.ExecuteScalar(strSql);
            if (!string.IsNullOrEmpty(typeID))
            {
                dataTransactionContext.DeletedObjTypes.Add(Convert.ToInt32(typeID), "ObjType");
            }
            //删除数据库表
            bool isTableExist = GatewayFactory.Default.Db.ExecuteDataSet(CommandType.Text, string.Format("select * from sysobjects where id = object_id('{0}');", designClass.ClassName)).Tables[0].Rows.Count > 0;
            if (isTableExist)
            {
                dataTransactionContext.SqlCommandStrings.Add("DROP TABLE [" + designClass.ClassName + "];");
            }

            //开启事务
            using (TransactionScope trans = new TransactionScope())
            {
                dataTransactionContext.Commit();
                trans.Complete();
            }
        }
        #endregion 应用程序设计
    }
}
