
using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

namespace FGISAddin3
{
    public partial class MoiCadastreDockpaneView : UserControl
    {
        private const string  apiRoot_NLSC = "https://api.nlsc.gov.tw/";
        public ObservableCollection<MoiCadastreItem> lstMoiItems = new ObservableCollection<MoiCadastreItem>();
        private List<IDisposable> _graphic;
        private CIMTextSymbol _TextSymbol = null;
        private CIMPolygonSymbol _PolygonSymbol = null;

        public MoiCadastreDockpaneView()
        {
            InitializeComponent();
            lstMoiCadastre.ItemsSource = lstMoiItems;
            _graphic = Utility.Graphics("MoiQuery");
            QueuedTask.Run(() =>
            {
                CIMStroke outline = SymbolFactory.Instance.ConstructStroke(CIMColor.CreateRGBColor(255, 0, 0), 2.0, SimpleLineStyle.Solid);
                List<CIMSymbolLayer> symbolLayers = new List<CIMSymbolLayer>();
                symbolLayers.Add(outline);
                _PolygonSymbol = new CIMPolygonSymbol()
                {
                    SymbolLayers = symbolLayers.ToArray()
                };
                _TextSymbol = SymbolFactory.Instance.ConstructTextSymbol(ColorFactory.Instance.BlackRGB, 8.5, "Corbel", "Regular");               
                _TextSymbol.HorizontalAlignment = ArcGIS.Core.CIM.HorizontalAlignment.Center;
                _TextSymbol.VerticalAlignment = ArcGIS.Core.CIM.VerticalAlignment.Bottom;
            });
            if (!DesignerProperties.GetIsInDesignMode(this))
                this.Loaded += Pane_LoadedAsync;
        }

        private void Pane_LoadedAsync(object sender, RoutedEventArgs e)
        {
            if (cmbCounty.ItemsSource != null)
                return;

            // 準備由 nlsc 服務讀取 xml 後 parser
            //var xmlResult = Utility.myHttpGET(apiRoot_NLSC+"other/ListCounty",Encoding.UTF8);
            //if (xmlResult.Substring(0, 5).Equals("error"))
            //{
            //    MessageBox.Show("國土測繪中心(nlsc)伺服器無回應，將無法使用地政司地籍查詢", "系統通知");
            //    return;
            //}
            //XDocument doc = XDocument.Parse(xmlResult);
            //List<CountyItem> items = doc.Root.Elements("countyItem").Select(e => new CountyItem
            //{
            //    countycode = (string)e.Element("countycode"),
            //    countyname = (string)e.Element("countyname")
            //}).ToList();

            // 因國土測繪 ListCounty 常掛(可能大家程式初始都連此)，此處寫死
            List<CountyItem> items = new List<CountyItem>
            {
                new CountyItem { countycode = "A", countyname = "臺北市" },
                new CountyItem { countycode = "B", countyname = "臺中市" },
                new CountyItem { countycode = "C", countyname = "基隆市" },
                new CountyItem { countycode = "D", countyname = "臺南市" },
                new CountyItem { countycode = "E", countyname = "高雄市" },
                new CountyItem { countycode = "F", countyname = "新北市" },
                new CountyItem { countycode = "G", countyname = "宜蘭縣" },
                new CountyItem { countycode = "H", countyname = "桃園市" },
                new CountyItem { countycode = "I", countyname = "嘉義市" },
                new CountyItem { countycode = "J", countyname = "新竹縣" },
                new CountyItem { countycode = "K", countyname = "苗栗縣" },
                new CountyItem { countycode = "M", countyname = "南投縣" },
                new CountyItem { countycode = "N", countyname = "彰化縣" },
                new CountyItem { countycode = "O", countyname = "新竹市" },
                new CountyItem { countycode = "P", countyname = "雲林縣" },
                new CountyItem { countycode = "Q", countyname = "嘉義縣" },
                new CountyItem { countycode = "T", countyname = "屏東縣" },
                new CountyItem { countycode = "U", countyname = "花蓮縣" },
                new CountyItem { countycode = "V", countyname = "臺東縣" },
                new CountyItem { countycode = "W", countyname = "金門縣" },
                new CountyItem { countycode = "X", countyname = "澎湖縣" },
                new CountyItem { countycode = "Z", countyname = "連江縣" }
            };

            cmbCounty.ItemsSource = items;
        }

        private void cmbCounty_SelectionChangedAsync(object sender, SelectionChangedEventArgs e)
        {
            cmbTown.ItemsSource = null;
            if (cmbCounty.SelectedItem == null) 
                return;
            var xmlResult = Utility.myHttpGET(apiRoot_NLSC + "other/ListTown/" + cmbCounty.SelectedValue.ToString(),
                                              Encoding.UTF8);
            if (xmlResult.Substring(0, 5).Equals("error"))
            {
                MessageBox.Show("國土測繪中心(nlsc)伺服器無回應", "系統通知");
                return;
            }
            XDocument doc = XDocument.Parse(xmlResult);
            List<TownItem> items = doc.Root.Elements("townItem").Select(e => new TownItem
            {
                towncode = (string)e.Element("towncode"),
                townname = (string)e.Element("townname")
            }).ToList();

            cmbTown.ItemsSource = items;
        }

