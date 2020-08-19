using GeoAPI;
using GeoAPI.Geometries;
using Microsoft.SqlServer.Types;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Ultranaco.Database.SQLServer.Objects
{
  public static class DataReaderExtend
  {
    public static IGeometry GetGeography(this object column)
    {
      NetTopologySuiteBootstrapper.Bootstrap();
      var geoReader = new SqlServerBytesReader { IsGeography = true, HandleSRID = true };
      var geo = (SqlGeography)column;
      var geometry = geoReader.Read(geo.Serialize().Buffer);
      return geometry;
    }
  }
}
