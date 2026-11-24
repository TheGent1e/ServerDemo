using Mysql.Controllers;
using Mysql.Models;

public enum RegisterResultType
{
    defaultError,// 其他错误
    Success,// 注册成功
    UserNameExists,// 用户名已存在
    InvalidInput,// 输入不合法

}
public enum LoginResultType
{
    defaultError,// 其他错误
    Success,// 登录成功
    UserNotFound,// 用户不存在
    InvalidPassword,// 密码错误

}
namespace Mysql.Service
{
    public class UserService
    {
        // 注册新用户，成功返回 true；用户名已存在或输入不合法时返回 false。
        public RegisterResultType Register(string userName, string password)
        {
            try
            {
                GameUser user = null;
                // 查询数据库中是否存在相同用户名的用户 
                user = MysqlManager.Instance.SqlSugarDB.Queryable<GameUser>().Where(u => u.UserName == userName).First();
                if(user != null)
                {
                    return  RegisterResultType.UserNameExists;
                }
                user = new GameUser
                {
                    UserName = userName,
                    NickName = userName,
                    Level = 1,
                    IsActive = 1
                };
                user.Password = password;
                user.CreateTime = DateTime.Now;
                //插入新用户到数据库
                MysqlManager.Instance.SqlSugarDB.Insertable(user).ExecuteCommand();
                return RegisterResultType.Success;

            }
            catch (Exception e)
            {

                Debug.Log("注册查询数据库异常");
                return RegisterResultType.defaultError;
            }
           
        }

        // 验证账号密码。成功时更新最后登录时间并返回用户资料。
        public LoginResultType Login(string userName, string password, out GameUser loginUser)
        {
            GameUser user = null;
            loginUser = null;
            try
            {
                // 查询数据库中是否存在相同用户名的用户 
                user = MysqlManager.Instance.SqlSugarDB.Queryable<GameUser>().Where(u => u.UserName == userName).First();
                if (user!=null)
                {
                    if (user.IsActive == 0)
                    {
                        return LoginResultType.defaultError;
                    }
                    if (user.Password==password)
                    {
                        user.LastLoginTime = DateTime.Now;
                        MysqlManager.Instance.SqlSugarDB.Updateable<GameUser>()
                            .SetColumns(it => it.LastLoginTime == user.LastLoginTime)
                            .Where(it => it.Id == user.Id)
                            .ExecuteCommand();
                        loginUser = user;
                        return LoginResultType.Success;
                    }
                    return LoginResultType.InvalidPassword;
                }
                Debug.LogWarn("没有查询到任何数据");
                return LoginResultType.UserNotFound;
            }
            catch (Exception e)
            {
                Debug.LogError("登录出现异常" + e);
                return LoginResultType.defaultError;
            }
        }

        public LoginResultType Login(string userName, string password)
        {
            return Login(userName, password, out _);
        }
    }
}
