using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace FGISAddin3
{
    internal class ImageMapQuery_ShowButton : Button
    {
        protected override void OnClick()
        {
            ImageMapQueryDockpaneViewModel.Show();
        }
    }

    internal class ImageMapQueryDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "FGISAddin3_ImageMapQueryDockpane";

        protected ImageMapQueryDockpaneViewModel()
        {
        }

        protected override Task InitializeAsync() {
            return base.InitializeAsync();
        }        
        
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;
            pane.Activate();
        }

    }

}
