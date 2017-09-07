using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Orm.Common;
using Rock.Dyn.Core;
using Rock.Orm.Data;
using Rock.Common;
using System.IO;
using Rock.Templating;
namespace Rock.UIDesign
{
    public class UIDesignService
    {
        #region 字典模型维护
        public void AddDictForm(DynObject dictForm)
        {
            Check.Require(dictForm != null, "DictForm对象不允许为空!");
            DynEntity dictFormEntity = new DynEntity("DictForm");

            dictFormEntity["DictFormID"] = dictForm["DictFormID"];
            dictFormEntity["DictFormName"] = dictForm["DictFormName"];
            dictFormEntity["ModelType"] = dictForm["ModelType"];
            dictFormEntity["ModelTypeName"] = dictForm["ModelTypeName"];
            dictFormEntity["ReferTypes"] = dictForm["ReferTypes"];
            dictFormEntity["ColumnCount"] = dictForm["ColumnCount"];
            dictFormEntity["Comment"] = dictForm["Comment"];
            dictFormEntity["Model"] = DynObjectTransverter.DynObjectToJson(dictForm);
            GatewayFactory.Default.Save(dictFormEntity);
        }
        public DynObject GetDictFormByID(int dictFormID)
        {
            DynObject result = null;
            DynEntity dictForm = GatewayFactory.Default.Find("DictForm", dictFormID);
            if (dictForm != null)
            {
                result = DynObjectTransverter.JsonToDynObject(dictForm["Model"] as string);
            }
            return result;
        }

        public void ModifyDictForm(DynObject dictForm)
        {
            Check.Require(dictForm != null, "DictForm对象不允许为空!");
            DynEntity dictFormEntity = GatewayFactory.Default.Find("DictForm", dictForm["DictFormID"]);
            Check.Require(dictFormEntity != null, "DictForm在数据库中不存在无法修改!");

            dictFormEntity["DictFormName"] = dictForm["DictFormName"];
            dictFormEntity["ModelType"] = dictForm["ModelType"];
            dictFormEntity["ModelTypeName"] = dictForm["ModelTypeName"];
            dictFormEntity["ReferTypes"] = dictForm["ReferTypes"];
            dictFormEntity["ColumnCount"] = dictForm["ColumnCount"];
            dictFormEntity["Comment"] = dictForm["Comment"];
            dictFormEntity["Model"] = DynObjectTransverter.DynObjectToJson(dictForm);
            //dictFormEntity["Script"] = GenerateDictFormJsCode(dictForm);
            GatewayFactory.Default.Save(dictFormEntity);
        }

        private string GenerateDictFormJsCode(DynObject dictForm, string templateName)
        {
            List<Rock.Dyn.Core.DynObject> formItems = dictForm["FormItems"] as List<Rock.Dyn.Core.DynObject>;
            Rock.Dyn.Core.DynObject dataGrid = dictForm["DataGrid"] as Rock.Dyn.Core.DynObject;
            List<Rock.Dyn.Core.DynObject> gridColumns = dataGrid["GridColumns"] as List<Rock.Dyn.Core.DynObject>;
            if (formItems.Count == 0 )
            {
                return "对象模型未添加表单项";
            }
            if (gridColumns.Count == 0 )
            {
                return "对象模型未添加列表项";
            }
            Dictionary<string, object> item = new Dictionary<string, object>();
            Template dictTemplate = Template.Create("DictTemplatet", File.ReadAllText(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "bin\\Templatet\\" + templateName + ".txt", Encoding.UTF8));
            item.Add("dictForm", dictForm);
            return dictTemplate.Render("DictTemplatet", item);
        }

        #endregion 字典模型维护

