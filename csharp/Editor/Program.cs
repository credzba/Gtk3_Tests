using System;
using NLog;

namespace PythonEditor
{
    public class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            // Configure NLog (basic console logging)
            var config = new NLog.Config.LoggingConfiguration();
            var consoleTarget = new NLog.Targets.ConsoleTarget("console");
            config.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget);
            LogManager.Configuration = config;

            logger.Info("Python Editor starting");

            // Initialize GTK
            CustomMainLoop.Instance.Init();

            // Create and show the editor
            var editor = new PythonEditorControl();
            editor.Show();

            // Run the main loop
            CustomMainLoop.Instance.Run();

            logger.Info("Python Editor closed");
        }
    }
}