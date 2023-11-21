using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace FGISAddin3
{
    internal class ImageMapSetting_ShowButton : Button
    {
        private ImageMapSettingWindow imageMapSettingWindow = null;

        protected override void OnClick()
        {
            if (imageMapSettingWindow != null)
            {
                imageMapSettingWindow.Show();
                return;
            }
            CreateImageMapSettingWindow();
            imageMapSettingWindow.Show();
        }

        public void CreateImageMapSettingWindow()
        {
            imageMapSettingWindow = new ImageMapSettingWindow();
            imageMapSettingWindow.Owner = FrameworkApplication.Current.MainWindow;
            imageMapSettingWindow.Closing += ImageMapSettingWindow_Closing;
            imageMapSettingWindow.Closed += (o, e) => { imageMapSettingWindow = null; };
        }

        void ImageMapSettingWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            imageMapSettingWindow.Hide();
        }

        
    }
}
