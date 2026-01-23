// MainWindow.cs
using System;
using Gtk;


public class MainWindow : GtkWindowBase
{
    private Fixed scrollArea;
    private Label scrollLabel;
    private Button exitButton;
    private int xOffset = 0;

    public MainWindow()
        : base("MainWindow.ui", "GtkHelloScroll.MainWindow.ui")
    {
        // Constructor does not need anything else — base handles UI loading
    }

    protected override void Setup(Builder builder)
    {
        var quitMenuItem = (MenuItem)builder.GetObject("quit_menu_item");
        if (quitMenuItem != null)
            quitMenuItem.Activated += delegate { Gtk.Application.Quit(); };

        scrollArea = (Fixed)builder.GetObject("scroll_area");
        scrollLabel = (Label)builder.GetObject("scroll_label");
        exitButton = (Button)builder.GetObject("exit_button");

        window.Destroyed += delegate { Gtk.Application.Quit(); };
        exitButton.Clicked += delegate { Gtk.Application.Quit(); };

        GLib.Timeout.Add(50, new GLib.TimeoutHandler(() =>
        {
            xOffset += 5;
            if (xOffset > 600)
                xOffset = -200;
            scrollArea.Move(scrollLabel, xOffset, 5);
            return true;
        }));
    }
}