        #region 单据列表模型维护
        public void AddBillListForm(DynObject billListForm)
        {
            Check.Require(billListForm != null, "BillListForm对象不允许为空!");
            DynEntity billListFormEntity = new DynEntity("BillListForm");

            billListFormEntity["BillListFormID"] = billListForm["BillListFormID"];
            billListFormEntity["BillListFormName"] = billListForm["BillListFormName"];
            billListFormEntity["MasterType"] = billListForm["MasterType"];
            billListFormEntity["DetailType"] = billListForm["DetailType"];
            billListFormEntity["ReferTypes"] = billListForm["ReferTypes"];
            billListFormEntity["BillName"] = billListForm["BillName"];
            billListFormEntity["DetailMainReferType"] = billListForm["DetailMainReferType"];
            billListFormEntity["DetailMainReferName"] = billListForm["DetailMainReferName"];
            billListFormEntity["Comment"] = billListForm["Comment"];
            billListFormEntity["Model"] = DynObjectTransverter.DynObjectToJson(billListForm);
            GatewayFactory.Default.Save(billListFormEntity);
        }
        public DynObject GetBillListFormByID(int billListFormID)
        {
            DynObject result = null;
            DynEntity billListForm = GatewayFactory.Default.Find("BillListForm", billListFormID);
            if (billListForm != null)
            {
                result = DynObjectTransverter.JsonToDynObject(billListForm["Model"] as string);
            }
            return result;
        }

        public void ModifyBillListForm(DynObject billListForm)
        {
            Check.Require(billListForm != null, "BillListForm对象不允许为空!");
            DynEntity billListFormEntity = GatewayFactory.Default.Find("BillListForm", billListForm["BillListFormID"]);
            Check.Require(billListFormEntity != null, "BillListForm在数据库中不存在无法修改!");

            billListFormEntity["BillListFormName"] = billListForm["BillListFormName"];
            billListFormEntity["MasterType"] = billListForm["MasterType"];
            billListFormEntity["DetailType"] = billListForm["DetailType"];
            billListFormEntity["ReferTypes"] = billListForm["ReferTypes"];
            billListFormEntity["BillName"] = billListForm["BillName"];
            billListFormEntity["DetailMainReferType"] = billListForm["DetailMainReferType"];
            billListFormEntity["DetailMainReferName"] = billListForm["DetailMainReferName"];
            billListFormEntity["Comment"] = billListForm["Comment"];
            billListFormEntity["Model"] = DynObjectTransverter.DynObjectToJson(billListForm);
            //billListFormEntity["Script"] = GenerateBillListFormJsCode(billListForm);

            GatewayFactory.Default.Save(billListFormEntity);
        }

        private string GenerateBillListFormJsCode(DynObject billListForm, string templateName)
        {
            Rock.Dyn.Core.DynObject masterGrid = billListForm["MasterGrid"] as Rock.Dyn.Core.DynObject;
            List<Rock.Dyn.Core.DynObject> masterGridColumns = masterGrid["GridColumns"] as List<Rock.Dyn.Core.DynObject>;
            Rock.Dyn.Core.DynObject detailMainReferGrid = billListForm["DetailMainReferGrid"] as Rock.Dyn.Core.DynObject;
            List<Rock.Dyn.Core.DynObject> detailMainReferGridColumns = detailMainReferGrid["GridColumns"] as List<Rock.Dyn.Core.DynObject>;
            if (masterGridColumns.Count == 0)
            {
                return "对象模型未添加主表列表项";
            }
            if (detailMainReferGridColumns.Count == 0)
            {
                return "对象模型未添加明细主参照列表项";
            }           
            Dictionary<string, object> item = new Dictionary<string, object>();
            Template dictTemplate = Template.Create("BillListTemplatet", File.ReadAllText(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "bin\\Templatet\\" + templateName + ".txt", Encoding.UTF8));
            item.Add("billListForm", billListForm);
            return dictTemplate.Render("BillListTemplatet", item);
        }

        #endregion 单据列表模型维护

