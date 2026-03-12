// Program.cs
using System;
using System.IO;
using NLog;
using UI;

public class Program
{
    // Lazy NLog singleton — initialized only after AssemblyResolve is wired
    private static readonly Lazy<Logger> _logger =
        new Lazy<Logger>(() => LogManager.GetCurrentClassLogger());
    private static Logger logger => _logger.Value;

    static void Main(string[] args)
    {
        // Wire AssemblyResolve BEFORE touching NLog or GTK
        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

        // NLog configuration is loaded from NLog.config automatically
        logger.Info("ShardManager starting");

        string shardsFile = FindShardsFile(args);
        if (shardsFile == null)
        {
            Console.Error.WriteLine("Could not locate UO_Copilot.shards.");
            Console.Error.WriteLine("Usage: ShardManager [path/to/UO_Copilot.shards]");
            Environment.Exit(1);
        }

        logger.Info("Using shards file: {0}", shardsFile);

        var mainWindow = new ShardSelector(shardsFile);
        mainWindow.Run();

        logger.Info("ShardManager exiting");
        Console.WriteLine("Main window closed.");
    }

    private static string FindShardsFile(string[] args)
    {
        // 1. Command-line argument
        if (args != null && args.Length > 0 && File.Exists(args[0]))
            return Path.GetFullPath(args[0]);

        // 2. Next to the executable
        string exeDir = AppDomain.CurrentDomain.BaseDirectory;
        string candidate = Path.Combine(exeDir, "UO_Copilot.shards");
        if (File.Exists(candidate)) return candidate;

        // 3. Current working directory
        if (File.Exists("UO_Copilot.shards"))
            return Path.GetFullPath("UO_Copilot.shards");

        // 4. User home directory
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        candidate = Path.Combine(home, "UO_Copilot.shards");
        if (File.Exists(candidate)) return candidate;

        return null;
    }

    private static System.Reflection.Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        string exeDir = AppDomain.CurrentDomain.BaseDirectory;
        string name = new System.Reflection.AssemblyName(args.Name).Name;
        string path = Path.Combine(exeDir, name + ".dll");
        if (File.Exists(path))
            return System.Reflection.Assembly.LoadFrom(path);
        return null;
    }
}
