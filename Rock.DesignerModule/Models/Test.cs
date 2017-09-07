using Rock.Dyn.Core;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using System.Data;
using Rock.Orm.Common.Design;
using Rock.Addition;
using Rock.Security;
using Rock.Biz.Entities;

namespace Rock.Addition
{
    public class ObjType : Rock.Orm.Common.Design.Entity
    {
        //

        public int TypeID { get; set; }

        //

        public string Name { get; set; }

        //

        public string CnName { get; set; }

        //

        public string Description { get; set; }

        //

        public string AppLevel { get; set; }

        //

        public bool IsDymatic { get; set; }

        //

        public int NextID { get; set; }

        //

        public string Definition { get; set; }

    }


}
namespace Rock.Security
{
    public class MenuItem : Rock.Orm.Common.Design.Entity
    {
        //

        public string Icon { get; set; }

        //资源ID

        [Description("资源ID")]
        public int? ActionID { get; set; }

        //排序

        [Description("排序")]
        public int? DisplayOrder { get; set; }

        //父菜单id

        [Description("父菜单id")]
        public int? ParentID { get; set; }

        //所属级次

        [Description("所属级次")]
        public int Grades { get; set; }

        //是否无权时隐藏

        [Description("是否无权时隐藏")]
        public bool HiddenNoRight { get; set; }

        //

        public string CommandText { get; set; }

        //菜单项名称

        [Description("菜单项名称")]
        public string MenuItemName { get; set; }

        //菜单项ID

        [Description("菜单项ID")]
        public int MenuItemID { get; set; }

    }


    public class ResourceGroup : Rock.Orm.Common.Design.Entity
    {
        //

        public string MemberType { get; set; }

        //

        public System.Object[] Resources { get; set; }

        //

        public int? ObjTypeID { get; set; }

        //父菜单id

        [Description("父菜单id")]
        public int? ParentID { get; set; }

        //所属级次

        [Description("所属级次")]
        public int? Grades { get; set; }

        //资源组名称

        [Description("资源组名称")]
        public string ResourceGroupName { get; set; }

        //资源组ID

        [Description("资源组ID")]
        public int? ResourceGroupID { get; set; }

    }


    public class Resource : Rock.Orm.Common.Design.Entity
    {
        //

        public Rock.Security.ResourceGroup ResourceGroup { get; set; }

        //

        public string Description { get; set; }

        //

        public int? ObjID { get; set; }

        //资源名称

        [Description("资源名称")]
        public string ResourceName { get; set; }

        //资源ID

        [Description("资源ID")]
        public int? ResourceID { get; set; }

    }


    public class User : Rock.Orm.Common.Design.Entity
    {
        //

        public string Comment { get; set; }

        //

        public int State { get; set; }

        //

        public int MainOrgID { get; set; }

        //

        public string Fax { get; set; }

        //

        public string UserType { get; set; }

        //

        public string QQ { get; set; }

        //

        public string Address { get; set; }

        //

        public DateTime? CardExpiryDate { get; set; }

        //

        public DateTime? CardIssueDate { get; set; }

        //

        public string CardNo { get; set; }

        //

        public string CardKind { get; set; }

        //

        public string Sex { get; set; }

        //申请审核时间

        [Description("申请审核时间")]
        public DateTime CreateTime { get; set; }

        //联系方式

        [Description("联系方式")]
        public string Mobile { get; set; }

        //Email

        [Description("Email")]
        public string Email { get; set; }

        //电话

        [Description("电话")]
        public string Telphone { get; set; }

        //

        public string Password { get; set; }

        //

        public string TrueName { get; set; }

        //登陆员工姓名

        [Description("登陆员工姓名")]
        public string UserName { get; set; }

        //登录员工id

        [Description("登录员工id")]
        public int UserID { get; set; }

    }


    public class Role : Rock.Orm.Common.Design.Entity
    {
        //

        public string Comment { get; set; }

        //

        public int? State { get; set; }

        //

        public string RoleKind { get; set; }

        //角色名称

        [Description("角色名称")]
        public string RoleName { get; set; }

        //

        public int RoleID { get; set; }

    }


    public class UserRole : Rock.Orm.Common.Design.Entity
    {
        //

        public int? RoleID { get; set; }

        //登录员工id

        [Description("登录员工id")]
        public int? UserID { get; set; }

    }


    public class Organization : Rock.Orm.Common.Design.Entity
    {
        //

        public string Comment { get; set; }

        //

        public int? UserID { get; set; }

        //

        public int? State { get; set; }

        //

        public string FullName { get; set; }

        //

