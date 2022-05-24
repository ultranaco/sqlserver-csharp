using Microsoft.SqlServer.Types;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace Ultranaco.Database.SQLServer.Objects
{
  public static class DataReaderExtend
  {
    public static Geometry GetGeography(this object column)
    {
      var geoReader = new SqlServerBytesReader { IsGeography = true };
      var geo = (SqlGeography)column;
      var geometry = geoReader.Read(geo.Serialize().Buffer);
      return geometry;
    }
  }
}
