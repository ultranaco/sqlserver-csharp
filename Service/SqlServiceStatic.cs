using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ultranaco.Database.SQLServer.Service
{
  public partial class SqlService
  {
    public static List<T> ExecuteList<T>(string sql, IEnumerable<SqlParameter> parameters, Func<IDataReader, T> mapper, string connectionString)
    {
      IEnumerable<T> result;
      result = _sqlService.ExecuteReader(sql, parameters, mapper);
      return result.ToList();
    }

    public static T ExecuteObject<T>(string sql, IEnumerable<SqlParameter> parameters, Func<IDataReader, T> mapper, string connectionString)
    {
      T result;

      result = _sqlService.ExecuteReader(sql, parameters, mapper).FirstOrDefault();

      return result;
    }

    public static T ScrollForward<T>(string sql, IEnumerable<SqlParameter> @params, string cursorName, Func<IDataReader, T> rowMapper, string connectionString)
    {
      T result;

      result = _sqlService.ScrollForward(sql, @params, cursorName, rowMapper);

      return result;
    }

    public static IEnumerable<T> ExecuteReader<T>(string sql, IEnumerable<SqlParameter> parameters, Func<IDataReader, T> mapper, string connectionString)
    {
      IEnumerable<T> result;

      result = _sqlService.ExecuteReader(sql, parameters, mapper);

      return result;
    }

    public static int ExecuteNonQuery(string sql, IEnumerable<SqlParameter> parameters, string connectionString)
    {
      int result;

      result = _sqlService.ExecuteNonQuery(sql, parameters);

      return result;
    }

    public static object ExecuteScalar(string sql, IEnumerable<SqlParameter> parameters, string connectionString)
    {
      object result;

      result = _sqlService.ExecuteScalar(sql, parameters);

      return result;
    }
  }
}
