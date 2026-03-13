using Gtk; using UI.Settings;
namespace UI.Settings.Tabs.Agents
{
    public class RestockSubTab : TabBase
    {
        public override string TabName  => "Restock";
        protected override string UiFile => "RestockSubTab.ui";
        protected override void OnPopulate(Builder builder) { }
    }
}
