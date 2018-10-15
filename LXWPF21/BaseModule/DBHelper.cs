using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace BaseModule
{
    public static class DBHelper
  {
    private static string connectionString = ConfigurationManager.ConnectionStrings[0].ConnectionString;
    private static DbProviderFactory provider = DbProviderFactories.GetFactory(ConfigurationManager.ConnectionStrings[0].ProviderName);

    /// <summary>
    /// 执行SQL语句
    /// </summary>
    /// <param name="SQLString">要执行的SQL</param>
    /// <returns>受影响的行数</returns>
    public static int ExecuteSql(string SQLString)
    {
      using (DbConnection connection = provider.CreateConnection())
      {
        connection.ConnectionString = connectionString;
        using (DbCommand cmd = provider.CreateCommand())
        {
          cmd.Connection = connection;
          cmd.CommandText = SQLString;
          connection.Open();
          return cmd.ExecuteNonQuery();
        }
      }
    }

    /// <summary>
    /// 根据查询获取一个数据集
    /// </summary>
    /// <param name="SQLString">查询的SQL</param>
    /// <returns>数据集</returns>
    public static DataSet Query(string SQLString)
    {
      using (DbConnection connection = provider.CreateConnection())
      {
        connection.ConnectionString = connectionString;
        using (DbCommand cmd = provider.CreateCommand())
        {
          cmd.Connection = connection;
          cmd.CommandText = SQLString;

          DataSet ds = new DataSet();
          DbDataAdapter adapter = provider.CreateDataAdapter();
          adapter.SelectCommand = cmd;
          adapter.Fill(ds);
          return ds;
        }
      }
    }



    /// <summary>
    /// 执行多条SQL语句，实现数据库事务。
    /// </summary>
    /// <param name="SQLStringList">多条SQL语句</param>	
    public static int ExecuteSqlTran(IEnumerable<String> SQLStringList)
    {
      using (DbConnection connection = provider.CreateConnection())
      {
        connection.ConnectionString = connectionString;
        connection.Open();
        DbTransaction tx = connection.BeginTransaction();
        using (DbCommand cmd = provider.CreateCommand())
        {    
          cmd.Connection = connection;
          cmd.Transaction = tx;
          try
          {
            int count = 0;
            foreach (string sql in SQLStringList)
            {
              if (sql.Trim().Length > 1)
              {
                cmd.CommandText = sql;
                count += cmd.ExecuteNonQuery();
              }
            }
            tx.Commit();
            return count;
          }
          catch
          {
            tx.Rollback();
            return 0;
          }
        }
      }
    }


  }
}