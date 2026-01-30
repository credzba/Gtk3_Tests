// CustomMainLoop.cs - Version 5.0 - Simplified with custom main loop
using System;
using System.Collections.Generic;
using Gtk;
using GLib;
using NLog;

/// <summary>
/// Custom GTK main loop with thread-safe invoke queue.
/// Runs its own main loop that processes both GTK events and queued actions.
/// 
/// Usage - Replace in your code:
///    Gtk.Application.Init() → CustomMainLoop.Instance.Init()
///    Gtk.Application.Invoke → CustomMainLoop.Instance.Invoke
///    Gtk.Application.Run() → CustomMainLoop.Instance.Run()
///    Gtk.Application.Quit() → CustomMainLoop.Instance.Quit()
/// </summary>
public class CustomMainLoop
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private static CustomMainLoop instance;
    private static readonly object instanceLock = new object();
    
    private readonly Queue<System.Action> invokeQueue;
    private readonly object queueLock;
    private volatile bool running;
    
    /// <summary>
    /// Get the singleton instance of CustomMainLoop
    /// </summary>
    public static CustomMainLoop Instance
    {
        get
        {
            if (instance == null)
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new CustomMainLoop();
                    }
                }
            }
            return instance;
        }
    }
    
    private CustomMainLoop()
    {
        invokeQueue = new Queue<System.Action>();
        queueLock = new object();
        running = false;
        
        logger.Debug("Instance created");
    }
    
    private bool isInitialized = false;
    private readonly object initLock = new object();
    
    /// <summary>
    /// Initialize GTK Application.
    /// Call this once at application startup, before creating any windows.
    /// This replaces Gtk.Application.Init()
    /// </summary>
    public void Init()
    {
        if (!isInitialized)
        {
            lock (initLock)
            {
                if (!isInitialized)
                {
                    logger.Info("Initializing GTK Application");
                    Gtk.Application.Init();
                    isInitialized = true;
                    logger.Info("Initialization complete");
                }
            }
        }
    }
    
    /// <summary>
    /// Queue an action to be executed on the UI thread.
    /// Thread-safe - can be called from any thread.
    /// </summary>
    /// <param name="action">Action to execute on UI thread</param>
    public void Invoke(System.Action action)
    {
        if (action == null)
            throw new ArgumentNullException("action");
        
        lock (queueLock)
        {
            invokeQueue.Enqueue(action);
            logger.Debug("ENQUEUE - Queue now has {0} items", invokeQueue.Count);
        }
    }
    
    /// <summary>
    /// Run the main loop. This blocks until Quit() is called.
    /// Must be called from the UI thread.
    /// </summary>
    public void Run()
    {
        running = true;
            
        logger.Info("Starting custom main loop");
        
        // Our own main loop
        while (running)
        {
            // Process GTK events (may block briefly)
            bool hadEvents = GLib.MainContext.Iteration(false); // false = don't block
            
            // Process our invoke queue
            ProcessInvokeQueue();
            
            // Small sleep to avoid busy-waiting if no events
            if (!hadEvents && PendingCount == 0)
            {
                System.Threading.Thread.Sleep(1);
            }
        }
        
        logger.Info("Main loop exited");
    }

    /// <summary>
    /// Signal the main loop to stop running.
    /// Thread-safe - can be called from any thread.
    /// </summary>
    public void Quit()
    {
        logger.Info("Quit requested");
        running = false;
        // Don't call Gtk.Application.Quit() since we're not using Gtk.Application.Run()
    }
    
    /// <summary>
    /// Process all pending actions in the invoke queue
    /// </summary>
    private void ProcessInvokeQueue()
    {
        System.Action[] actions = null;
        
        lock (queueLock)
        {
            if (invokeQueue.Count > 0)
            {
                logger.Debug("DEQUEUE - Processing {0} queued actions", invokeQueue.Count);
                actions = invokeQueue.ToArray();
                invokeQueue.Clear();
            }
        }
        
        if (actions != null)
        {
            for (int i = 0; i < actions.Length; i++)
            {
                logger.Debug("Executing action {0}/{1}", i+1, actions.Length);
                try
                {
                    actions[i]();
                    logger.Debug("Action {0}/{1} completed", i+1, actions.Length);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error in action {0}/{1}", i+1, actions.Length);
                }
            }
        }
    }
    
    public bool IsRunning { get { return running; } }
    
    public int PendingCount
    {
        get
        {
            lock (queueLock)
            {
                return invokeQueue.Count;
            }
        }
    }
}