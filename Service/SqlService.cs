using Ultranaco.Appsettings;
using Ultranaco.Objects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ultranaco.Database.SQLServer.Service
{
   public partial class SqlService : IDisposable
   {
      private SqlConnection _connection;
      private string _connectionString;
      public int _offset = 0;

      public SqlService(string connectionString, bool isPrefixName = true)
      {
         this.Connect(connectionString, isPrefixName);
      }

      private void Connect(string connectionString, bool isPrefixName = true)
      {
         if (isPrefixName)
            _connectionString = ConnectionStringService.Get(connectionString);
         else
            _connectionString = connectionString;
         
         if (_connection == null || (_connection != null && _connection.State != ConnectionState.Open))
         {
            _connection = new SqlConnection(_connectionString);
            _connection.Open();
         }
      }

      public T ExecuteObject<T>(string sql, IEnumerable<SqlParameter> parameters, Func<IDataReader, T> mapper)
      {
         return this.ExecuteList(sql, parameters, mapper).FirstOrDefault();
      }

      public List<T> ExecuteList<T>(string sql, IEnumerable<SqlParameter> parameters, Func<IDataReader, T> mapper)
      {
         return ExecuteReader(sql, parameters, mapper).ToList();
      }

      public IEnumerable<T> ExecuteReader<T>(string sql, IEnumerable<SqlParameter> parameters, Func<IDataReader, T> mapper)
      {
         List<T> result = new List<T>();

         using (var command = new SqlCommand(sql, _connection))
         {
            command.Parameters.AddRange(parameters.ToArray());
            using (IDataReader reader = command.ExecuteReader())
            {
               while (reader.Read())
                  result.Add(mapper(reader));

               command.Parameters.Clear();
               parameters = new List<SqlParameter>();
            }
         }

         return result;
      }

      public int ExecuteNonQuery(string sql, IEnumerable<SqlParameter> parameters)
      {
         int result;
         using (var command = new SqlCommand(sql, _connection))
         {
            command.Parameters.AddRange(parameters.ToArray());
            result = command.ExecuteNonQuery();
            command.Parameters.Clear();
            parameters = new List<SqlParameter>();
         }
         return result;
      }

      public object ExecuteScalar(string sql, IEnumerable<SqlParameter> parameters)
      {
         object result;
         using (var command = new SqlCommand(sql, _connection))
         {
            command.Parameters.AddRange(parameters.ToArray());
            result = command.ExecuteScalar();
            command.Parameters.Clear();
            parameters = new List<SqlParameter>();
         }

         return result;
      }

      public T Scroll<T>(string sql, IEnumerable<SqlParameter> @params, string orderBy, Func<IDataReader, T> rowMapper)
      {
         this.Connect(_connectionString, false);
         sql = String.Format(@"
SELECT  * FROM (
{0}
) rowSet
ORDER BY {1} 
OFFSET {2} ROWS FETCH NEXT 1 ROWS ONLY
",sql, orderBy, _offset).Replace(";", "");
         try
         {
            var result = this.ExecuteObject(sql, @params, rowMapper);
            _offset++;
            _connection.Close();
            return result;
         }
         catch (Exception e)
         {
            this.Dispose();
            throw e;
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
      public T ScrollForward<T>(string sql, IEnumerable<SqlParameter> @params, string cursorName, Func<IDataReader, T>  rowMapper)
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
            return this.ExecuteObject(sql, @params, rowMapper);
         }
         catch (Exception e)
         {
            this.Dispose();
            throw e;
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
            throw e;
         }
      }

      public void Dispose()
      {
         _connection.Close();
         _offset = 0;
      }
   }
}
