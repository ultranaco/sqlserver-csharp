using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ultranaco.Database.SQLServer.Service
{

   public static class OpertatorCompareExtend
   {
      public static string ToSQL(this OperatorCompare operatr)
      {
         switch (operatr)
         {
            case OperatorCompare.GreaterThan:
               return ">";
            case OperatorCompare.GreaterThanOrEqual:
               return ">=";
            case OperatorCompare.LessThan:
               return "<";
            case OperatorCompare.LessThanOrEqual:
               return "<=";
            case OperatorCompare.Distinct:
               return "<>";
            case OperatorCompare.NotIn:
               return "NOT IN";
            case OperatorCompare.Not:
            case OperatorCompare.In:
               return operatr.ToString().ToUpper();
            default:
               return operatr.ToString();
         }
      }
   }
}
