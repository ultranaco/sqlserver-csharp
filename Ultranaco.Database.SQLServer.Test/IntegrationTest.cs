using System;
using NUnit.Framework;
using Ultranaco.Database.SQLServer.Service;
using Microsoft.Extensions.Configuration;
using System.IO;
using Ultranaco.Appsettings;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ultranaco.Database.SQLServer.Test;

public class IntegrationTest
{
  private SqlService _sql;

  public IntegrationTest()
  {
    IConfiguration configuration = new ConfigurationBuilder()
   .SetBasePath(Directory.GetCurrentDirectory() + "/../../..")
   .AddJsonFile("appSettings.json")
   .Build()
   .AttachConnectionString();

    var connection = SqlConnectionPool.Set("MasterPool");
    TestContext.Progress.WriteLine($@"connection state: {connection.State}");

    this._sql = new SqlService();
  }

  [SetUp]
  public void Setup()
  {
  }

  [Test, Order(1)]
  public void CreateUltranacoDatabase()
  {
    var parameters = new List<SqlParameter>();
    var query = @"
    IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'UltranacoLabs')
    BEGIN
      CREATE DATABASE [UltranacoLabs]
      COLLATE Latin1_General_CI_AS;
      ALTER DATABASE [UltranacoLabs] SET  DISABLE_BROKER;
    END
";
    var rowAffected = this._sql.ExecuteNonQuery(query, parameters, "MasterPool");

    TestContext.Progress.WriteLine("Rows affected {0}", rowAffected);
    Assert.Pass();
  }

  [Test, Order(2)]
  public void EstablishConnectionToUltranacoDB()
  {
    var connection = SqlConnectionPool.Set("UltranacoPool");
    TestContext.Progress.WriteLine($@"connection state: {connection.State}");
    Assert.Pass();
  }

  [Test, Order(3)]
  public void CreateDummyTable()
  {
    // INFO: uncomment for debug porpouse
    // SqlConnectionPool.Set("UltranacoPool");

    var parameters = new List<SqlParameter>();
    var query = @"
    IF NOT EXISTS 
    (
      SELECT TABLE_NAME 
      FROM INFORMATION_SCHEMA.TABLES 
      WHERE TABLE_NAME = 'Products'
    )
    BEGIN
      CREATE TABLE Products
      (
        id int IDENTITY(1,1) PRIMARY KEY,
        name varchar(255) NOT NULL,
        code varchar(255) NOT NULL,
        price float
      );
    END
";
    var rowAffected = this._sql.ExecuteNonQuery(query, parameters, "UltranacoPool");

    TestContext.Progress.WriteLine("Rows affected {0}", rowAffected);
    Assert.Pass();
  }

  [Test, Order(4)]
  public void InsertAndDynamicParametersTest()
  {
    var parameters = new List<SqlParameter>();

    parameters.Add("name", "name_1");
    parameters.Add("code", "code_1");
    parameters.Add("price", 1.4);

    var query = @"
    INSERT INTO Products (name, code, price) VALUES(@name, @code, @price);
    ";

    for (var index = 0; index < 100; index++)
    {
      this._sql.ExecuteNonQuery(query, parameters, "UltranacoPool");
      parameters[0].Value = "name_" + index;
      parameters[1].Value = "code_" + index;
      parameters[2].Value = 1.4 + index;
    }

    Assert.Pass();
  }

  [Test, Order(5)]
  public void ExecuteScalarTest()
  {
    var parameters = new List<SqlParameter>();
    var query = "SELECT COUNT(id) totalProducts FROM Products";

    int count = (int)this._sql.ExecuteScalar(query, parameters, "UltranacoPool");

    Assert.AreEqual(100, count);
  }
}