        #region 单据模型维护
        public void AddBillForm(DynObject billForm)
        {
            Check.Require(billForm != null, "BillForm对象不允许为空!");
            DynEntity billFormEntity = new DynEntity("BillForm");

            billFormEntity["BillFormID"] = billForm["BillFormID"];
            billFormEntity["BillFormName"] = billForm["BillFormName"];
            billFormEntity["MasterType"] = billForm["MasterType"];
            billFormEntity["DetailType"] = billForm["DetailType"];
            billFormEntity["ReferTypes"] = billForm["ReferTypes"];
            billFormEntity["BillName"] = billForm["BillName"];
            billFormEntity["BizService"] = billForm["BizService"];
            billFormEntity["DetailMainReferType"] = billForm["DetailMainReferType"];
            billFormEntity["DetailMainReferName"] = billForm["DetailMainReferName"];
            billFormEntity["Comment"] = billForm["Comment"];
            billFormEntity["Model"] = DynObjectTransverter.DynObjectToJson(billForm);
            GatewayFactory.Default.Save(billFormEntity);
        }
        public DynObject GetBillFormByID(int billFormID)
        {
            DynObject result = null;
            DynEntity billForm = GatewayFactory.Default.Find("BillForm", billFormID);
            if (billForm != null)
            {
                result = DynObjectTransverter.JsonToDynObject(billForm["Model"] as string);
            }
            return result;
        }

        public void ModifyBillForm(DynObject billForm)
        {
            Check.Require(billForm != null, "BillForm对象不允许为空!");
            DynEntity billFormEntity = GatewayFactory.Default.Find("BillForm", billForm["BillFormID"]);
            Check.Require(billFormEntity != null, "BillForm在数据库中不存在无法修改!");

            billFormEntity["BillFormName"] = billForm["BillFormName"];
            billFormEntity["MasterType"] = billForm["MasterType"];
            billFormEntity["DetailType"] = billForm["DetailType"];
            billFormEntity["ReferTypes"] = billForm["ReferTypes"];
            billFormEntity["BillName"] = billForm["BillName"];
            billFormEntity["BizService"] = billForm["BizService"];
            billFormEntity["DetailMainReferType"] = billForm["DetailMainReferType"];
            billFormEntity["DetailMainReferName"] = billForm["DetailMainReferName"];
            billFormEntity["Comment"] = billForm["Comment"];
            billFormEntity["Model"] = DynObjectTransverter.DynObjectToJson(billForm);
            //billFormEntity["Script"] = GenerateBillFormJsCode(billForm);

            GatewayFactory.Default.Save(billFormEntity);
        }

        private string GenerateBillFormJsCode(DynObject billForm, string templateName)
        {
            List<Rock.Dyn.Core.DynObject> masterFormItems = billForm["MasterFormItems"] as List<Rock.Dyn.Core.DynObject>;
            Rock.Dyn.Core.DynObject detailMainReferGrid = billForm["DetailMainReferGrid"] as Rock.Dyn.Core.DynObject;
            List<Rock.Dyn.Core.DynObject> detailMainReferGridColumns = detailMainReferGrid["GridColumns"] as List<Rock.Dyn.Core.DynObject>;
            if (masterFormItems.Count == 0)
            {
                return "单据模型未添加主表列表项";
            }
            if (detailMainReferGridColumns.Count == 0)
            {
                return "单据模型未添加明细主参照列表项";
            } 

            Dictionary<string, object> item = new Dictionary<string, object>();
            Template dictTemplate = Template.Create("BillTemplatet", File.ReadAllText(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "bin\\Templatet\\" + templateName + ".txt", Encoding.UTF8));
            item.Add("billForm", billForm);
            return dictTemplate.Render("BillTemplatet", item);
        }

        #endregion 单据模型维护

