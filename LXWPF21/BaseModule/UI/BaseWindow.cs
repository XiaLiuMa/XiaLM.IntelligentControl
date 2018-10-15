using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace BaseModule.UI
{
    public class BaseWindow : Window
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.ResizeMode = ResizeMode.NoResize;
            this.WindowStyle = WindowStyle.None;

            //加载UIModule程序集窗口的多语言资源
            Uri res = null;
            Type type = this.GetType();
            string AssemblyName = type.Assembly.GetName().Name;
            if (AssemblyName == "UIModule")
            {
                //if (LangManager.Instance.CurrentLang == LangType.English)
                //{
                //    res = new Uri($"/{AssemblyName};component/{type.Namespace.Replace("UIModule.", string.Empty)}/Lang/{type.Name}_EN.xaml", UriKind.Relative);
                //}
                //else if (LangManager.Instance.CurrentLang == LangType.Chinese)
                //{
                //    res = new Uri($"/{AssemblyName};component/{type.Namespace.Replace("UIModule.", string.Empty)}/Lang/{type.Name}_CN.xaml", UriKind.Relative);
                //}
                res = new Uri($"/{AssemblyName};component/{type.Namespace.Replace("UIModule.", string.Empty)}/Lang/{type.Name}_CN.xaml", UriKind.Relative);
                this.Resources.MergedDictionaries.Clear();
                this.Resources.MergedDictionaries.Add((ResourceDictionary)Application.LoadComponent(res));
            }

            this.Loaded += new RoutedEventHandler(BaseWindow_Loaded);
        }

        void BaseWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Owner == null && this != Application.Current.MainWindow)
            {
                this.Owner = Application.Current.MainWindow;
            }
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);   //所有弹出窗体,禁用关闭,最大化,最小化按钮
        }
    }
}
