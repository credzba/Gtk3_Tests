// CustomMainLoop.cs - Version 6.0 - Custom main loop with wake mechanism
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Gtk;
using GLib;
using NLog;

/// <summary>
/// Custom GTK main loop with thread-safe invoke queue.
/// Runs its own main loop that processes both GTK events and queued actions.
/// Uses platform-specific wake mechanisms to avoid busy-waiting.
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
    private static CustomMainLoop? instance;
    private static readonly object instanceLock = new object();
    
    private readonly Queue<System.Action> invokeQueue;
    private readonly object queueLock;
    private volatile bool running;
    
    // Platform-specific wake mechanism
    private IWakeMechanism? wakeMechanism;
    
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
        
        // Detect platform and create appropriate wake mechanism
        if (Environment.OSVersion.Platform == PlatformID.Unix ||
            Environment.OSVersion.Platform == PlatformID.MacOSX)
        {
            wakeMechanism = new UnixPipeWake();
        }
        else
        {
            wakeMechanism = new WindowsEventWake();
        }
        
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
        
        bool shouldWake = false;
        
        lock (queueLock)
        {
            shouldWake = invokeQueue.Count == 0;
            invokeQueue.Enqueue(action);
            logger.Debug("ENQUEUE - Queue now has {0} items", invokeQueue.Count);
        }
        
        if (shouldWake)
        {
            wakeMechanism?.Wake();
        }
    }
    
    /// <summary>
    /// Run the main loop. This blocks until Quit() is called.
    /// Must be called from the UI thread.
    /// </summary>
    public void Run()
    {
        running = true;
        
        // Setup wake mechanism
        wakeMechanism?.Setup(ProcessInvokeQueue, () => running);
        
        logger.Info("Starting custom main loop");
        
        // Our own main loop
        while (running)
        {
            // Process GTK events (blocks, but wake mechanism can interrupt)
            bool hadEvents = GLib.MainContext.Iteration(true);
            
            // Process our invoke queue
            ProcessInvokeQueue();
        }
        
        logger.Info("Main loop exited");
        wakeMechanism?.Cleanup();
    }

    /// <summary>
    /// Signal the main loop to stop running.
    /// Thread-safe - can be called from any thread.
    /// </summary>
    public void Quit()
    {
        logger.Info("Quit requested");
        running = false;
        // Wake the main loop so it can exit
        wakeMechanism?.Wake();
    }
    
    /// <summary>
    /// Process all pending actions in the invoke queue
    /// </summary>
    private void ProcessInvokeQueue()
    {
        System.Action[]? actions = null;
        
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

/// <summary>
/// Interface for platform-specific wake mechanisms
/// </summary>
interface IWakeMechanism
{
    void Setup(System.Action processQueue, Func<bool> isRunning);
    void Wake();
    void Cleanup();
}

/// <summary>
/// Unix/Linux implementation using self-pipe pattern
/// </summary>
class UnixPipeWake : IWakeMechanism
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private int pipeFdRead = -1;
    private int pipeFdWrite = -1;
    private GLib.IOChannel? ioChannel;
    private uint ioWatchId;
    private System.Action? processQueue;
    private Func<bool>? isRunning;
    
    [DllImport("libc")]
    private static extern int pipe(int[] pipefd);
    
    [DllImport("libc")]
    private static extern IntPtr write(int fd, byte[] buf, UIntPtr count);
    
    [DllImport("libc")]
    private static extern IntPtr read(int fd, byte[] buf, UIntPtr count);
    
    [DllImport("libc")]
    private static extern int close(int fd);
    
    public void Setup(System.Action processQueue, Func<bool> isRunning)
    {
        this.processQueue = processQueue;
        this.isRunning = isRunning;
        
        // Create pipe
        int[] pipefd = new int[2];
        if (pipe(pipefd) != 0)
        {
            throw new Exception("Failed to create pipe for event loop");
        }
        
        pipeFdRead = pipefd[0];
        pipeFdWrite = pipefd[1];
        
        logger.Debug("Pipe created: read={0}, write={1}", pipeFdRead, pipeFdWrite);
        
        // Monitor pipe with IOChannel - this integrates with GLib main loop
        ioChannel = new GLib.IOChannel(pipeFdRead);
        ioWatchId = ioChannel.AddWatch((int)GLib.Priority.Default, GLib.IOCondition.In, OnPipeReadable);
        
        logger.Debug("IOChannel watch added with ID={0}", ioWatchId);
    }
    
    private bool OnPipeReadable(GLib.IOChannel channel, GLib.IOCondition condition)
    {
        logger.Debug("OnPipeReadable called");
        
        // Drain pipe
        byte[] buffer = new byte[256];
        while (true)
        {
            IntPtr result = read(pipeFdRead, buffer, (UIntPtr)buffer.Length);
            if (result.ToInt32() <= 0)
                break;
        }
        
        // Process queue (will also be called in main loop, but doing it here is more responsive)
        if (processQueue != null)
            processQueue();
        
        if (isRunning != null)
            return isRunning();
        
        return false;
    }
    
    public void Wake()
    {
        if (pipeFdWrite != -1)
        {
            byte[] wakeByte = new byte[1] { 1 };
            IntPtr result = write(pipeFdWrite, wakeByte, (UIntPtr)1);
            logger.Debug("Wake signal sent, result={0}", result.ToInt32());
        }
    }
    
    public void Cleanup()
    {
        if (ioWatchId != 0)
        {
            GLib.Source.Remove(ioWatchId);
            ioWatchId = 0;
        }
        
        if (ioChannel != null)
        {
            ioChannel.Shutdown(false);
            ioChannel = null;
        }
        
        if (pipeFdRead != -1)
        {
            close(pipeFdRead);
            pipeFdRead = -1;
        }
        
        if (pipeFdWrite != -1)
        {
            close(pipeFdWrite);
            pipeFdWrite = -1;
        }
        
        logger.Debug("Wake mechanism cleaned up");
    }
}

/// <summary>
/// Windows implementation using Win32 events and GLib.Timeout polling
/// </summary>
class WindowsEventWake : IWakeMechanism
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private ManualResetEvent? wakeEvent;
    private uint timeoutId;
    private System.Action? processQueue;
    private Func<bool>? isRunning;
    
    public void Setup(System.Action processQueue, Func<bool> isRunning)
    {
        this.processQueue = processQueue;
        this.isRunning = isRunning;
        
        wakeEvent = new ManualResetEvent(false);
        
        // Use GLib.Timeout to poll the event
        // Check every 10ms for wake signal
        timeoutId = GLib.Timeout.Add(10, OnTimeout);
        
        logger.Debug("Windows wake mechanism setup with 10ms polling");
    }
    
    private bool OnTimeout()
    {
        // Check if we were signaled
        if (wakeEvent?.WaitOne(0) == true)
        {
            wakeEvent?.Reset();
            logger.Debug("Wake event triggered");
            processQueue?.Invoke();
        }
        
        if (isRunning != null)
            return isRunning();
        
        return false;
    }
    
    public void Wake()
    {
        if (wakeEvent != null)
        {
            wakeEvent.Set();
            logger.Debug("Wake event set");
        }
    }
    
    public void Cleanup()
    {
        /*
        if (timeoutId != 0)
        {
            GLib.Source.Remove(timeoutId);
            timeoutId = 0;
        }
        */

        if (wakeEvent != null)
        {
            wakeEvent.Dispose();
            wakeEvent = null;
        }
        
        logger.Debug("Wake mechanism cleaned up");
    }
}