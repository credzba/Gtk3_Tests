// SpellGridSubTab.cs  — stub, UI TBD
using Gtk;
using UI.Settings;

namespace UI.Settings.Tabs.Toolbars
{
    public class SpellGridSubTab : TabBase
    {
        public override string TabName   => "Spell Grid";
        protected override string UiFile  => "SpellGridSubTab.ui";
        protected override void OnPopulate(Builder builder) { }
    }
}
