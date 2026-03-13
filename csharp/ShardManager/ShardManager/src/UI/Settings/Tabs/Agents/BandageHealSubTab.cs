using Gtk; using UI.Settings;
namespace UI.Settings.Tabs.Agents
{
    public class BandageHealSubTab : TabBase
    {
        public override string TabName  => "Bandage Heal";
        protected override string UiFile => "BandageHealSubTab.ui";
        protected override void OnPopulate(Builder builder) { }
    }
}
