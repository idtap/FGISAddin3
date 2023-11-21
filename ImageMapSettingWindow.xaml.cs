
using System.Windows;
using System.Windows.Controls;
using ArcGIS.Desktop.Framework.Controls;


namespace FGISAddin3
{
    public partial class ImageMapSettingWindow : ProWindow
    {        
        public ImageMapSettingWindow()
        {
            InitializeComponent();
            dataGrid.ItemsSource = ImageServiceTool.imageServices;
            // 直接定義 ServiceType 可選項目
            cboServiceType.Items.Add("ImageServer");
            cboServiceType.Items.Add("MapServer");
            cboServiceType.Items.Add("WMS");

            // 載入初始資料
            LoadInitialData();
        }

        private void LoadInitialData()
        {
            // 載入 json 服務資料
            ImageServiceTool.LoadServiceJson();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ImageServiceModel newService = new ImageServiceModel { ServiceName = "New Name", FieldType = "N" };

            ImageServiceTool.imageServices.Add(newService);

            // scroll to new
            dataGrid.SelectedItem = newService;
            dataGrid.ScrollIntoView(newService);

            dataGrid.Items.Refresh();

            MessageBox.Show("暫時新增一筆空白資料，填入所需內容後，按下[修改]存檔","通知");
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem != null)
            {
                var result = MessageBox.Show("確定要刪除嗎?", "刪除確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    ImageServiceTool.imageServices.Remove((ImageServiceModel)dataGrid.SelectedItem);
                    // imageServices 存檔
                    ImageServiceTool.SaveServiceJson();
                }
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // 將目前輸入存回 imageServices
            if (dataGrid.SelectedItem is ImageServiceModel selectedService)
            {
                selectedService.ServiceName = txtServiceName.Text;
                selectedService.ServiceUrl = txtServiceUrl.Text;
                selectedService.ServiceType = cboServiceType.SelectedItem.ToString();
                selectedService.ProxyUrl = txtProxyUrl.Text;
                selectedService.Token = txtToken.Text;
                selectedService.QueryField = txtQueryField.Text;
                if( (bool)chkFieldType.IsChecked )
                    selectedService.FieldType = "N";
                else
                    selectedService.FieldType = "C";

                dataGrid.Items.Refresh();

                MessageBox.Show("存檔成功，按鍵繼續","通知");
            }

            // imageServices 存檔
            ImageServiceTool.SaveServiceJson();
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid.SelectedItem is ImageServiceModel selectedService)
            {
                txtServiceName.Text = selectedService.ServiceName;
                txtServiceUrl.Text = selectedService.ServiceUrl;
                cboServiceType.SelectedItem = selectedService.ServiceType;
                txtProxyUrl.Text = selectedService.ProxyUrl;
                txtToken.Text = selectedService.Token;
                txtQueryField.Text = selectedService.QueryField;
                if( selectedService.FieldType.Equals("C") )
                    chkFieldType.IsChecked = false;
                else
                    chkFieldType.IsChecked = true;
            }
        }
    }

}
