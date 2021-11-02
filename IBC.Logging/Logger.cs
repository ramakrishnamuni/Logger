using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace IBC.Logging
{
    public class Logger : ILoggerService
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        public async Task LogAsync(LogLevels eventLevel, Exception exInfo = null, CustomColumns values=null)
        {
            var levelSwitch = new LoggingLevelSwitch();

            await Task.Factory.StartNew(() =>
            {
                string information = GenerateMessageTemplate();
                var connectionString = Configuration.GetSection(Constants.DbConnection);
                var customLoggingLevel = Configuration.GetSection(Constants.CustomLoggingLevel);
                var loggingTable = Configuration.GetSection(Constants.LoggingTable);

                //Log in console to trace serilog issues
                Serilog.Debugging.SelfLog.Enable(msg => System.Diagnostics.Debug.WriteLine(msg));

                //Remove unnecessary columns
                var columnOptions = GetColumnOptions();
                columnOptions.Store.Remove(StandardColumn.Message);
                columnOptions.Store.Remove(StandardColumn.MessageTemplate);

                levelSwitch.MinimumLevel = GetLogEventLevel(customLoggingLevel.Value);
                var logLevel = GetLogEventLevel(eventLevel.ToString());

                using (var log = GetLog(connectionString.Value, loggingTable.Value, columnOptions, levelSwitch))
                {
                    if (log.IsEnabled(logLevel))
                    {
                        if (exInfo != null)
                        {
                            log.Write(logLevel, exInfo, information, new object[] { exInfo?.Message, values.UserName });
                        }
                        else
                        {
                            log.Write(logLevel, information, new object[] { null,values.UserName });
                        }
                    }
                }
            });
        }

        private string GenerateMessageTemplate() => "{ExceptionMessage}{UserName}";

        private LogLevels GetLogLevel(string value)
        {
            if (value == LogLevels.Debug.ToString())
            {
                return LogLevels.Debug;
            }
            return LogLevels.Debug;
        }

        private ColumnOptions GetColumnOptions()
        {
            return new ColumnOptions()
            {
                AdditionalColumns = new List<SqlColumn>
                  {
                        new SqlColumn {DataType = SqlDbType.NVarChar, ColumnName = "Source"},
                        new SqlColumn {DataType = SqlDbType.NVarChar,DataLength=100, ColumnName = "UserName"},
                        new SqlColumn {DataType = SqlDbType.NVarChar,DataLength=100, ColumnName = "ExceptionMessage"},
                  }
            };
        }

        private Serilog.Core.Logger GetLog(string connectionString, string loggingTable,
            ColumnOptions columnOptions, LoggingLevelSwitch levelSwitch)
        {
            return new LoggerConfiguration()
                    .MinimumLevel.ControlledBy(levelSwitch)
                    .WriteTo.MSSqlServer(connectionString, loggingTable, columnOptions: columnOptions,
                    autoCreateSqlTable: false
                    ).CreateLogger();
        }

        private LogEventLevel GetLogEventLevel(string value)
        {
            var level = LogEventLevel.Debug;

            try
            {
                level = (LogEventLevel)Enum.Parse(typeof(LogEventLevel), value);
            }
            catch (Exception ex)
            {
                var result = $"CLogger.Logger.getLogEventLevel(): exception getting customlogging level, defaulted to Debug Level. logLevel:{LogLevels.Debug}, exception:{ex}";
                System.Diagnostics.Debug.WriteLine(result);
            }
            return level;

        }
    }
}
