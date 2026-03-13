using Gtk; using UI.Settings;
namespace UI.Settings.Tabs.Agents
{
    public class OrganizerSubTab : TabBase
    {
        public override string TabName  => "Organizer";
        protected override string UiFile => "OrganizerSubTab.ui";
        protected override void OnPopulate(Builder builder) { }
    }
}
