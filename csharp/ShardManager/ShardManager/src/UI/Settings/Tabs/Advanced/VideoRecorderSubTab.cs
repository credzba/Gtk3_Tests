// VideoRecorderSubTab.cs  — stub, UI TBD
using Gtk;
using UI.Settings;

namespace UI.Settings.Tabs.Advanced
{
    public class VideoRecorderSubTab : TabBase
    {
        public override string TabName   => "Video Recorder";
        protected override string UiFile  => "VideoRecorderSubTab.ui";
        protected override void OnPopulate(Builder builder) { }
    }
}
