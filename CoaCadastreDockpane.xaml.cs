using ArcGIS.Core.Data;
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
            // 啟動時先預載縣市
            if (!DesignerProperties.GetIsInDesignMode(this))
                this.Loaded += CtrSec_LoadedAsync;
        }

        private async void CtrSec_LoadedAsync(object sender, RoutedEventArgs e)
        {
            if (cmbCounty.ItemsSource == null)
                cmbCounty.ItemsSource = await APISource.GetCounties();
        }

        // 縣市選項變更
        private async void cmbCounty_SelectionChangedAsync(object sender, SelectionChangedEventArgs e)
        {
            cmbTown.ItemsSource = null;
            if (cmbCounty.SelectedItem == null) return;

            cmbTown.ItemsSource = await APISource.GetTowns(cmbCounty.SelectedValue.ToString());
            OnSecSelected?.Invoke(this, new SecEventArgs());
        }

        // 鄉鎮選項變更
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

        // 段別選項變更
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

        // 段別文字變更
        private void cmbSec_TextChanged(object sender, TextChangedEventArgs e)
        {
            (cmbSec.ItemsSource as ICollectionView)?.Refresh();
            cmbSec.IsDropDownOpen = true;
        }

        // 縣市定位
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

        // 鄉鎮區定位
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

        // 段別定位
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

        // 地號變更
        private void txtLandNo_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtAddress.Text = txtLandNo.Text.Trim().Length > 0 ?
                string.Format("{0}{1}{2}地號", SecAddress,
                txtLandNo.Text, txtSubNo.Text.Trim().Length > 0 ?
                "-" + txtSubNo.Text : "") : SecAddress;
        }

        // 查詢結果項目雙按
        private void LstCadastre_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstCadastre.SelectedItem == null) return;
            (this.DataContext as CoaCadastreDockpaneViewModel).Locate();
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
