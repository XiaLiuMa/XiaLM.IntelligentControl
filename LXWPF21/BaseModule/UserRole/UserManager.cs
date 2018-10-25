using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace BaseModule.UserRole
{
    public delegate void UserChanged();

    /// <summary>
    /// 用户管理器
    /// </summary>
    public class UserManager
    {
        private static UserManager instace;
        private readonly static object objLock = new object();
        /// <summary>
        /// 登录用户改变事件
        /// </summary>
        public event UserChanged OnUserChanged;

        private UserManager()
        {

        }
        public static UserManager Instance
        {
            get
            {
                if (instace == null)
                {
                    instace = new UserManager();
                }
                return instace;
            }
        }

        /// <summary>
        /// 当前登录用户
        /// </summary>
        public User CurrentUser { get; private set; } = new User() { Name = "anonymous", Role = Role.Operator };

        

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string EncodeWithMD5(string str)
        {
            string pwd = string.Empty;
            MD5 md5 = MD5.Create();
            Byte[] byt = md5.ComputeHash(Encoding.Unicode.GetBytes(str));
            for (int i = 0; i < byt.Length; ++i)
            {
                pwd += byt[i].ToString("x");
            }
            return pwd;
        }

        /// <summary>
        /// 登录，成功后CurrentUser保存当前登录的用户
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="pwd">密码</param>
        /// <returns>true：成功 false：失败</returns>
        public bool Login(string username, string pwd)
        {
            bool result = false;

            //对密码先进行MD5加密
            string strMD5 = EncodeWithMD5(pwd);
            DataSet ds = DBHelper.Query($"select * from iuser where UserName='{username}' and Password='{strMD5}'");
            if (ds.Tables[0].Rows.Count > 0)
            {
                result = true;
                CurrentUser.Name = username;
                CurrentUser.Role = (Role)int.Parse(ds.Tables[0].Rows[0]["Role"].ToString());
                EventTrigger.FireEvent(this, OnUserChanged, nameof(OnUserChanged));
            }
            return result;
        }

        /// <summary>
        /// 更新当前用户的密码。返回结果
        /// 0：成功
        /// -1：新密码不能和旧密码一样
        /// -2：旧密码错误或用户不存在
        /// -3：密码最近9次已使用过
        /// -4：数据库错误
        /// </summary>
        /// <param name="oldPwd">旧密码</param>
        /// <param name="newPwd">新密码</param>
        /// <returns></returns>
        public int UpdatePassword(string oldPwd, string newPwd)
        {
            if (newPwd == oldPwd)
            {
                return -1;
            }
            oldPwd = EncodeWithMD5(oldPwd);
            newPwd = EncodeWithMD5(newPwd);

            DataSet ds = DBHelper.Query($"select * from iuser where UserName='{CurrentUser.Name}' and Password='{oldPwd}'");
            if (ds.Tables[0].Rows.Count == 0)
            {
                return -2;
            }
            DataRow dr = ds.Tables[0].Rows[0];
            StringBuilder sb = new StringBuilder();
            for (int i = 2; i <= 10; ++i)
            {
                if (dr["Password" + i].ToString() == newPwd)
                {
                    return -3;
                }
                sb.AppendFormat(",Password{0}='{1}'", i, dr["Password" + (i == 2 ? "" : (i - 1).ToString())]);
            }

            int cnt = DBHelper.ExecuteSql($"update iuser set PassWord='{newPwd}'{sb.ToString()} where UserName='{CurrentUser.Name}'");
            if (cnt != 1)
            {
                return -4;
            }
            return 0;
        }

        /// <summary>
        /// 创建一个新用户。返回结果
        /// 0：成功
        /// -1：当前用户角色非维护员
        /// -2：用户名已存在
        /// -3：数据库错误
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="pwd">密码</param>
        /// <param name="role">角色</param>
        /// <returns></returns>
        public int CreatUser(string username, string pwd, Role role)
        {
            if (CurrentUser.Role != Role.Maintainer)
            {
                return -1;
            }

            DataSet ds = DBHelper.Query($"select * from iuser where UserName='{username}'");
            if (ds.Tables[0].Rows.Count > 0)
            {
                return -2;
            }

            //对密码先进行MD5加密
            string strMD5 = EncodeWithMD5(pwd);
            int cnt = DBHelper.ExecuteSql($"insert into iuser (UserName, Password, Role) values ('{username}','{strMD5}',{(int)role})");
            if (cnt != 1)
            {
                return -3;
            }
            return 0;
        }

        /// <summary>
        /// 重置一个用户的密码为用户名，只能重置角色比自己低的用户。返回结果 
        /// -1：用户名不存在
        /// -2：当前用户角色没权限
        /// -3：数据库错误
        /// </summary>
        /// <param name="username">用户名</param>
        /// <returns></returns>
        public int ResetPassword(string username)
        {
            DataSet ds = DBHelper.Query($"select * from iuser where UserName='{username}'");
            if (ds.Tables[0].Rows.Count == 0)
            {
                return -1;
            }

            if (CurrentUser.Role <= (Role)int.Parse(ds.Tables[0].Rows[0]["Role"].ToString()))
            {
                return -2;
            }

            //对密码先进行MD5加密
            string strMD5 = EncodeWithMD5(username);
            int cnt = DBHelper.ExecuteSql($"update iuser set PassWord='{strMD5}' where UserName='{username}'");
            if (cnt != 1)
            {
                return -3;
            }
            return 0;
        }

    }
}