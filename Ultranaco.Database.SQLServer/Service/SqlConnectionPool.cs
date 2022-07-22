using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using Ultranaco.Appsettings;

namespace Ultranaco.Database.SQLServer.Service;

public class SqlConnectionPool
{
  private static ConcurrentDictionary<string, SqlConnection> _connections = new ConcurrentDictionary<string, SqlConnection>();

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

  public static SqlConnection Set(string key, string connectionString, bool useAppSettingsFile = true)
  {
    if (useAppSettingsFile)
    {
      connectionString = ConnectionStringParameter.Get(connectionString);
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