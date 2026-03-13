using Gtk; using UI.Settings;
namespace UI.Settings.Tabs.Agents
{
    public class VendorSellSubTab : TabBase
    {
        public override string TabName  => "Vendor Sell";
        protected override string UiFile => "VendorSellSubTab.ui";
        protected override void OnPopulate(Builder builder) { }
    }
}
