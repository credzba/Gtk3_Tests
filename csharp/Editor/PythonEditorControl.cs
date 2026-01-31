using System;
using Gtk;

namespace PythonEditor
{
    public class PythonEditorControl
    {
        private PythonEditorWindow window;
        
        public PythonEditorControl()
        {
            window = new PythonEditorWindow();
        }
        
        // Public API
        public void Show()
        {
            window.ShowAll();
        }
        
        public void SetText(string text)
        {
            window.SetEditorText(text);
        }
        
        public string GetText()
        {
            return window.GetEditorText();
        }
        
        public void LoadFile(string filename)
        {
            window.LoadFile(filename);
        }
        
        public void SaveFile(string filename)
        {
            window.SaveFile(filename);
        }
        
        public void Run()
        {
            CustomMainLoop.Instance.Init();
            window.ShowAll();
            CustomMainLoop.Instance.Run();
        }
        
        public Window Window => window;
    }
}