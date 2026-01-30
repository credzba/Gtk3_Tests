// SourceProxyHack.cs
using System;
using System.Reflection;
using System.Collections;
using Gtk;
using GLib;

public class SourceProxyHackTest
{
    public static void Main()
    {
        Console.WriteLine("=== SourceProxy Hack Test v1.2 ===\n");
        Gtk.Application.Init();
        
        Console.WriteLine("=== Testing SourceProxy Manipulation ===\n");
        
        // Get the Source type and the source_handlers field
        Type sourceType = typeof(GLib.Source);
        FieldInfo handlersField = sourceType.GetField("source_handlers", 
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        
        if (handlersField == null)
        {
            Console.WriteLine("ERROR: Could not find source_handlers field!");
            return;
        }
        
        Hashtable handlers = (Hashtable)handlersField.GetValue(null);
        Console.WriteLine($"Found source_handlers hashtable: {handlers != null}");
        Console.WriteLine($"Current handler count: {handlers.Count}");
        
        // Create an idle source using GLib.Idle.Add
        Console.WriteLine("\nAdding an idle source...");
        bool callbackRan = false;
        
        uint sourceId = GLib.Idle.Add(() => {
            Console.WriteLine("  Callback executing...");
            callbackRan = true;
            return false;  // Let it auto-remove
        });
        
        Console.WriteLine($"Created source ID: {sourceId}");
        Console.WriteLine($"Handler count after add: {handlers.Count}");
        
        // Try to find the SourceProxy for this ID
        if (handlers.ContainsKey(sourceId))
        {
            object proxy = handlers[sourceId];
            Console.WriteLine($"Found proxy object: {proxy.GetType().Name}");
            
            // Get the ID field
            Type proxyType = proxy.GetType();
            FieldInfo idField = proxyType.GetField("ID", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            
            if (idField != null)
            {
                uint currentId = (uint)idField.GetValue(proxy);
                Console.WriteLine($"Current ID value: {currentId}");
                
                // Process the event so the callback runs
                Console.WriteLine("\nProcessing GTK events...");
                while (Gtk.Application.EventsPending())
                    Gtk.Application.RunIteration();
                
                Console.WriteLine($"Callback ran: {callbackRan}");
                Console.WriteLine($"Handler count after callback: {handlers.Count}");
                Console.WriteLine($"Proxy still in hashtable: {handlers.ContainsKey(sourceId)}");
                
                // Try calling Remove again even though it's already gone
                Console.WriteLine("\nTrying to call Remove() again after callback...");
                bool removeResult = GLib.Source.Remove(sourceId);
                Console.WriteLine($"Remove result: {removeResult}");
                Console.WriteLine($"Handler count after Remove: {handlers.Count}");
                
                // Try setting ID to 0
                if (handlers.ContainsKey(sourceId))
                {
                    Console.WriteLine("\nTrying to set ID to 0...");
                    idField.SetValue(proxy, (uint)0);
                    uint newId = (uint)idField.GetValue(proxy);
                    Console.WriteLine($"ID after setting to 0: {newId}");
                }
            }
            else
            {
                Console.WriteLine("ERROR: Could not find ID field!");
            }
        }
        else
        {
            Console.WriteLine("ERROR: Source ID not found in handlers!");
        }
        
        Console.WriteLine("\nFinal handler count: {handlers.Count}");
    }
}