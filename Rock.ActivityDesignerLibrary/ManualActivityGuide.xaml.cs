using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Shapes;

namespace Rock.ActivityDesignerLibrary
{
    /// <summary>
    /// ManualActivityGuide.xaml 的交互逻辑
    /// </summary>
    public partial class ManualActivityGuide : Window
    {
        DesignService designService = new DesignService();

        private int _type = 0;
        private int _item = 0;
        private int _position = -2;
        private string _command = "";
        private string _expression = "";
        private string _description = "";
        private List<string> _lstExpression = new List<string>();
        private int _workflowActivityID = 0;

        public int Type
        {
            get { return _type; }
            set { _type = value; }
        }
        public int Item
        {
            get { return _item; }
            set { _item = value; }
        }
        public int Position
        {
            get { return _position; }
            set { _position = value; }
        }
        public string Command
        {
            get { return _command; }
            set { _command = value; }
        }
        public string Expression
        {
            get { return _expression; }
            set { _expression = value; }
        }
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
        public int WorkflowActivityID
        {
            get { return _workflowActivityID; }
            set { _workflowActivityID = value; }
        }
        public ManualActivityGuide()
        {
            InitializeComponent();

            List<ComboItem> list = new List<ComboItem>();
            list.Add(new ComboItem() { ID = 0, Text = "角色" });
            list.Add(new ComboItem() { ID = 1, Text = "用户" });
            list.Add(new ComboItem() { ID = 2, Text = "逻辑代词" });
            list.Add(new ComboItem() { ID = 3, Text = "部门" });

            cbxTypes.ItemsSource = list;
            cbxTypes.DisplayMemberPath = "Text";
            cbxTypes.SelectedValuePath = "ID";
        }

        private void btnCommit_Click(object sender, RoutedEventArgs e)
        {
            _expression = string.Join("^", this._lstExpression.ToArray());

            if (_workflowActivityID == 0)
            {
                //获取工作流活动的ID
                _workflowActivityID = designService.GetNextID("WorkflowActivity");
            }

            //设置ShowDialog的返回值
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //设置ShowDialog的返回值
            this.DialogResult = false;
            this.Close();
        }

        private void cbxTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboItem selectedItem = cbxTypes.SelectedItem as ComboItem;
            List<ComboItem> comboItems = new List<ComboItem>();
            DataTable dataTable;
            switch (selectedItem.Text)
            {
                case "用户":
                    dataTable = designService.GetDataTable("select UserID, UserName from [User]");
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        ComboItem cb = new ComboItem();
                        cb.ID = Convert.ToInt32(dataTable.Rows[i]["UserID"]);
                        cb.Text = dataTable.Rows[i]["UserName"].ToString();
                        comboItems.Add(cb);
                    }
                    cbxItems.ItemsSource = comboItems;
                    cbxItems.DisplayMemberPath = "Text";
                    cbxItems.SelectedValuePath = "ID";
                    cbxItems.SelectedValue = _item;
                    cbxPosition.ItemsSource = null;
                    break;
                case "角色":
                    dataTable = designService.GetDataTable("select RoleID, RoleName from [Role]");
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        ComboItem cb = new ComboItem();
                        cb.ID = Convert.ToInt32(dataTable.Rows[i]["RoleID"]);
                        cb.Text = dataTable.Rows[i]["RoleName"].ToString();
                        comboItems.Add(cb);
                    }
                    cbxItems.ItemsSource = comboItems;
                    cbxItems.DisplayMemberPath = "Text";
                    cbxItems.SelectedValuePath = "ID";
                    cbxItems.SelectedValue = _item;

                    cbxPosition.ItemsSource = null;
                    break;
                case "逻辑代词":
                    comboItems.Add(new ComboItem() { ID = 0, Text = "发起者" });
                    comboItems.Add(new ComboItem() { ID = 1, Text = "上一步执行者" });
                    comboItems.Add(new ComboItem() { ID = 2, Text = "发起者的上一级部门的" });
                    comboItems.Add(new ComboItem() { ID = 3, Text = "上一步执行者的上一级部门的" });

                    cbxItems.ItemsSource = comboItems;
                    cbxItems.DisplayMemberPath = "Text";
                    cbxItems.SelectedValuePath = "ID";
                    cbxItems.SelectedValue = _item;

                    //职位:
                    List<ComboItem> positionsComboItems = new List<ComboItem>();
                    positionsComboItems.Add(new ComboItem() { ID = 0, Text = "自己" });

