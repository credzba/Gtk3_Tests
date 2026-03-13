using Gtk; using UI.Settings;
namespace UI.Settings.Tabs.Agents
{
    public class DressArmSubTab : TabBase
    {
        public override string TabName  => "Dress / Arm";
        protected override string UiFile => "DressArmSubTab.ui";
        protected override void OnPopulate(Builder builder) { }
    }
}
