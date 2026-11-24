using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mysql
{
    public class MysqlManager:Singleton<MysqlManager>
    {
        public SqlSugarClient SqlSugarDB;
        public void Init()
        {
            SqlSugarDB = new SqlSugarClient(
            new ConnectionConfig()
            {
                ConnectionString = "server=127.0.0.1;port=3306;uid=root;pwd=admin123;database=testdb;SslMode=None",
                DbType = SqlSugar.DbType.MySql,//设置数据库类型
                IsAutoCloseConnection = true,//自动释放数据务，如果存在事务，在事务结束后释放
                InitKeyType = InitKeyType.Attribute //从实体特性中读取主键自增列信息
            });
            Debug.LogInfo("数据库连接成功：127.0.0.1:3306.当前数据库：testdb");

            RemoveItemTemplateForeignKey();

            // 打印SQL日志
            SqlSugarDB.Aop.OnLogExecuting = (sql, pars) =>
            {
                Console.WriteLine(sql);
            };
        }

        private void RemoveItemTemplateForeignKey()
        {
            // Item details are maintained by the Unity client locally. Keep only the
            // user relation on user_item so local item IDs are not blocked by MySQL.
            string[] foreignKeys = { "user_item_ibfk_2", "fk_itemid" };
            foreach (string foreignKey in foreignKeys)
            {
                try
                {
                    SqlSugarDB.Ado.ExecuteCommand(
                        $"ALTER TABLE `user_item` DROP FOREIGN KEY `{foreignKey}`");
                    Debug.LogInfo("已移除 user_item 的物品模板外键：{0}", foreignKey);
                }
                catch (Exception ex)
                {
                    // The constraint may already be removed or may use another name.
                    Debug.LogInfo("跳过 user_item 外键 {0}：{1}", foreignKey, ex.Message);
                }
            }
        }
    }
}
