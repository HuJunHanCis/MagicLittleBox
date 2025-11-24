using System;
using System.IO;
using System.Windows;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace MagicLittleBox
{
    
    public class LevelNumberEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            int levelNum = 0;
            
            switch (logEvent.Level)
            {
                case LogEventLevel.Debug:
                    levelNum = 1;
                    break;
                case LogEventLevel.Information:
                    levelNum = 2;
                    break;
                case LogEventLevel.Warning:
                    levelNum = 3;
                    break;
                case LogEventLevel.Error:
                    levelNum = 4;
                    break;
                case LogEventLevel.Fatal:
                    levelNum = 5;
                    break;
            }

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("LevelNum", levelNum));
        }
    }
    
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ConfigureLogger();                // 先把日志也拉起来
            
            try
            {
                Log.Information("[100]: 应用程序启动");
                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex,"[100]: 应用程序崩溃");
                throw;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("[100]: 应用程序关闭");
            Log.CloseAndFlush(); 
            base.OnExit(e);
        }
        
        // 1 = Debug 
        // 2 = Information
        // 3 = Warning
        // 4 = Error
        // 5 = Fatal 

        private void ConfigureLogger()
        {
            var logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "MagicLittleBox",
                "Logs");
    
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            
            string logFileName = $"{DateTime.Now:yyMMdd-HHmm}.txt";
            string logFilePath = Path.Combine(logDirectory, logFileName);
            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.With(new LevelNumberEnricher())
                .WriteTo.File(
                    path: logFilePath,
                    rollingInterval: RollingInterval.Infinite,
                    outputTemplate: "[{Timestamp:HH:mm:ss}][{LevelNum}]: {Message:lj}{NewLine}{Exception}")
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss}][{LevelNum}]: {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("[100]: 日志系统初始化完成");
        }
    }
}