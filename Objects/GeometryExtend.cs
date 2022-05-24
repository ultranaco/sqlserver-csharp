using Microsoft.SqlServer.Types;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Text;

namespace Ultranaco.Database.SQLServer.Objects
{
  public static class GeometryExtend
  {
    public static string ToGeoJson(this Geometry geometry, Dictionary<string, object> attributes = null)
    {
      var feature = new NetTopologySuite.Features.Feature(geometry, new NetTopologySuite.Features.AttributesTable());

      if (attributes != null)
        foreach (var att in attributes)
          feature.Attributes.Add(att.Key, att.Value);
      
      var sb = new StringBuilder();
      var serializer = NetTopologySuite.IO.GeoJsonSerializer.Create();
      serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
      using (var sw = new StringWriter(sb))
      {
        serializer.Serialize(sw, feature);
      }
      var result = sb.ToString();
      return result;
    }

    public static Geometry GeoJsonToGeometry(this string geojson, int SRID = 4326)
    {
      JsonTextReader reader = new JsonTextReader(new StringReader(geojson));
      var serializer = NetTopologySuite.IO.GeoJsonSerializer.Create();
      var feature = serializer.Deserialize<NetTopologySuite.Features.Feature>(reader);
      feature.Geometry.SRID = SRID;
      return feature.Geometry;
    }

    public static SqlGeography GeoJsonToSqlBytes(this string geojson, int SRID = 4326, bool isgeography = true)
    {
      var geometry = geojson.GeoJsonToGeometry(SRID);
      var geography = geometry.GeometryToSQL();
      return geography;
    }

    public static SqlGeography GeometryToSQL(this Geometry geometry, int SRID = 4326, bool isgeography = true)
    {
      var geometryWriter = new SqlServerBytesWriter { IsGeography = isgeography };
      geometry.SRID = SRID;
      var bytes = geometryWriter.Write(geometry);
      var geographyRaw = new SqlBytes(bytes);
      var geography = SqlGeography.Deserialize(geographyRaw);
      return geography;
    }

    public static SqlBytes GeometryToSQlBytes(this Geometry geometry, int SRID = 4326, bool isgeography = true)
    {
      var geometryWriter = new SqlServerBytesWriter { IsGeography = isgeography };
      geometry.SRID = SRID;
      var bytes = geometryWriter.Write(geometry);
      var geographyRaw = new SqlBytes(bytes);
      return geographyRaw;
    }

    public static string GeometryToHexString(this Geometry geometry, int SRID = 4326, bool isgeography = true)
    {
      var sqlGeography = geometry.GeometryToSQlBytes();
      var rawPolygon = BitConverter.ToString(sqlGeography.Value)
            .Replace("-", "").ToUpper();

      return rawPolygon;
    }
  }
}