                    //如果有职位,从数据库获取职位,目前只有自己
                    cbxPosition.ItemsSource = positionsComboItems;
                    cbxPosition.DisplayMemberPath = "Text";
                    cbxPosition.SelectedValuePath = "ID";
                    cbxPosition.SelectedValue = _position;
                    break;
                case "部门":
                    comboItems.Add(new ComboItem() { ID = 0, Text = "部门" });

                    cbxItems.ItemsSource = comboItems;
                    cbxItems.DisplayMemberPath = "Text";
                    cbxItems.SelectedValuePath = "ID";
                    cbxItems.SelectedValue = _item;
                    break;
                default:
                    break;
            }
        }

        private void cbxItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (Convert.ToInt32(cbxTypes.SelectedValue) == 3)
            //{
            //    ComboItem selectedItem = cbxItems.SelectedItem as ComboItem;

            //    if (selectedItem != null)
            //    {
            //        Dictionary<string, object> dictParams = new Dictionary<string, object>();

            //        string sqlStr = "SELECT Position.PositionID, Position.PositionName FROM OrganizationPosition INNER JOIN Position ON OrganizationPosition.PositionID = Position.PositionID WHERE OrganizationPosition.OrganizationID = " + selectedItem.ID.ToString();
            //        dictParams.Add("sqlString", sqlStr);

            //        object result = DynTypeManager.MethodHandler(null, "QueryService_ExecQuery", dictParams);

            //        DynObject position = result as DynObject;

            //        if (position != null)
            //        {
            //            List<DynObject> rows = position["Rows"] as List<DynObject>;

            //            if (rows != null)
            //            {
            //                List<ComboItem> comboItems = new List<ComboItem>();

            //                for (int i = 0; i < rows.Count; i++)
            //                {
            //                    List<string> value = rows[i]["Values"] as List<string>;
            //                    if (value != null)
            //                    {
            //                        ComboItem cb = new ComboItem();
            //                        cb.ID = Convert.ToInt32(value[0]);
            //                        cb.Text = value[1].ToString();

            //                        comboItems.Add(cb);
            //                    }
            //                }

            //                cbxPosition.ItemsSource = comboItems;
            //                cbxPosition.DisplayMemberPath = "Text";
            //                cbxPosition.SelectedValuePath = "ID";
            //                cbxPosition.SelectedValue = _position;
            //            }
            //        }
            //    }
            //}
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cbxTypes.SelectedIndex = _type;
            txtCmd.Text = _command;
            txtDescription.Text = _description;

            if (_expression != null)
            {
                foreach (var item in _expression.Split('^'))
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        _lstExpression.Add(item);
                    }
                }

                this.lbxExpressions.ItemsSource = null;
                this.lbxExpressions.ItemsSource = _lstExpression;
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            this.tabControl1.SelectedIndex++;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            this.tabControl1.SelectedIndex--;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            int index = lbxExpressions.Items.IndexOf(lbxExpressions.SelectedItem);

            if (index > 0)
            {
                _lstExpression.RemoveAt(index);

                this.lbxExpressions.ItemsSource = null;
                this.lbxExpressions.ItemsSource = _lstExpression;
            }
            else
            {
                MessageBox.Show("没有选中项不能删除！");
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string expression = this.txtExpression.Text.Trim();
            if (!string.IsNullOrEmpty(expression))
            {
                _lstExpression.Add(expression);

                this.lbxExpressions.ItemsSource = null;
                this.lbxExpressions.ItemsSource = _lstExpression;

                this.txtExpression.Clear();
            }
            else
            {
                MessageBox.Show("输入的条件表达式为空");
            }
        }

        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl tc = sender as TabControl;
            this.ChangeButtonStatus(tc.SelectedIndex.ToString());
        }

        private void ChangeButtonStatus(string statusCode)
        {
            switch (statusCode)
            {
                case "0":
                    this.btnBack.Visibility = Visibility.Collapsed;
                    this.btnCommit.Visibility = Visibility.Collapsed;
                    this.btnNext.Visibility = Visibility.Visible;
                    break;

                case "1":
                    this.btnBack.Visibility = Visibility.Visible;
                    this.btnCommit.Visibility = Visibility.Collapsed;
                    this.btnNext.Visibility = Visibility.Visible;
                    break;

                case "2":
                    this.btnBack.Visibility = Visibility.Visible;
                    this.btnCommit.Visibility = Visibility.Visible;
                    this.btnNext.Visibility = Visibility.Collapsed;
                    break;

                default:
                    this.btnBack.Visibility = Visibility.Collapsed;
                    this.btnCommit.Visibility = Visibility.Collapsed;
                    this.btnNext.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}