        #region 树模型维护
        public void AddTreeForm(DynObject treeForm)
        {
            Check.Require(treeForm != null, "TreeForm对象不允许为空!");
            DynEntity treeFormEntity = new DynEntity("TreeForm");

            treeFormEntity["TreeFormID"] = treeForm["TreeFormID"];
            treeFormEntity["TreeFormName"] = treeForm["TreeFormName"];
            treeFormEntity["ModelType"] = treeForm["ModelType"];
            treeFormEntity["ModelTypeName"] = treeForm["ModelTypeName"];
            treeFormEntity["ReferTypes"] = treeForm["ReferTypes"];
            treeFormEntity["ColumnCount"] = treeForm["ColumnCount"];
            treeFormEntity["Comment"] = treeForm["Comment"];
            treeFormEntity["Model"] = DynObjectTransverter.DynObjectToJson(treeForm);
            GatewayFactory.Default.Save(treeFormEntity);
        }
        public DynObject GetTreeFormByID(int treeFormID)
        {
            DynObject result = null;
            DynEntity treeForm = GatewayFactory.Default.Find("TreeForm", treeFormID);
            if (treeForm != null)
            {
                result = DynObjectTransverter.JsonToDynObject(treeForm["Model"] as string);
            }
            return result;
        }

        public void ModifyTreeForm(DynObject treeForm)
        {
            Check.Require(treeForm != null, "TreeForm对象不允许为空!");
            DynEntity treeFormEntity = GatewayFactory.Default.Find("TreeForm", treeForm["TreeFormID"]);
            Check.Require(treeFormEntity != null, "TreeForm在数据库中不存在无法修改!");

            treeFormEntity["TreeFormName"] = treeForm["TreeFormName"];
            treeFormEntity["ModelType"] = treeForm["ModelType"];
            treeFormEntity["ModelTypeName"] = treeForm["ModelTypeName"];
            treeFormEntity["ReferTypes"] = treeForm["ReferTypes"];
            treeFormEntity["ColumnCount"] = treeForm["ColumnCount"];
            treeFormEntity["Comment"] = treeForm["Comment"];
            treeFormEntity["Model"] = DynObjectTransverter.DynObjectToJson(treeForm);
            //treeFormEntity["Script"] = GenerateTreeFormJsCode(treeForm);

            GatewayFactory.Default.Save(treeFormEntity);
        }

        private string GenerateTreeFormJsCode(DynObject treeForm, string templateName)
        {
            List<Rock.Dyn.Core.DynObject> formItems = treeForm["FormItems"] as List<Rock.Dyn.Core.DynObject>;
            Rock.Dyn.Core.DynObject dataGrid = treeForm["DataGrid"] as Rock.Dyn.Core.DynObject;
            List<Rock.Dyn.Core.DynObject> gridColumns = dataGrid["GridColumns"] as List<Rock.Dyn.Core.DynObject>;
            if (formItems.Count == 0)
            {
                return "对象模型未添加表单项";
            }
            if (gridColumns.Count == 0)
            {
                return "对象模型未添加列表项";
            }           
            Dictionary<string, object> item = new Dictionary<string, object>();
            Template dictTemplate = Template.Create("TreeTemplatet", File.ReadAllText(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "bin\\Templatet\\" + templateName + ".txt", Encoding.UTF8));
            item.Add("treeForm", treeForm);
            return dictTemplate.Render("TreeTemplatet", item);
        }

        #endregion 树模型维护

        #region 关联实体模型维护
        public void AddRelationForm(DynObject relationForm)
        {
            Check.Require(relationForm != null, "RelationForm对象不允许为空!");
            DynEntity relationFormEntity = new DynEntity("RelationForm");

            relationFormEntity["RelationFormID"] = relationForm["RelationFormID"];
            relationFormEntity["RelationFormName"] = relationForm["RelationFormName"];
            relationFormEntity["RelationType"] = relationForm["RelationType"];
            relationFormEntity["MasterType"] = relationForm["MasterType"];
            relationFormEntity["SlaveType"] = relationForm["SlaveType"];
            relationFormEntity["MasterTypeName"] = relationForm["MasterTypeName"];
            relationFormEntity["SlaveTypeName"] = relationForm["SlaveTypeName"];
            relationFormEntity["ColumnCount"] = relationForm["ColumnCount"];
            relationFormEntity["Comment"] = relationForm["Comment"];
            relationFormEntity["Model"] = DynObjectTransverter.DynObjectToJson(relationForm);
            GatewayFactory.Default.Save(relationFormEntity);
        }
        public DynObject GetRelationFormByID(int relationFormID)
        {
            DynObject result = null;
            DynEntity relationForm = GatewayFactory.Default.Find("RelationForm", relationFormID);
            if (relationForm != null)
            {
                result = DynObjectTransverter.JsonToDynObject(relationForm["Model"] as string);
            }
            return result;
        }

