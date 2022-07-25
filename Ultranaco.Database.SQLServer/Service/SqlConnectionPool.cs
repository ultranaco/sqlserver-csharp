using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;
using Ultranaco.Appsettings;

namespace Ultranaco.Database.SQLServer.Service;

public class SqlConnectionPool
{
  private static ConcurrentDictionary<string, SqlConnection> _connections = new ConcurrentDictionary<string, SqlConnection>();

  public SqlConnectionPool(string key, string connectionString = null, bool useAppSettingsFile = true)
  {
    Set(key, connectionString, useAppSettingsFile);
  }

  public static SqlConnection Get(string key)
  {
    SqlConnection connection;

    var wasObtained = _connections.TryGetValue(key, out connection);

    if (!wasObtained)
    {
      throw new Exception("SqlConnection: an error ocurred or not found a connection while trying to retrieve it from collection");
    }

    if (connection.State != ConnectionState.Open)
    {
      if (connection.State == ConnectionState.Closed
      || connection.State == ConnectionState.Broken)
      {
        connection.Open();
      }
    }

    return connection;
  }

  public static SqlConnection Set(string key, string connectionString = null, bool useAppSettingsFile = true)
  {
    if (useAppSettingsFile && connectionString == null)
    {
      connectionString = ConnectionStringParameter.Get(key);
    }
    else if (connectionString == null)
    {
      throw new Exception("SqlConnectionPool: connnection string is not set");
    }

    var connection = new SqlConnection(connectionString);
    var isAdded = _connections.TryAdd(key, connection);

    if (!isAdded)
    {
      throw new Exception("an error ocurred while adding a connection to collection");
    }

    connection.Open();

    return connection;
  }
}