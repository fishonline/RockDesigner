using Rock.DesignerModule.Service;
using System;
using System.Windows;
using Telerik.Windows.Controls;

namespace Rock.Designer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {        
        protected override void OnStartup(StartupEventArgs e)
        {
            bool isSuccess = false;
            try
            {
                string _appPath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

                AppLoader appLoader = AppLoader.Instance;
                appLoader.AppID = 0;
                appLoader.AppPath = _appPath;
                appLoader.LoadBaseSystem();
                appLoader.LoadAppInfrastructure();
                isSuccess = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("初始化数据失败 \r\n原因:" + ex.Message + "\r\n" + ex.StackTrace);
                this.OnExit(null);
                return;
            }
            if (isSuccess)
            {
                try
                {
                    base.OnStartup(e);

                    StyleManager.ApplicationTheme = new Windows7Theme();

                    (new Bootstrapper()).Run();
                }
                catch (Exception)
                {
                    isSuccess = false;
                    this.OnExit(null);
                    return;
                }
            }

            if (!isSuccess)
            {
                this.OnExit(null);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Application.Current.Shutdown();
            base.OnExit(e);
        }
    }
}
