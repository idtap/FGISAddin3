using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using FGISAddin3.CoaCadastre;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace FGISAddin3
{
    public partial class CoaCadastreDockpaneView : UserControl
    {
        public delegate void SecSelectedHandler(object sender, SecEventArgs args);
        public event SecSelectedHandler OnSecSelected;

        public CoaCadastreDockpaneView()
        {
            InitializeComponent();
            InputTokenWindow.LoadCoaToken();
            // �Ұʮɥ��w������
            if (!DesignerProperties.GetIsInDesignMode(this))
                this.Loaded += CtrSec_LoadedAsync;
        }

        private async void CtrSec_LoadedAsync(object sender, RoutedEventArgs e)
        {
            if (cmbCounty.ItemsSource == null)
                cmbCounty.ItemsSource = await APISource.GetCounties();
        }

        // �����ﶵ�ܧ�
        private async void cmbCounty_SelectionChangedAsync(object sender, SelectionChangedEventArgs e)
        {
            cmbTown.ItemsSource = null;
            if (cmbCounty.SelectedItem == null) return;

            cmbTown.ItemsSource = await APISource.GetTowns(cmbCounty.SelectedValue.ToString());
            OnSecSelected?.Invoke(this, new SecEventArgs());
        }

        // �m��ﶵ�ܧ�
        private async void cmbTown_SelectionChangedAsync(object sender, SelectionChangedEventArgs e)
        {
            cmbSec.ItemsSource = null;
            if (cmbTown.SelectedItem == null) return;
            var feat = cmbTown.SelectedItem as Feature;
            var Secs = await QueuedTask.Run(async () =>
            {
                try
                {
                    var secs = await APISource.GetSecs(feat["CountyName"].ToString(),
                        feat["TownName"].ToString());
                    return secs.Select(x => new SecData()
                    {
                        OBJECTID = x.GetObjectID(),
                        SCNAME = x["SCNAME"].ToString()
                    }).ToArray();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    return null;
                }
            });
            if (Secs == null) return;
            ICollectionView view = CollectionViewSource.GetDefaultView(Secs);
            view.SortDescriptions.Add(new System.ComponentModel.SortDescription("SCNAME", ListSortDirection.Ascending));
            view.Filter = new Predicate<object>(SecFilter);
            cmbSec.ItemsSource = view;
            OnSecSelected?.Invoke(this, new SecEventArgs());
        }

        private bool SecFilter(object t)
        {
            if (cmbSec.Text == "") return true;
            try
            {
                return (t as SecData).SCNAME.Contains(cmbSec.Text);
            }
            catch (Exception)
            {
                return false;
            }
        }

        // �q�O�ﶵ�ܧ�
        private void cmbSec_SelectionChangedAsync(object sender, SelectionChangedEventArgs e)
        {
            if (OnSecSelected != null && e.AddedItems.Count > 0)
                OnSecSelected(this, new SecEventArgs()
                {
                    secAddress = new SecAddress()
                    {
                        County = cmbCounty.SelectedValue?.ToString(),
                        Town = cmbTown.SelectedValue?.ToString(),
                        SecName = cmbSec.SelectedValue?.ToString()
                    }
                });
        }

        public string SecAddress
        {
            get
            {
                return string.Format("{0}{1}{2}",
                    cmbCounty.SelectedValue?.ToString(),
                    cmbTown.SelectedValue?.ToString(),
                    cmbSec.SelectedValue?.ToString()
                    );
            }
        }

        // �q�O��r�ܧ�
        private void cmbSec_TextChanged(object sender, TextChangedEventArgs e)
        {
            (cmbSec.ItemsSource as ICollectionView)?.Refresh();
            cmbSec.IsDropDownOpen = true;
        }

        // �����w��
        private void btnLocateCounty_Click(object sender, RoutedEventArgs e)
        {
            if (cmbCounty.SelectedItem == null | MapView.Active == null) return;
            try
            {
                var feat = cmbCounty.SelectedItem as Feature;
                QueuedTask.Run(async () =>
                {
                    var res = await APISource.GetCounty(feat.GetObjectID());
                    MapView.Active.ZoomTo(res.GetShape());
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        // �m��ϩw��
        private void btnLocateTown_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTown.SelectedItem == null || MapView.Active == null) return;
            var feat = cmbTown.SelectedItem as Feature;
            QueuedTask.Run(async () =>
            {
                var res = await APISource.GetTown(feat.GetObjectID());
                MapView.Active.ZoomTo(res.GetShape());
            });
        }

        // �q�O�w��
        private void btnLocateSec_Click(object sender, RoutedEventArgs e)
        {
            if (cmbSec.SelectedItem == null || MapView.Active == null) return;
            var secData = cmbSec.SelectedItem as SecData;
            QueuedTask.Run(async () =>
            {
                var res = await APISource.GetSec(secData.OBJECTID);
                MapView.Active.ZoomTo(res.GetShape());
            });
        }

        // �a���ܧ�
        private void txtLandNo_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtAddress.Text = txtLandNo.Text.Trim().Length > 0 ?
                string.Format("{0}{1}{2}�a��", SecAddress,
                txtLandNo.Text, txtSubNo.Text.Trim().Length > 0 ?
                "-" + txtSubNo.Text : "") : SecAddress;
        }

        // �d�ߵ��G��������
        private void LstCadastre_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstCadastre.SelectedItem == null) return;
            (this.DataContext as CoaCadastreDockpaneViewModel).Locate();
        }

        // �d�߾�q�a�y
        private async void btnQueryBySec_Click(object sender, RoutedEventArgs e)
        {          
            if( SecAddress.Length<=0 )
            {
                MessageBox.Show("�|�������F��");
                return;
            }
            var data = await APISource.GetCadastres(SecAddress);           
            if (data == null || data.Length==0)
            {
                MessageBox.Show("�a�y�d�ߦ��A���L�^���Χ䤣�������");
                return;
            }
            // �u�������e���X�ǳƶ}�ҹϼh
            var sec_code = data[0].Return14SecNo.Substring(0, 6);
            MessageBox.Show("�t�αN�H�ϼh�覡�[�W�q��["+sec_code+"]����}�ҡA�����~��");

            var layerUrl = "https://coagis.colife.org.tw/arcgis/rest/services/CadastralMap/CadastralMap_Tiled_"+APISource._version+"/MapServer/1";
            Map map = MapView.Active.Map;
            var name = SecAddress;

            CIMInternetServerConnection serverConnection = null;
            await QueuedTask.Run(() =>
            {
                serverConnection = new CIMInternetServerConnection()
                {
                    Anonymous = true,
                    HideUserProperty = true,
                    URL = "https://coagis.colife.org.tw/arcgis/rest/services"                    
                    //User = "User_CadastralMap",
                    //Password = "User_CadastralMap2017coa"
                };
            });

            CIMAGSServiceConnection connection = null;
            await QueuedTask.Run(() =>
            {
                var param = new CIMStringMap { 
                    Key = "token", 
                    Value = InputTokenWindow.coaToken 
                };
                connection = new CIMAGSServiceConnection()
                {
                    ObjectName = "CadastralMap/CadastralMap_Tiled_" + APISource._version + "/MapServer/1",
                    ObjectType = "MapServer",
                    CustomParameters = [param],
                    URL = layerUrl,
                    ServerConnection = serverConnection
                };
            });

            var whereCause = "�q�� like '%" + sec_code + "%'";
            //var flyrCreatnParam = new FeatureLayerCreationParams(new Uri(layerUrl))
            FeatureLayerCreationParams flyrCreatnParam = null;
            await QueuedTask.Run(() =>
            {
                flyrCreatnParam = new FeatureLayerCreationParams(connection)
                {
                    Name = name,
                    IsVisible = true,
                    MinimumScale = 1000000,
                    MaximumScale = 100,
                    DefinitionQuery = new DefinitionQuery(
                        whereClause: whereCause, name: name)
                    //RendererDefinition = new SimpleRendererDefinition()
                    //{
                        //SymbolTemplate = SymbolFactory.Instance.ConstructPointSymbol(
                        //    CIMColor.CreateRGBColor(255, 0, 0), 8, SimpleMarkerStyle.Hexagon).MakeSymbolReference()                        
                    //}
                };
            });
            FeatureLayer featureLayer = null;
            var bo = true;
            await QueuedTask.Run(() =>
            {
                try
                {
                    featureLayer = LayerFactory.Instance.CreateLayer<FeatureLayer>(
                        flyrCreatnParam, map);                                                        
                }
                catch (Exception ex)
                {                    
                    MessageBox.Show("�[�J�a�y�A�ȹϼh���ѡA�i�� token ����","���~");
                    bo = false;                    
                }
            });
            if (bo)
            {
                var secData = cmbSec.SelectedItem as SecData;
                await QueuedTask.Run(async () =>
                {
                    var res = await APISource.GetSec(secData.OBJECTID);
                    MapView.Active.ZoomTo(res.GetShape());
                });
                MessageBox.Show("�����A�i��[Map]�U�d�ݦ��ϼh����", "�q��");
            }
        }

        // �ܧ� Token
        private void btnChangeToken_Click(object sender, RoutedEventArgs e)
        {
            InputTokenWindow dialog = new InputTokenWindow();
            bool? result = dialog.ShowDialog();
            //if (result == true) {}
        }
    }

    public class FeatureToValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var res = QueuedTask.Run(() =>
            {
                return (value as Feature)?[parameter.ToString()];
            });
            return res.Result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SecAddress
    {
        public string County { set; get; }
        public string Town { set; get; }
        public string SecdNo { set; get; }
        public string SecName { set; get; }
    }

    public class SecEventArgs : EventArgs
    {
        public SecAddress secAddress { set; get; }
    }


}