        public string FullCode { get; set; }

        //

        public string FullID { get; set; }

        //

        public string Zip { get; set; }

        //

        public string Address { get; set; }

        //

        public string Fax { get; set; }

        //

        public string Phone { get; set; }

        //

        public string OrganizationCode { get; set; }

        //

        public string OrganizationKind { get; set; }

        //父级id

        [Description("父级id")]
        public int? ParentID { get; set; }

        //所属级次

        [Description("所属级次")]
        public int? Grades { get; set; }

        //

        public string OrganizationName { get; set; }

        //

        public int OrganizationID { get; set; }

    }


    public class Action : Rock.Orm.Common.Design.Entity
    {
        //

        public int ActionGroupID { get; set; }

        //

        public string ActionName { get; set; }

        //

        public int? ActionID { get; set; }

    }


    public class ActionGroup : Rock.Orm.Common.Design.Entity
    {
        //

        public int Grades { get; set; }

        //

        public int? ParentID { get; set; }

        //

        public string ActionGroupName { get; set; }

        //

        public int? ActionGroupID { get; set; }

    }


    public class UserGroup : Rock.Orm.Common.Design.Entity
    {
        //

        public int ParentID { get; set; }

        //级次

        [Description("级次")]
        public int Grades { get; set; }

        //

        public string UserGroupName { get; set; }

        //

        public int? UserGroupID { get; set; }

    }


    public class UserOrganization : Rock.Orm.Common.Design.Entity
    {
        //

        public int? UserID { get; set; }

        //

        public int? OrganizationID { get; set; }

    }


    public class UserGroupUser : Rock.Orm.Common.Design.Entity
    {
        //

        public int? UserID { get; set; }

        //

        public Rock.Security.UserGroup UserGroup { get; set; }

    }


    public class UserResourceGroup : Rock.Orm.Common.Design.Entity
    {
        //

        public int? ResourceGroupID { get; set; }

        //

        public int? UserGroupID { get; set; }

    }


    public class Employee : Rock.Orm.Common.Design.Entity
    {
        //

        public string EmployeeName { get; set; }

        //

        public int EmployeeID { get; set; }

    }


    public class Position : Rock.Orm.Common.Design.Entity
    {
        //

        public string PositionName { get; set; }

        //

        public int? PositionID { get; set; }

    }


    public class EmployeeOrganizationPosition : Rock.Orm.Common.Design.Entity
    {
        //

        public int? PositionID { get; set; }

        //

        public int? OrganizationID { get; set; }

        //

        public int? EmployeeID { get; set; }

    }


    public class ActionPermission : Rock.Orm.Common.Design.Entity
    {
        //自动主键

        [Description("自动主键")]
        public int ActionPermissionID { get; set; }

        //

        public string Comment { get; set; }

        //

        public int? ActionID { get; set; }

        //

        public int? RoleID { get; set; }

        //自动主键

        [Description("自动主键")]
        public string ActionPermissionName { get; set; }

    }


    public class ResourcePermission : Rock.Orm.Common.Design.Entity
    {
        //自动主键

        [Description("自动主键")]
        public int ResourcePermissionID { get; set; }

        //

        public string Comment { get; set; }

        //

        public int? ResourceGroupID { get; set; }

        //

        public int? RoleID { get; set; }

        //自动主键

        [Description("自动主键")]
        public string ResourcePermissionName { get; set; }

    }


    public class Authorize : Rock.Orm.Common.Design.Entity
    {
        //自动主键

        [Description("自动主键")]
        public int AuthorizeID { get; set; }

        //

        public int RoleID { get; set; }

        //

        public int OrganizationID { get; set; }

        //自动主键

        [Description("自动主键")]
        public string AuthorizeName { get; set; }

    }


    public class ResourceGroupOperate : Rock.Orm.Common.Design.Entity
    {
        //

        public Rock.Security.ResourceGroup ResourceGroup { get; set; }

        //

        public string DisplayName { get; set; }

        //

        public string ResourceGroupOperateName { get; set; }

        //

        public int? ResourceGroupOperateID { get; set; }

    }


}
namespace Rock.Biz.Entities
{
    public class KCXP : Rock.Orm.Common.Design.Entity
    {
        //

        public int? WorkNodeID { get; set; }

        //

        public string KCXPName { get; set; }

        //

        public int? KCXPID { get; set; }

    }


    public interface IBaoPan
    {
        string StartBaoPan(int selfID, string title, string path, string userName, string passWord);

        string StopBaoPan(int selfID, string title);

        bool Print(int selfID);

        bool QueryPrint(int selfID);

    }


}
