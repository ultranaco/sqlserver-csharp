﻿using Ultranaco.Objects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Ultranaco.Database.SQLServer.Service
{
  public partial class SqlService : IDisposable
  {
    public int _offset = 0;
    public string ConnectionString { get; private set; }

    public SqlService(string connectionString)
    {
      this.ConnectionString = connectionString;
    }

    public T ExecuteObject<T>(string sql, IEnumerable<SqlParameter> parameters, Func<IDataReader, T> mapper, bool isStoreProcedure = false)
    {
      return ExecuteReader(sql, parameters, mapper, isStoreProcedure, 0).FirstOrDefault();
    }

    public List<T> ExecuteList<T>(string sql, IEnumerable<SqlParameter> parameters, Func<IDataReader, T> mapper, bool isStoreProcedure = false)
    {
      return ExecuteReader(sql, parameters, mapper, isStoreProcedure).ToList();
    }

    public IEnumerable<T> ExecuteReader<T>(string sql, IEnumerable<SqlParameter> parameters, Func<IDataReader, T> mapper, bool isStoreProcedure = false, int indexBreak = -1, int commmandTimeout = 0)
    {
      List<T> result = new List<T>();

      using(var connection = new SqlConnection(this.ConnectionString))
      {
        connection.Open();
        using (var command = new SqlCommand(sql, connection))
        {
          if (isStoreProcedure)
            command.CommandType = CommandType.StoredProcedure;
            
          command.CommandTimeout = commmandTimeout;
          command.Parameters.AddRange(parameters.ToArray());
          using (IDataReader reader = command.ExecuteReader())
          {
            var index = 0;
            while (reader.Read())
            {
              result.Add(mapper(reader));
              if (indexBreak > -1 && indexBreak == index)
                break;
              index++;
            }

            command.Parameters.Clear();
            parameters = new List<SqlParameter>();
          }
        }
        connection.Close();
      }

      return result;
    }

    public int ExecuteNonQuery(string sql, IEnumerable<SqlParameter> parameters, bool isStoreProcedure = false, int commmandTimeout = 0)
    {
      int result;

      using(var connection = new SqlConnection(this.ConnectionString))
      {
        connection.Open();
        using (var command = new SqlCommand(sql, connection))
        {
          if (isStoreProcedure)
            command.CommandType = CommandType.StoredProcedure;

          command.CommandTimeout = commmandTimeout;
          command.Parameters.AddRange(parameters.ToArray());
          result = command.ExecuteNonQuery();
          command.Parameters.Clear();
          parameters = new List<SqlParameter>();
        }
        connection.Close();
      }

      return result;
    }

    public object ExecuteScalar(string sql, IEnumerable<SqlParameter> parameters, bool isStoreProcedure = false, int commmandTimeout = 0)
    {
      object result;

      using (var connection = new SqlConnection(this.ConnectionString))
      {
        connection.Open();
        using (var command = new SqlCommand(sql, connection))
        {
          if (isStoreProcedure)
            command.CommandType = CommandType.StoredProcedure;

          command.CommandTimeout = commmandTimeout;
          command.Parameters.AddRange(parameters.ToArray());
          result = command.ExecuteScalar();
          command.Parameters.Clear();
          parameters = new List<SqlParameter>();
        }
        connection.Close();
      }

      return result;
    }

    public T Scroll<T>(string sql, IEnumerable<SqlParameter> @params, string orderBy, Func<IDataReader, T> rowMapper)
    {
      sql = String.Format(@"
SELECT  * FROM 
(
  {0}
) rowSet
ORDER BY {1} 
OFFSET {2} ROWS FETCH NEXT 1 ROWS ONLY
", sql, orderBy, _offset).Replace(";", "");
      try
      {
        var result = this.ExecuteObject(sql, @params, rowMapper);
        _offset++;
        return result;
      }
      catch (Exception e)
      {
        this.Dispose();
        throw new Exception("SQL error in scroll", e);
      }
    }

    /// <summary>
    /// Retrieves fetch rows from sql sentences, if not anymore restart cursor or sqlConnection closes when cursor is deadllocked automatically
    /// </summary>
    /// <typeparam name="T">Generic type</typeparam>
    /// <param name="sql">sql sentence</param>
    /// <param name="params"></param>
    /// <param name="cursorName"></param>
    /// <param name="db"></param>
    /// <param name="rowMapper"></param>
    /// <param name="timeOut"></param>
    /// <returns></returns>
    public T ScrollForward<T>(string sql, IEnumerable<SqlParameter> @params, string cursorName, Func<IDataReader, T> rowMapper)
    {
      sql = String.Format(@"
DECLARE @cur_status int;
SET @cur_status = CURSOR_STATUS('global', '{1}');

IF @cur_status = 1
	BEGIN
		FETCH NEXT FROM {1}
	END
ELSE
	BEGIN
      IF @cur_status = 0
      BEGIN
		   CLOSE {1}
         DEALLOCATE {1}
      END
      ELSE
      BEGIN
		   IF @cur_status = -1 OR @cur_status = -2
		   BEGIN
			   DEALLOCATE {1}
		   END
		   DECLARE {1} CURSOR FORWARD_ONLY GLOBAL
		   FOR 
			   {0}
		   OPEN {1}
		   FETCH NEXT FROM {1}
      END
	END
", sql, cursorName);
      try
      {
        return ExecuteObject(sql, @params, rowMapper);
      }
      catch (Exception e)
      {
        this.Dispose();
        throw new Exception("Error in cursor", e);
      }
    }

    /// <summary>
    /// Closes scroll by cursorName
    /// </summary>
    /// <param name="cursorName"></param>
    /// <param name="db"></param>
    /// <param name="timeOut"></param>
    /// <returns></returns>
    public int CloseScroll(string cursorName)
    {
      var sql = String.Format(@"
DECLARE @cur_status int;
SET @cur_status = CURSOR_STATUS('global', @cursorName);
IF @cur_status = -1
	BEGIN
		DEALLOCATE {0}
	END
ELSE
	BEGIN
		CLOSE {0}
      DEALLOCATE {0}
	END
SET @cur_status = CURSOR_STATUS('global', @cursorName);
SELECT @cur_status cursor_status;
", cursorName);
      try
      {
        var result = this.ExecuteObject(sql, new SqlParameter[]
        {
              new SqlParameter("@cursorName", cursorName)
        }, (r) =>
        {
          return r["cursor_status"].GetInt32();
        });

        return result;

      }
      catch (Exception e)
      {
        this.Dispose();
        throw new Exception("Error in cursor", e);
      }
    }

    public void Dispose()
    {
      this._offset = 0;
    }
  }
}
