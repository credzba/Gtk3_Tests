using Gtk; using UI.Settings;
namespace UI.Settings.Tabs.Agents
{
    public class ScavengerSubTab : TabBase
    {
        public override string TabName  => "Scavenger";
        protected override string UiFile => "ScavengerSubTab.ui";
        protected override void OnPopulate(Builder builder) { }
    }
}
