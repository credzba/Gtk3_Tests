using Gtk; using UI.Settings;
namespace UI.Settings.Tabs.Scripting
{
    public class CSharpSubTab : TabBase
    {
        public override string TabName   => "C#";
        protected override string UiFile  => "CSharpSubTab.ui";
        protected override void OnPopulate(Builder builder) { }
    }
}
