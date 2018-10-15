using BaseModule;
using BaseModule.UI;
using BaseModule.UserRole;
using CustomControls;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace LXWPF21PC
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : BaseWindow
    {
        //用来存储最近登陆过的用户的用户名；
        private List<string> names = null;
        public LoginWindow()
        {
            InitializeComponent();

            //获取最近登陆过的用户的信息。
            System.Data.DataSet ds = DBHelper.Query("select username from user where lastlogintime is not null order by lastlogintime desc  limit 5");
            names = new List<string>();
            int n = ds.Tables[0].Rows.Count;
            AutoCompleteEntry autoEntry = null;
            for (int i = 0; i < n; i++)
            {
                string name = ds.Tables[0].Rows[i][0].ToString();
                autoEntry = new AutoCompleteEntry(name, null);
                names.Add(name);
                userNameBox.AddItem(autoEntry);
            }
            if (n > 0)
            {
                userNameBox.Text = ds.Tables[0].Rows[0][0].ToString();
            }
            else
            {
                userNameBox.Text = "User";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UserManager um = UserManager.Instance;
            string pwd = pwdBox.Password;
            string userName = userNameBox.Text;
            //当为普通用户的时候
            DateTime loginTime = DateTime.Now;
            //不成功
            if (!um.Login(userName, pwd))
            {

                lblMsg.Content = "用户名或密码错误";
                //密码或用户名错误提示效果
                DoubleAnimationUsingKeyFrames dau = new DoubleAnimationUsingKeyFrames();
                dau.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0))));
                dau.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5))));
                dau.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2.5))));
                dau.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(3.5))));
                lblMsg.BeginAnimation(Label.OpacityProperty, dau);//开始动画

            }
            //成功时候，进去主界面
            else
            {
                // DBHelper.ExecuteSql($"update user set PassWord='{strMD5}' where UserName='{username}'");

                int n = DBHelper.ExecuteSql($"update user set LastLoginTime='{loginTime}' where UserName='{userName}'");
                if (n > 0)
                {
                    // GoToMainWindow(userName);
                    GoToMainWindow();
                }

            }

        }

        private void GoToMainWindow()
        {
            //lblMsg.BeginAnimation(Label.OpacityProperty, dau);//开始动画
            DoubleAnimation da = new DoubleAnimation();
            //动画时间
            da.From = this.Top;
            da.To = 1500;
            da.Duration = TimeSpan.FromSeconds(0.5);
            da.Completed += Da_Completed;
            this.BeginAnimation(Window.TopProperty, da);

            // Window mainWindow = new MainWindow(name);
            Window mainWindow = new MainWindow();

            this.Close();
            mainWindow.Show();
        }

        private void Da_Completed(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BaseWindow_Loaded(object sender, RoutedEventArgs e)
        {


            //DoubleAnimation da = new DoubleAnimation();
            //da.From = 0;    //起始值
            //da.To = 1;      //结束值
            //da.Duration = TimeSpan.FromSeconds(1.5);         //动画持续时间
            //this.BeginAnimation(Window.OpacityProperty, da);


            //DoubleAnimationUsingKeyFrames dak = new DoubleAnimationUsingKeyFrames();
            ////关键帧定义
            //dak.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0))));
            //dak.KeyFrames.Add(new LinearDoubleKeyFrame(100, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.1))));
            //dak.KeyFrames.Add(new LinearDoubleKeyFrame(200, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.2))));
            //dak.KeyFrames.Add(new LinearDoubleKeyFrame(300, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.25))));
            //dak.KeyFrames.Add(new LinearDoubleKeyFrame(this.ActualHeight, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.3))));


            //this.BeginAnimation(Window.HeightProperty, dak);
        }
    }
}
