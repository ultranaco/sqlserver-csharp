using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ultranaco.Database.SQLServer.Service
{
   public static class SqlParameterExtend
   {
      public static void Add(this List<SqlParameter> sqlParameters, string parameterName, object value)
      {
         sqlParameters.Add(new SqlParameter(parameterName, (object)(value == null ? DBNull.Value : value)));
      }
   }
}
