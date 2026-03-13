using Gtk; using UI.Settings;
namespace UI.Settings.Tabs.Filters
{
    public class TargettingSubTab : TabBase
    {
        public override string TabName   => "Targetting";
        protected override string UiFile  => "TargettingSubTab.ui";
        protected override void OnPopulate(Builder builder) { }
    }
}
