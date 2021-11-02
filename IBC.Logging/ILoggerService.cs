using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IBC.Logging
{
    public interface ILoggerService
    {
      public  Task LogAsync(LogLevels eventLevel, Exception ex = null, CustomColumns values=null);

    }
}
