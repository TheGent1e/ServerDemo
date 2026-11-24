using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mysql.Models
{
    [SugarTable("game_user")]
    public class GameUser
    {
        [SugarColumn(ColumnName = "id", IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }
        [SugarColumn(ColumnName = "username")]
        public string UserName { get; set; }
        [SugarColumn(ColumnName = "password")]
        public string Password { get; set; }
        [SugarColumn(ColumnName = "nickname")]
        public string NickName { get; set; }
        [SugarColumn(ColumnName = "level")]
        public int Level { get; set; }
        [SugarColumn(ColumnName = "create_time")]
        public DateTime CreateTime { get; set; }
        [SugarColumn(ColumnName = "last_login_time")]
        public DateTime? LastLoginTime { get; set; }
        [SugarColumn(ColumnName = "is_active")]
        public int IsActive { get; set; }
    }

}
