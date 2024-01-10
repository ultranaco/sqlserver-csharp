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
  [SetUp]
  public void Setup()
  {
    IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory() + "/../../..")
    .AddJsonFile("appSettings.json")
    .Build()
    .AttachApplicationKeys()
    .AttachConnectionString();


    SqlServicePool.Set("UltranacoPool");
    SqlServicePool.Set("MasterPool");
  }

  public IntegrationTest()
  {
    IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory() + "/../../..")
    .AddJsonFile("appSettings.json")
    .Build()
    .AttachApplicationKeys()
    .AttachConnectionString();


    SqlServicePool.Set("UltranacoPool");
    SqlServicePool.Set("MasterPool");
  }

  [Test, Order(1)]
  public void CreateUltranacoDatabase()
  {
    TestContext.Progress.WriteLine("CreateUltranacoDatabase: starting at {0}", DateTime.Now);
    var parameters = new List<SqlParameter>();
    var query = @"
    IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'UltranacoLabs')
    BEGIN
      CREATE DATABASE [UltranacoLabs]
      COLLATE Latin1_General_CI_AS;
      ALTER DATABASE [UltranacoLabs] SET  DISABLE_BROKER;
      PRINT 'UltranacoLabs was created succesfully';
    END
    ELSE
    BEGIN
      PRINT 'UltranacoLabs database already exists';
    END
";
    SqlService.ExecuteNonQuery(query, parameters, "MasterPool");
    Assert.Pass();
  }

  [Test, Order(2)]
  public void EstablishConnectionToUltranacoDB()
  {
    TestContext.Progress.WriteLine("EstablishConnectionToUltranacoDB: starting at {0}", DateTime.Now);
    var connection = SqlServicePool.Set("UltranacoPool");

    TestContext.Progress.WriteLine($@"connection state: {connection}");
    Assert.Pass();
  }

  [Test, Order(3)]
  public void CreateDummyTable()
  {
    TestContext.Progress.WriteLine("CreateDummyTable: starting at {0}", DateTime.Now);
    // INFO: uncomment for debug porpouse
    // SqlConnectionPool.Set("UltranacoPool");

    var parameters = new List<SqlParameter>();
    var query = @"
    IF EXISTS 
    (
      SELECT TABLE_NAME 
      FROM INFORMATION_SCHEMA.TABLES 
      WHERE TABLE_NAME = 'Products'
    )
    BEGIN
      DROP TABLE Products
    END
    
    CREATE TABLE Products
    (
      id int IDENTITY(1,1) PRIMARY KEY,
      name varchar(255) NOT NULL,
      code varchar(255) NOT NULL,
      price float
    );
";
    SqlService.ExecuteNonQuery(query, parameters, "UltranacoPool");
    Assert.Pass();
  }

  [Test, Order(4)]
  public void InsertAndDynamicParametersTest()
  {
    TestContext.Progress.WriteLine("InsertAndDynamicParametersTest: starting at {0}", DateTime.Now);
    var parameters = new List<SqlParameter>();

    parameters.Add("name", "name_1");
    parameters.Add("code", "code_1");
    parameters.Add("price", 1.4);

    var query = @"
    INSERT INTO Products (name, code, price) VALUES(@name, @code, @price);
    ";

    for (var index = 0; index < 100; index++)
    {
      SqlService.ExecuteNonQuery(query, parameters, "UltranacoPool");
      parameters[0].Value = "name_" + index;
      parameters[1].Value = "code_" + index;
      parameters[2].Value = 1.4 + index;
    }

    Assert.Pass();
  }

  [Test, Order(5)]
  public void ExecuteScalarTest()
  {
    TestContext.Progress.WriteLine("ExecuteScalarTest: starting at {0}", DateTime.Now);
    var parameters = new List<SqlParameter>();
    var query = "SELECT COUNT(id) totalProducts FROM Products";

    int count = (int)SqlService.ExecuteScalar(query, parameters, "UltranacoPool");

    Assert.AreEqual(100, count);
  }
}