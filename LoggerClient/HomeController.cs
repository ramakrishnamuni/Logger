using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IBC.Logging;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LoggerClient
{
    [Route("Home")]
    public class HomeController : ControllerBase
    {
        private readonly ILoggerService logger;
        //private string _source="Test";

        public HomeController(ILoggerService logger)
        {
            this.logger = logger;
        }
        // GET: /<controller>/
        [Route("Get")]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            try
            {
                int d = 0;
                int s = 5 / d;
                await logger.LogAsync(LogLevels.Debug,null, new CustomColumns() { UserName="Rama"});
            }
            catch (Exception ex)
            {

                await logger.LogAsync(LogLevels.Error, ex, new CustomColumns() { UserName = "Rama" });

            }

            return new string[] { "value1", "value2" };
        }
    }
}
