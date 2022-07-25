using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Ultranaco.Database.SQLServer.Service;

public partial class SqlService
{
  public static T ExecuteObject<T>(string sql, IEnumerable<SqlParameter> parameters, Func<IDataReader, T> mapper, string keyConnectionString)
  {
    var service = SqlServicePool.Get(keyConnectionString);
    return service.ExecuteObject(sql, parameters, mapper);
  }

  public static List<T> ExecuteList<T>(string sql, IEnumerable<SqlParameter> parameters, Func<IDataReader, T> mapper, string keyConnectionString)
  {
    var service = SqlServicePool.Get(keyConnectionString);
    return service.ExecuteList(sql, parameters, mapper);
  }

  public static IEnumerable<T> ExecuteReader<T>(string sql, IEnumerable<SqlParameter> parameters, Func<IDataReader, T> mapper, string keyConnectionString, int indexBreak = -1, int commmandTimeout = 0)
  {
    var service = SqlServicePool.Get(keyConnectionString);
    return service.ExecuteReader(sql, parameters, mapper, indexBreak, commmandTimeout);
  }

  public static int ExecuteNonQuery(string sql, IEnumerable<SqlParameter> parameters, string keyConnectionString, int commmandTimeout = 0)
  {
    var service = SqlServicePool.Get(keyConnectionString);
    return service.ExecuteNonQuery(sql, parameters, commmandTimeout);
  }

  public static object ExecuteScalar(string sql, IEnumerable<SqlParameter> parameters, string keyConnectionString, int commmandTimeout = 0)
  {
    var service = SqlServicePool.Get(keyConnectionString);
    return service.ExecuteScalar(sql, parameters, commmandTimeout);
  }
}