        public void ModifyRelationForm(DynObject relationForm)
        {
            Check.Require(relationForm != null, "RelationForm对象不允许为空!");
            DynEntity relationFormEntity = GatewayFactory.Default.Find("RelationForm", relationForm["RelationFormID"]);
            Check.Require(relationFormEntity != null, "RelationForm在数据库中不存在无法修改!");

            relationFormEntity["RelationFormName"] = relationForm["RelationFormName"];
            relationFormEntity["RelationType"] = relationForm["RelationType"];
            relationFormEntity["MasterType"] = relationForm["MasterType"];
            relationFormEntity["SlaveType"] = relationForm["SlaveType"];
            relationFormEntity["MasterTypeName"] = relationForm["MasterTypeName"];
            relationFormEntity["SlaveTypeName"] = relationForm["SlaveTypeName"];
            relationFormEntity["ColumnCount"] = relationForm["ColumnCount"];
            relationFormEntity["Comment"] = relationForm["Comment"];
            relationFormEntity["Model"] = DynObjectTransverter.DynObjectToJson(relationForm);
            //relationFormEntity["Script"] = GenerateRelationFormJsCode(relationForm);

            GatewayFactory.Default.Save(relationFormEntity);
        }

        private string GenerateRelationFormJsCode(DynObject relationForm, string templateName)
        {
            List<Rock.Dyn.Core.DynObject> masterFormItems = relationForm["MasterFormItems"] as List<Rock.Dyn.Core.DynObject>;
            List<Rock.Dyn.Core.DynObject> masterGridColumns = relationForm["MasterGridColumns"] as List<Rock.Dyn.Core.DynObject>;
            List<Rock.Dyn.Core.DynObject> slaveGridColumns = relationForm["SlaveGridColumns"] as List<Rock.Dyn.Core.DynObject>;
            List<Rock.Dyn.Core.DynObject> slaveWaitGridColumns = relationForm["SlaveWaitGridColumns"] as List<Rock.Dyn.Core.DynObject>;
            if (masterFormItems.Count == 0)
            {
                return "主实体类型未添加表单项";
            }
            if (masterGridColumns.Count == 0)
            {
                return "主实体类型未添加表格列集合";
            }
            if (slaveGridColumns.Count == 0)
            {
                return "从实体类型未添加表格列集合";
            }
            if (slaveWaitGridColumns.Count == 0)
            {
                return "从实体类型未添加待选表格列集合";
            }

            Dictionary<string, object> item = new Dictionary<string, object>();
            Template dictTemplate = Template.Create("RelationTemplatet", File.ReadAllText(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "bin\\Templatet\\" + templateName + ".txt", Encoding.UTF8));
            item.Add("relationForm", relationForm);
            return dictTemplate.Render("RelationTemplatet", item);
        }

        #endregion 关联实体模型维护

        public string GenerateJsCode(DynObject fromModel, string templateType, string templateName)
        {
            string result;
            switch (templateType)
            {
                case "DictForm":
                    result = GenerateDictFormJsCode(fromModel, templateName);
                    break;
                case "BillListForm":
                    result = GenerateBillListFormJsCode(fromModel, templateName);
                    break;
                case "BillForm":
                    result = GenerateBillFormJsCode(fromModel, templateName);
                    break;
                case "TreeForm":
                    result = GenerateTreeFormJsCode(fromModel, templateName);
                    break;
                case "RelationForm":
                    result = GenerateRelationFormJsCode(fromModel, templateName);
                    break;
                default:
                    result = "模板类型不存在!";
                    break;
            }
            return result;
        }
    }
}
