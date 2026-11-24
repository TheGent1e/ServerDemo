using Mysql.Service;

using Mysql.Models;

namespace Mysql.Controllers
{
    public class UserController : Singleton<UserController>
    {
        private readonly UserService _userService = new();


        // 注册
        public RegisterResultType Register(string userName, string password)
        {

            if (string.IsNullOrWhiteSpace(userName)
                || string.IsNullOrWhiteSpace(password)
                || userName.Length < 4 || userName.Length > 16)
            {
                return RegisterResultType.InvalidInput;
            }
            else
            {
                return _userService.Register(userName, password);
            }

        }

        // 登录。
        public LoginResultType Login(string userName, string password, out Mysql.Models.GameUser loginUser)
        {
            return _userService.Login(userName, password, out loginUser);
        }

        public LoginResultType Login(string userName, string password)
        {
            return Login(userName, password, out _);
        }
    }
}

