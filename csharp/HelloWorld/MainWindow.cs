// MainWindow.cs
using System;
using Gtk;
using System.Threading;


public class MainWindow : GtkWindowBase
{
    private Fixed scrollArea;
    private Label scrollLabel;
    private Button exitButton;
    private int xOffset = 0;

    // Thread synchronization variables
    private Thread animationThread;
    private volatile bool keepRunning = true;
    private ManualResetEventSlim threadStarted = new ManualResetEventSlim();
    private ManualResetEventSlim threadStopped = new ManualResetEventSlim();

    public MainWindow()
        : base("MainWindow.ui", "GtkHelloScroll.MainWindow.ui")
    {
        // Constructor does not need anything else â€” base handles UI loading
    }

    protected override void Setup(Builder builder)
    {
        var quitMenuItem = (MenuItem)builder.GetObject("quit_menu_item");
        if (quitMenuItem != null)
            quitMenuItem.Activated += delegate { CustomMainLoop.Instance.Quit(); };

        scrollArea = (Fixed)builder.GetObject("scroll_area");
        scrollLabel = (Label)builder.GetObject("scroll_label");
        exitButton = (Button)builder.GetObject("exit_button");

        // Set initial position of the label
        scrollArea.Move(scrollLabel, xOffset, 5);
        Console.WriteLine($"Initial label position set to ({xOffset}, 5)");

        window.Destroyed += delegate
        {
            StopAnimationThread();
            CustomMainLoop.Instance.Quit();
        };

        exitButton.Clicked += delegate
        {
            StopAnimationThread();
            CustomMainLoop.Instance.Quit();
        };

        /*
        GLib.Timeout.Add(50, new GLib.TimeoutHandler(() =>
        {
            xOffset += 5;
            if (xOffset > 600)
                xOffset = -200;
            scrollArea.Move(scrollLabel, xOffset, 5);
            return true;
        }));
        */

        StartAnimationThread();
    }


    private void StartAnimationThread()
    {
        animationThread = new Thread(AnimationThreadProc);
        animationThread.IsBackground = true; // Make it a background thread
        animationThread.Start();

        // Wait for thread to start (optional but good practice)
        threadStarted.Wait(1000);
    }

    private void StopAnimationThread()
    {
        Console.WriteLine("Animation thread terminating");

        // Signal the thread to stop
        keepRunning = false;

        // Wait forever with periodic console messages
        if (animationThread != null && animationThread.IsAlive)
        {
            Console.WriteLine("Waiting for animation thread to terminate...");

            int secondsWaiting = 0;
            while (animationThread.IsAlive)
            {
                Thread.Sleep(1000); // Wait 1 second
                secondsWaiting++;
                Console.WriteLine($"Waiting for animation thread... ({secondsWaiting} seconds)");

                // Optional: Try interrupting the thread if it's stuck
                if (secondsWaiting > 5 && animationThread.IsAlive)
                {
                    Console.WriteLine("Thread seems stuck, attempting to interrupt...");
                    try
                    {
                        animationThread.Interrupt();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Interrupt failed: {ex.Message}");
                    }
                }

                // Break after a very long time as a safety measure
                if (secondsWaiting > 30)
                {
                    Console.WriteLine("Emergency break after 30 seconds of waiting!");
                    break;
                }
            }

            if (secondsWaiting <= 30)
                Console.WriteLine("Animation thread terminated gracefully");
        }
    }

    private void AnimationThreadProc()
    {
        threadStarted.Set();
        Console.WriteLine("Animation thread started");

        try
        {
            while (keepRunning)
            {
                // Update UI on the GTK main thread
                CustomMainLoop.Instance.Invoke(() =>
                {
                    if (keepRunning) // Check again in case we stopped while waiting
                    {
                        xOffset += 5;
                        if (xOffset > 600)
                            xOffset = -200;
                        
                        scrollArea.Move(scrollLabel, xOffset, 5);
                        scrollArea.QueueDraw(); // Force the Fixed container to redraw
                    }
                });

                // Sleep for 50ms
                Thread.Sleep(50);
            }
        }
        finally
        {
            Console.WriteLine("Animation thread stopped");
            threadStopped.Set();
        }
    }

}