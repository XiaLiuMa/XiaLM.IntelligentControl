using System;

namespace BaseModule.UserRole
{
  public enum Role
  {
    Operator,
    Admin,
    Maintainer
  }

  public class User
  {
    public string Name { get; set; }
        //增加Password字段。
    public string Password { get; set; }
    public DateTime LastLoginTime { get; set; }
    public Role Role { get; set; }
        public override string ToString()
        {
            return Name + ":" + Password + ":" + Role;
        }
    }

}
