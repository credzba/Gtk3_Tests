using Gtk; using UI.Settings;
namespace UI.Settings.Tabs.Scripting
{
    public class UosSubTab : TabBase
    {
        public override string TabName   => "UOS";
        protected override string UiFile  => "UosSubTab.ui";
        protected override void OnPopulate(Builder builder) { }
    }
}
