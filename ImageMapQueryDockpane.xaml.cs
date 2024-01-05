
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Core.Internal.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FGISAddin3
{
    public partial class ImageMapQueryDockpaneView : UserControl
    {
        private static ServiceCompositeSubLayer wmsLayers = null;
        private static string prevWmsLayerName = "";
        private static string imageLayerName = "";
        private static ArcGIS.Core.Geometry.SpatialReference spatialImageList = null;
        public ObservableCollection<ImageListItem> lstImageItems = new ObservableCollection<ImageListItem>();
        private List<IDisposable> _graphic = null;
        private CIMTextSymbol _TextSymbol = null;
        private CIMPolygonSymbol _PolygonSymbol = null;

        public ImageMapQueryDockpaneView()
        {
            InitializeComponent();
            cmbService.ItemsSource = ImageServiceTool.imageServices;
            lstImageQuery.ItemsSource = lstImageItems;
            _graphic = Utility.Graphics("ImageQueryList");
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
            // 載入初始資料
            LoadInitialData();
        }

        private void LoadInitialData()
        {
            ImageServiceTool.LoadServiceJson();
        }

        // 查詢前先清除之前查詢
        private async void clearPrevQuery()
        {
            await QueuedTask.Run(() =>
            {
                Map map = MapView.Active.Map;
                // 清除圖層，不移除以便比對
                //if (!imageLayerName.Equals(""))
                //{
                //    Layer layer = map.FindLayers(imageLayerName).FirstOrDefault();
                //    if( layer != null )
                //        map.RemoveLayer(layer);
                //}
                // 清除 list 
                if (_graphic != null)
                {
                    _graphic.ForEach(x => x.Dispose());
                    _graphic.Clear();
                }                
            });
            imageLayerName = "";
            lstImageItems.Clear();
            wmsLayers = null;
            prevWmsLayerName = "";
        }

        private async void ImageQueryButton_Click(object sender, RoutedEventArgs e)
        {
            // 取下輸入參數            
            ImageServiceModel serviceItem = null;
            if (cmbService.SelectedItem is ImageServiceModel selectedService)
                serviceItem = ImageServiceTool.getItemByServiceName(selectedService.ServiceName);
            if (serviceItem == null)
            {
                MessageBox.Show("尚未選取服務", "錯誤");
                return;
            }
            else
                MessageBox.Show("開始查詢，按鍵後請等待查詢結果出現", "通知");

            // 依不同的服務型態處理
            var imageType = serviceItem.ServiceType.ToString();
            if( imageType.Equals("ImageServer") )
            {
                // 查詢前先清除之前查詢
                clearPrevQuery();

                // 此種服務影像圖層直接開啟
                var imageryLayerUrl = serviceItem.ServiceUrl.ToString();
                Layer layer = null;
                await QueuedTask.Run(() =>
                {
                    Map map = MapView.Active.Map;
                    try
                    {
                        layer = LayerFactory.Instance.CreateLayer(new Uri(imageryLayerUrl), map);
                        spatialImageList = layer.GetSpatialReference();
                        imageLayerName = layer.Name;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("設定有誤或目前無法連上主機，稍後再試", "通知");
                    }
                });

                // 再開始查詢
                if (layer != null)
                {
                    var quote = "";
                    if (serviceItem.FieldType.ToString().Equals("C"))
                        quote = "'";

                    var listResult = ImageQuery_ImageServer(imageryLayerUrl + "/query", quote);

                    // 將資料加到 listbox, 準備點選定位
                    foreach (myFeature feature in listResult)
                    {
                        var showStr = "OBJECTID:" + feature.attributes.OBJECTID.ToString();
                        var posStr = "";

                        foreach (var pos in feature.geometry.rings[0])
                        {
                            if (!posStr.Equals(""))
                                posStr += ";";
                            posStr += pos[0].ToString() + "," + pos[1].ToString();
                        }

                        ImageListItem lstItem = new ImageListItem
                        {
                            showStr = showStr,
                            posStr = posStr
                        };

                        lstImageItems.Add(lstItem);
                    }
                }
            }
            else if( imageType.Equals("MapServer") )
            {
                // 查詢前先清除之前查詢
                clearPrevQuery();

                // 此種服務影像圖層亦是直接開啟
                var imageryLayerUrl = serviceItem.ServiceUrl.ToString();
                Layer layer = null;
                await QueuedTask.Run(() =>
                {
                    Map map = MapView.Active.Map;
                    try
                    {
                        layer = LayerFactory.Instance.CreateLayer(new Uri(imageryLayerUrl), map);
                        spatialImageList = layer.GetSpatialReference();
                        imageLayerName = layer.Name;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("設定有誤或目前無法連上主機，稍後再試", "通知");
                    }
                });

                // 再開始查詢
                if (layer != null)
                {
                    var quote = "";
                    if (serviceItem.FieldType.ToString().Equals("C"))
                        quote = "'";

                    // 無設定查詢欄位則使用 ObjectID
                    var queryField = serviceItem.QueryField.ToString().Trim();
                    if( queryField.Equals("") )
                    {
                        queryField = "OBJECTID";
                        quote = "";
                    }
                        
                    var listResult = await ImageQuery_MapServer(queryField,
                        imageryLayerUrl, quote);

                    // 將資料加到 listbox, 準備點選定位
                    foreach (Feature feature in listResult)
                    {
                        // 組合條列名
                        var showStr = "";
                        ArcGIS.Core.Geometry.Polygon polygon = null;
                        await QueuedTask.Run(() =>
                        {
                            showStr = "OBJECTID:" + feature.GetOriginalValue(0).ToString() +
                                      "," + serviceItem.QueryField.ToString() + ":" + feature.GetOriginalValue(1).ToString();
                            polygon = feature.GetShape() as ArcGIS.Core.Geometry.Polygon;
                        });
                        // 組合點位
                        var posStr = "";
                        // 圖資座標系要轉為地圖座標
                        var polygon_map = GeometryEngine.Instance.Project(polygon, MapView.Active.Map.SpatialReference) as ArcGIS.Core.Geometry.Polygon;
                        var polygon_outer = Utility.GetOutermostRings(polygon_map);
                        if (polygon_outer != null)
                        {
                            var vertices = polygon_outer.Points;
                            foreach (var pos in vertices)
                            {
                                if (!posStr.Equals(""))
                                    posStr += ";";
                                posStr += pos.X.ToString() + "," + pos.Y.ToString();
                            }
                        }
                        ImageListItem lstItem = new ImageListItem
                        {
                            showStr = showStr,
                            posStr = posStr
                        };

                        lstImageItems.Add(lstItem);
                    }
                }
            }
            else if( imageType.Equals("WMS") )
            {
                // 查詢前先清除之前查詢
                clearPrevQuery();
                
                var imageryLayerUrl = serviceItem.ServiceUrl.ToString();
                var serverConnection = new CIMInternetServerConnection 
                { 
                    URL = imageryLayerUrl
                };
                var connection = new CIMWMSServiceConnection 
                { 
                    ServerConnection = serverConnection
                };

                var layerParams = new LayerCreationParams(connection);
                layerParams.MapMemberPosition = MapMemberPosition.AddToTop;
                WMSLayer layer = null;
                Map map = MapView.Active.Map;
                await QueuedTask.Run(() =>
                {
                    try
                    {
                        layer = LayerFactory.Instance.CreateLayer<WMSLayer>(layerParams, map);
                        if (layer == null)
                        {
                            wmsLayers = null;
                            MessageBox.Show("設定有誤或目前無法連上主機，稍後再試", "通知");
                        }
                        else
                        {
                            layer.SetVisibility(true);
                            spatialImageList = layer.GetSpatialReference();
                            imageLayerName = layer.Name;
                            // 取下所有子圖層
                            wmsLayers = layer.Layers[0] as ServiceCompositeSubLayer;
                            // 單一子層再展開
                            while (wmsLayers.Layers.Count == 1) wmsLayers = wmsLayers.Layers[0] as ServiceCompositeSubLayer;
                            // 對所有子圖層關閉顯示
                            for (var ii = 0; ii < wmsLayers.Layers.Count; ii++)
                                wmsLayers.Layers[ii].SetVisibility(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        wmsLayers = null;
                        MessageBox.Show("設定有誤或目前無法連上主機，稍後再試", "通知");
                    }
                });
                // 將各筆加到 ListBox
                if (wmsLayers != null)
                {
                    var filterFrom = txtFilterFrom.Text.Trim();
                    var filterEnd = txtFilterEnd.Text.Trim();
                    for (var ii = 0; ii < wmsLayers.Layers.Count; ii++)
                    {
                        var showStr = wmsLayers.Layers[ii].Name.ToString();
                        var bo = true;
                        if (!filterFrom.Equals(""))
                            if (string.Compare(showStr, filterFrom) < 0)
                                bo = false;
                        if (bo && !filterEnd.Equals(""))
                            if (string.Compare(showStr, filterEnd) > 0)
                                bo = false;
                        if (bo)
                        {
                            var posStr = "";
                            ImageListItem lstItem = new ImageListItem
                            {
                                showStr = showStr,
                                posStr = posStr
                            };
                            lstImageItems.Add(lstItem);
                        }
                    }
                }
            }
            else if( imageType.Equals("WMTS") )
            {
                // 查詢前先清除之前查詢
                clearPrevQuery();
                
                var imageryLayerUrl = serviceItem.ServiceUrl.ToString();
                var serverConnection = new CIMInternetServerConnection 
                { 
                    URL = imageryLayerUrl
                };
                var connection = new CIMWMTSServiceConnection 
                { 
                    ServerConnection = serverConnection
                    //LayerName = "MTN"
                };

                var layerParams = new LayerCreationParams(connection);
                layerParams.MapMemberPosition = MapMemberPosition.AddToTop;
                Layer layer = null;
                Map map = MapView.Active.Map;
                await QueuedTask.Run(() =>
                {
                    try
                    {
                        layer = LayerFactory.Instance.CreateLayer<TiledServiceLayer>(layerParams, map);
                        if (layer == null)
                        {
                            MessageBox.Show("設定有誤或目前無法連上主機，稍後再試", "通知");
                        }
                        else
                        {
                            MapView.Active.ZoomTo(layer);
                            MessageBox.Show("影像圖磚式圖層僅能開啟無法查詢明細，按鍵後繼續","通知");                            
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("設定有誤或目前無法連上主機，稍後再試", "通知");
                    }
                });
            }
            else if( imageType.Equals("afasiUAV") )
            {
                // 查詢前先清除之前查詢
                clearPrevQuery();
                
                var layerUrl = serviceItem.ServiceUrl.ToString();

                var jsonResult = Utility.myHttpPOST(layerUrl,"",Encoding.UTF8);

            }
            else if( imageType.Equals("nlscUAS") )
            {
                // 查詢前先清除之前查詢
                clearPrevQuery();
                
                var layerUrl = serviceItem.ServiceUrl.ToString();

                var jsonResult = Utility.myHttpPOST(layerUrl,"",Encoding.UTF8);
            }
        }

        private List<myFeature> ImageQuery_ImageServer(string queryUrl, string quote)
        {
            // 組合過濾條件
            var filterStr = " 1=1 ";
            var filterFrom = txtFilterFrom.Text.Trim();
            var filterEnd = txtFilterEnd.Text.Trim();
            var queryField = "OBJECTID";
            if( queryField.Equals("") || (filterFrom.Equals("") && filterEnd.Equals("")))
                MessageBox.Show("無設定條件\n可能無法列出所有項目，按鍵繼續","通知");
            else {
                if( !filterFrom.Equals("") )
                    filterStr = filterStr + " AND "+ queryField + ">=" + quote + filterFrom + quote;
                if( !filterEnd.Equals("") )
                    filterStr = filterStr + " AND "+ queryField + "<=" + quote + filterEnd + quote;
            }
            var whereStr = Uri.EscapeDataString(filterStr);
            // 自行使用 httpclient 搜尋及 parser
            var param = string.Format("f=pjson&outFields=*&where={0}",whereStr);
            var jsonResult = Utility.myHttpGET(queryUrl+"?"+param,Encoding.UTF8);
            myFeatureCollection featureCollection = JsonConvert.DeserializeObject<myFeatureCollection>(jsonResult);
            List<myFeature> myFeatures = featureCollection.features;
            
            return myFeatures;
        }        

        private async Task<List<Feature>> ImageQuery_MapServer(string queryField,string queryUrl, string quote)
        {
            // 組合過濾條件
            var filterStr = " 1=1 ";
            var filterFrom = txtFilterFrom.Text.Trim();
            var filterEnd = txtFilterEnd.Text.Trim();
            if( queryField.Equals("") || (filterFrom.Equals("") && filterEnd.Equals("")))
                MessageBox.Show("此服務無設定條件\n系統將提供有限項目供查詢，按鍵繼續", "通知");
            else {
                if( !filterFrom.Equals("") )
                    filterStr = filterStr + " AND "+ queryField + ">=" + quote + filterFrom + quote;
                if( !filterEnd.Equals("") )
                    filterStr = filterStr + " AND "+ queryField + "<=" + quote + filterEnd + quote;
            }            

            IEnumerable<Feature> features = await Utility.GetFeatures(queryUrl, "0", filterStr);

            List<Feature> result = new List<Feature>();
            foreach (Feature feature in features)
            {
                result.Add(feature);
            }

            return result;
        }

        private void LstImageQuery_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstImageQuery.SelectedItem == null) return;
                renderQueryItem(lstImageQuery.SelectedValue.ToString(),
                              ((ImageListItem)lstImageQuery.SelectedItem).showStr,
                              spatialImageList);

        }

        private async void renderQueryItem(string ringStr, string displayStr, 
                                     ArcGIS.Core.Geometry.SpatialReference spatial)
        {
            // WMS 要用名稱查
            if (wmsLayers != null )
            {
                // 之前的要先 disable 
                if( !prevWmsLayerName.Equals("") )
                {
                    for (var ii = 0; ii < wmsLayers.Layers.Count; ii++)
                    {
                        var name = wmsLayers.Layers[ii].Name.ToString();
                        if (name.Equals(prevWmsLayerName))
                        {
                            await QueuedTask.Run(() =>
                            {
                                wmsLayers.Layers[ii].SetVisibility(false);
                            });
                            prevWmsLayerName = "";
                            break;
                        }
                    }
                }
                // 打開新的
                for (var ii = 0; ii < wmsLayers.Layers.Count; ii++)
                {
                    var name = wmsLayers.Layers[ii].Name.ToString();
                    if( name.Equals(displayStr) )
                    {
                        await QueuedTask.Run(() =>
                        {
                            wmsLayers.Layers[ii].SetVisibility(true);
                        });
                        prevWmsLayerName = wmsLayers.Layers[ii].Name.ToString();
                        break;
                    }
                }

                return;
            }

            // 其他 
            if (ringStr.Equals("")) 
                return;

            try
            {
                await QueuedTask.Run(() =>
                {
                    // 拆解此 ringStr 繪製及定位(中心點)
                    var pts = ringStr.Split(new string[] { ";" }, StringSplitOptions.None).Select(c => c.Split(new char[] { ',' }));
                    // 地圖 point
                    var mapPts = pts.Select(c => MapPointBuilder.CreateMapPoint(
                                                    double.Parse(c.First()),
                                                    double.Parse(c[1]),
                                                    spatial));                       
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
                MessageBox.Show(ex.ToString(), "錯誤通知");
            }
        }


        private void ImageQueryClearButton_Click(object sender, RoutedEventArgs e)
        {
            if( _graphic != null ) 
            {
                _graphic.ForEach(x => x.Dispose());
                _graphic.Clear();
            }
        }

        private void cmbService_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ImageServiceModel serviceItem = null;
            if (cmbService.SelectedItem is ImageServiceModel selectedService)
                serviceItem = ImageServiceTool.getItemByServiceName(selectedService.ServiceName);
            if (serviceItem != null)
            {
                var imageType = serviceItem.ServiceType.ToString();
                if( imageType.Equals("ImageServer") )
                    lblFilterField.Content = "[OBJECTID]";
                else if( imageType.Equals("WMS") )
                    lblFilterField.Content = "[圖層名]";
                else if( imageType.Equals("MapServer") )
                {
                    var queryField = serviceItem.QueryField.ToString().Trim();
                    if( queryField.Equals("") )
                        lblFilterField.Content = "[OBJECTID]";
                    else
                        lblFilterField.Content = "["+queryField+"]";
                }
                else
                    lblFilterField.Content = "[OBJECTID]";
            }

        }
    }

    public class myFeature
    {
        public Attributes attributes { get; set; }
        public Geometry geometry { get; set; }

        public class Attributes
        {
            public int OBJECTID { get; set; }
        }

        public class Geometry
        {
            public List<List<List<double>>> rings { get; set; }
        }
    }

    public class myFeatureCollection
    {
        public List<myFeature> features { get; set; }
    }

    public class ImageListItem
    {
        public string showStr { get; set; }
        public string posStr { get; set; }
    }
}
