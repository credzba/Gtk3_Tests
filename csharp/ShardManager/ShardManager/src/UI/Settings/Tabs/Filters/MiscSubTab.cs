using Gtk; using UI.Settings;
namespace UI.Settings.Tabs.Filters
{
    public class MiscSubTab : TabBase
    {
        public override string TabName   => "Misc";
        protected override string UiFile  => "MiscSubTab.ui";
        protected override void OnPopulate(Builder builder) { }
    }
}
