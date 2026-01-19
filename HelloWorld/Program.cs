using System;

public class Program
{
    static void Main(string[] args)
    {
        // Create and run the main window
        var mainWindow = new MainWindow();
        mainWindow.Run();

        // Program returns here after window closes
        Console.WriteLine("Main window closed.");
    }
}