        private void cmbTown_SelectionChangedAsync(object sender, SelectionChangedEventArgs e)
        {
            cmbSec.ItemsSource = null;
            if (cmbTown.SelectedItem == null)
                return;
            var xmlResult = Utility.myHttpGET(apiRoot_NLSC + "other/ListLandSection/" +
                cmbCounty.SelectedValue.ToString()+"/"+
                cmbTown.SelectedValue.ToString(), Encoding.UTF8);
            if (xmlResult.Substring(0, 5).Equals("error"))
            {
                MessageBox.Show("國土測繪中心(nlsc)伺服器無回應", "系統通知");
                return;
            }
            XDocument doc = XDocument.Parse(xmlResult);
            List<SectItem> items = doc.Root.Elements("sectItem").Select(e => new SectItem
            {
                sectcode = (string)e.Element("sectcode"),
                sectstr = (string)e.Element("sectstr")
            }).ToList();

            cmbSec.ItemsSource = items;
        }

        private void cmbSec_SelectionChangedAsync(object sender, SelectionChangedEventArgs e)
        {
            // 段小段挑選,暫不做任何事
        }                

        private void LstMoiCadastre_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 雙按查詢結果 ListBox 項目時要移動地圖至該處
            if (lstMoiCadastre.SelectedItem == null) return;
            // 取下此項 posStr, 繪製此筆
            renderQueryResult(lstMoiCadastre.SelectedValue.ToString(),
                              ((MoiCadastreItem)lstMoiCadastre.SelectedItem).showStr);
        }

        // 繪製由字串式的 x1,y1;x2,y2;... 組成的 polygon 圖資
        private void renderQueryResult( string ringStr, string displayStr )
        {
            //MessageBox.Show(ringStr);

            try
            {
                QueuedTask.Run(() =>
                {
                    // 拆解此 ringStr 繪製及定位(中心點)
                    var pts = ringStr.Split(new string[] { ";" }, StringSplitOptions.None).Select(c => c.Split(new char[] { ',' }));
                    // 地圖 point
                    var mapPts = pts.Select(c => MapPointBuilder.CreateMapPoint(
                                                    double.Parse(c.First()),
                                                    double.Parse(c[1]),
                                                    SpatialReferences.WGS84));   // 地政司用經緯度
                    // 由 mapPts 產生此 polygon
                    var polygon = PolygonBuilder.CreatePolygon(mapPts);
                    // 用重心當中心點
                    var pt = GeometryEngine.Instance.Centroid(polygon);
                    // 繪製
                    var txt = new CIMTextGraphic()
                    {
                        Symbol = _TextSymbol.MakeSymbolReference(),
                        Text = displayStr,
                        Shape = pt
                    };
                    _graphic.Add(MapView.Active.AddOverlay(polygon, _PolygonSymbol.MakeSymbolReference()));
                    _graphic.Add(MapView.Active.AddOverlay(txt));
                    MapView.Active.PanTo(pt);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(),"錯誤通知");
            }
        }
        

        private void txtLandNo_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 檢查輸入是否符合 00010000,1-1,1 
        }

        // 查詢
        private void MoiQueryButton_Click(object sender, RoutedEventArgs e)
        {
            // 此為透過環保署轉呼叫 nlsc MAP_0001 api                                             
            var url = "https://gis.epa.gov.tw/EPAWebGIS/Map/GetCadasMap/";
            url += "?city=" + cmbCounty.SelectedValue.ToString() + 
                   "&sect=" + cmbSec.SelectedValue.ToString() + 
                   "&sectno=" + txtLandNo.Text.Trim();
            MessageBox.Show("按鍵後請等待至結果出現!!","系統通知");
            var xmlResult = Utility.myHttpGET(url, Encoding.UTF8);
            if (xmlResult.Substring(0, 5).Equals("error"))
            {
                MessageBox.Show("國土測繪中心(nlsc)伺服器無回應", "系統通知");
                return;
            }
            // 回傳的資料為 gml 格式,要自行 parser
            var err = xmlResult.Contains("<error>");
            if (err)
            {
                MessageBox.Show("查無此地號", "系統通知");
                return;
            }
            var a = xmlResult.IndexOf("<gml:featureMember>");
            var a1 = xmlResult.IndexOf("<gml:coordinates>", a);
            var a2 = xmlResult.IndexOf("</gml:coordinates>", a);
            string posStr = "";
            if (a1 != -1 && a2 != -1)
            {
                var astr = xmlResult.Substring(a1 + 17, a2-(a1+17));
                // 切字串, 組合 [x,y]
                var astrArr = astr.Split(' ');
                for( var ii=0;ii<astrArr.Length;ii++)
                {
                    if (ii != 0)
                        posStr = posStr + ";";
                    posStr = posStr + astrArr[ii];
                }
            }
            var showStr = ((CountyItem)cmbCounty.SelectedItem).countyname +
                          ((TownItem)cmbTown.SelectedItem).townname +
                          ((SectItem)cmbSec.SelectedItem).sectstr +
                          txtLandNo.Text.Trim();
            MoiCadastreItem moiItem = new MoiCadastreItem {
                showStr = showStr,
                posStr = posStr
            };
            // 將其加到 lstMoiItems
            lstMoiItems.Add(moiItem);
            // 定位到此
            renderQueryResult(posStr,showStr);
                              

        }

        // 清除查詢結果
        private void MoiLstClearButton_Click(object sender, RoutedEventArgs e)
        {
            _graphic.ForEach(x => x.Dispose());
            _graphic.Clear();
            lstMoiItems.Clear();
        }
    }

    public class CountyItem
    {
        public string countycode { get; set; }
        public string countyname { get; set; }
    }

    public class TownItem
    {
        public string towncode { get; set; }
        public string townname { get; set; }
    }

    public class SectItem
    {
        public string sectcode { get; set; }
        public string sectstr { get; set; }
    }

    public class MoiCadastreItem
    {
        public string showStr { get; set; }
        public string posStr { get; set; }
    }
}
