// DpsMeterSubTab.cs  — stub, UI TBD
using Gtk;
using UI.Settings;

namespace UI.Settings.Tabs.Advanced
{
    public class DpsMeterSubTab : TabBase
    {
        public override string TabName   => "DPS Meter";
        protected override string UiFile  => "DpsMeterSubTab.ui";
        protected override void OnPopulate(Builder builder) { }
    }
}
