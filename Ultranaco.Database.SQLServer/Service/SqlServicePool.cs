using System;
using System.Collections.Concurrent;
using Ultranaco.Appsettings;

namespace Ultranaco.Database.SQLServer.Service;

public class SqlServicePool
{
  private static ConcurrentDictionary<string, SqlService> _connections = new ConcurrentDictionary<string, SqlService>();

  public SqlServicePool(string key, string connectionString = null, bool useAppSettingsFile = true)
  {
    SqlServicePool.Set(key, connectionString, useAppSettingsFile);
  }

  public static SqlService Get(string key)
  {
    SqlService service;

    var wasObtained = _connections.TryGetValue(key, out service);

    if (!wasObtained)
    {
      throw new Exception("SqlConnection: an error ocurred or not found a connection while trying to retrieve it from collection");
    }

    return service;
  }

  public static string Set(string key, string connectionString = null, bool useAppSettingsFile = true)
  {
    if (useAppSettingsFile && connectionString == null)
    {
      connectionString = ConnectionStringParameter.Get(key);
    }
    else if (connectionString == null)
    {
      throw new Exception("SqlConnectionPool: connnection string is not set");
    }

    Console.WriteLine(string.Format("ConnectionString to Service: {0}", connectionString));

    var sqlService = new SqlService(connectionString);

    var isAdded = _connections.TryAdd(key, sqlService);

    if (!isAdded)
    {
      throw new Exception("an error ocurred while adding a connection to collection");
    }

    return connectionString;
  }
}
