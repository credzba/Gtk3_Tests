// HelpStatusSubTab.cs  — stub, UI TBD
using Gtk;
using UI.Settings;

namespace UI.Settings.Tabs.Technical
{
    public class HelpStatusSubTab : TabBase
    {
        public override string TabName   => "Help / Status";
        protected override string UiFile  => "HelpStatusSubTab.ui";

        protected override void OnPopulate(Builder builder)
        {
            // Populated when UI design is provided.
        }
    }
}
