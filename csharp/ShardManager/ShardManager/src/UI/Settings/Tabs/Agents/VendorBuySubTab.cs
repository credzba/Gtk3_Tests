using Gtk; using UI.Settings;
namespace UI.Settings.Tabs.Agents
{
    public class VendorBuySubTab : TabBase
    {
        public override string TabName  => "Vendor Buy";
        protected override string UiFile => "VendorBuySubTab.ui";
        protected override void OnPopulate(Builder builder) { }
    }
}
