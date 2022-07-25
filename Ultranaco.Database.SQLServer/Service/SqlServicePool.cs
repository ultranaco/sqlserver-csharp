using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;
using Ultranaco.Appsettings;

namespace Ultranaco.Database.SQLServer.Service;

public class SqlServicePool
{
  private static ConcurrentDictionary<string, SqlService> _connections = new ConcurrentDictionary<string, SqlService>();

  public SqlServicePool(string key, string connectionString = null, bool useAppSettingsFile = true)
  {
    Set(key, null, connectionString, useAppSettingsFile);
  }

  public static SqlService Get(string key)
  {
    SqlService service;

    var wasObtained = _connections.TryGetValue(key, out service);

    if (!wasObtained)
    {
      throw new Exception("SqlConnection: an error ocurred or not found a connection while trying to retrieve it from collection");
    }

    var connection = service.Connection;

    if (connection.State != ConnectionState.Open)
    {
      if (connection.State == ConnectionState.Closed
      || connection.State == ConnectionState.Broken)
      {
        connection.Open();
      }
    }

    return service;
  }

  public static SqlConnection Set(string key, Action<object, SqlInfoMessageEventArgs> messageHandler = null, string connectionString = null, bool useAppSettingsFile = true)
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
    var sqlService = new SqlService(connection);

    var isAdded = _connections.TryAdd(key, sqlService);

    if (!isAdded)
    {
      throw new Exception("an error ocurred while adding a connection to collection");
    }

    connection.Open();

    if (messageHandler != null)
    {
      // connection.StatisticsEnabled = true;
      connection.InfoMessage += (sender, @event) =>
      {
        messageHandler(sender, @event);
      };
    }


    return connection;
  }
}