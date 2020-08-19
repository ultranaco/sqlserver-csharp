using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ultranaco.Database.SQLServer
{
   public enum OperatorCompare
   {
      GreaterThan = 0,
      GreaterThanOrEqual,
      LessThan,
      LessThanOrEqual,
      And,
      Or,
      Not,
      NotIn,
      In,
      Distinct
   }

}
