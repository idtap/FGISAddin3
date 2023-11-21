using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace FGISAddin3
{
    internal class MoiCadastreDockpane_ShowButton : Button
    {
        protected override void OnClick()
        {
            MoiCadastreDockpaneViewModel.Show();

        }
    }

    internal class MoiCadastreDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "FGISAddin3_MoiCadastreDockpane";

        protected MoiCadastreDockpaneViewModel()
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
