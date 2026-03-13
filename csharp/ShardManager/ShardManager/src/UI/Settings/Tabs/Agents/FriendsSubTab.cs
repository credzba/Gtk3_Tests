using Gtk; using UI.Settings;
namespace UI.Settings.Tabs.Agents
{
    public class FriendsSubTab : TabBase
    {
        public override string TabName  => "Friends";
        protected override string UiFile => "FriendsSubTab.ui";
        protected override void OnPopulate(Builder builder) { }
    }
}
