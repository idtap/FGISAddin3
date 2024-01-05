
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
            // ���J��l���
            LoadInitialData();
        }

        private void LoadInitialData()
        {
            ImageServiceTool.LoadServiceJson();
        }

        // �d�߫e���M�����e�d��
        private async void clearPrevQuery()
        {
            await QueuedTask.Run(() =>
            {
                Map map = MapView.Active.Map;
                // �M���ϼh�A�������H�K���
                //if (!imageLayerName.Equals(""))
                //{
                //    Layer layer = map.FindLayers(imageLayerName).FirstOrDefault();
                //    if( layer != null )
                //        map.RemoveLayer(layer);
                //}
                // �M�� list 
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
            // ���U��J�Ѽ�            
            ImageServiceModel serviceItem = null;
            if (cmbService.SelectedItem is ImageServiceModel selectedService)
                serviceItem = ImageServiceTool.getItemByServiceName(selectedService.ServiceName);
            if (serviceItem == null)
            {
                MessageBox.Show("�|������A��", "���~");
                return;
            }
            else
                MessageBox.Show("�}�l�d�ߡA�����е��ݬd�ߵ��G�X�{", "�q��");

            // �̤��P���A�ȫ��A�B�z
            var imageType = serviceItem.ServiceType.ToString();
            if( imageType.Equals("ImageServer") )
            {
                // �d�߫e���M�����e�d��
                clearPrevQuery();

                // ���تA�ȼv���ϼh�����}��
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
                        MessageBox.Show("�]�w���~�Υثe�L�k�s�W�D���A�y��A��", "�q��");
                    }
                });

                // �A�}�l�d��
                if (layer != null)
                {
                    var quote = "";
                    if (serviceItem.FieldType.ToString().Equals("C"))
                        quote = "'";

                    var listResult = ImageQuery_ImageServer(imageryLayerUrl + "/query", quote);

                    // �N��ƥ[�� listbox, �ǳ��I��w��
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
                // �d�߫e���M�����e�d��
                clearPrevQuery();

                // ���تA�ȼv���ϼh��O�����}��
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
                        MessageBox.Show("�]�w���~�Υثe�L�k�s�W�D���A�y��A��", "�q��");
                    }
                });

                // �A�}�l�d��
                if (layer != null)
                {
                    var quote = "";
                    if (serviceItem.FieldType.ToString().Equals("C"))
                        quote = "'";

                    // �L�]�w�d�����h�ϥ� ObjectID
                    var queryField = serviceItem.QueryField.ToString().Trim();
                    if( queryField.Equals("") )
                    {
                        queryField = "OBJECTID";
                        quote = "";
                    }
                        
                    var listResult = await ImageQuery_MapServer(queryField,
                        imageryLayerUrl, quote);

                    // �N��ƥ[�� listbox, �ǳ��I��w��
                    foreach (Feature feature in listResult)
                    {
                        // �զX���C�W
                        var showStr = "";
                        ArcGIS.Core.Geometry.Polygon polygon = null;
                        await QueuedTask.Run(() =>
                        {
                            showStr = "OBJECTID:" + feature.GetOriginalValue(0).ToString() +
                                      "," + serviceItem.QueryField.ToString() + ":" + feature.GetOriginalValue(1).ToString();
                            polygon = feature.GetShape() as ArcGIS.Core.Geometry.Polygon;
                        });
                        // �զX�I��
                        var posStr = "";
                        // �ϸ�y�Шt�n�ର�a�Ϯy��
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
                // �d�߫e���M�����e�d��
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
                            MessageBox.Show("�]�w���~�Υثe�L�k�s�W�D���A�y��A��", "�q��");
                        }
                        else
                        {
                            layer.SetVisibility(true);
                            spatialImageList = layer.GetSpatialReference();
                            imageLayerName = layer.Name;
                            // ���U�Ҧ��l�ϼh
                            wmsLayers = layer.Layers[0] as ServiceCompositeSubLayer;
                            // ��@�l�h�A�i�}
                            while (wmsLayers.Layers.Count == 1) wmsLayers = wmsLayers.Layers[0] as ServiceCompositeSubLayer;
                            // ��Ҧ��l�ϼh�������
                            for (var ii = 0; ii < wmsLayers.Layers.Count; ii++)
                                wmsLayers.Layers[ii].SetVisibility(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        wmsLayers = null;
                        MessageBox.Show("�]�w���~�Υثe�L�k�s�W�D���A�y��A��", "�q��");
                    }
                });
                // �N�U���[�� ListBox
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
                // �d�߫e���M�����e�d��
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
                            MessageBox.Show("�]�w���~�Υثe�L�k�s�W�D���A�y��A��", "�q��");
                        }
                        else
                        {
                            MapView.Active.ZoomTo(layer);
                            MessageBox.Show("�v���Ͽj���ϼh�ȯ�}�ҵL�k�d�ߩ��ӡA������~��","�q��");                            
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("�]�w���~�Υثe�L�k�s�W�D���A�y��A��", "�q��");
                    }
                });
            }
            else if( imageType.Equals("afasiUAV") )
            {
                // �d�߫e���M�����e�d��
                clearPrevQuery();
                
                var layerUrl = serviceItem.ServiceUrl.ToString();

                var jsonResult = Utility.myHttpPOST(layerUrl,"",Encoding.UTF8);

            }
            else if( imageType.Equals("nlscUAS") )
            {
                // �d�߫e���M�����e�d��
                clearPrevQuery();
                
                var layerUrl = serviceItem.ServiceUrl.ToString();

                var jsonResult = Utility.myHttpPOST(layerUrl,"",Encoding.UTF8);
            }
        }

        private List<myFeature> ImageQuery_ImageServer(string queryUrl, string quote)
        {
            // �զX�L�o����
            var filterStr = " 1=1 ";
            var filterFrom = txtFilterFrom.Text.Trim();
            var filterEnd = txtFilterEnd.Text.Trim();
            var queryField = "OBJECTID";
            if( queryField.Equals("") || (filterFrom.Equals("") && filterEnd.Equals("")))
                MessageBox.Show("�L�]�w����\n�i��L�k�C�X�Ҧ����ءA�����~��","�q��");
            else {
                if( !filterFrom.Equals("") )
                    filterStr = filterStr + " AND "+ queryField + ">=" + quote + filterFrom + quote;
                if( !filterEnd.Equals("") )
                    filterStr = filterStr + " AND "+ queryField + "<=" + quote + filterEnd + quote;
            }
            var whereStr = Uri.EscapeDataString(filterStr);
            // �ۦ�ϥ� httpclient �j�M�� parser
            var param = string.Format("f=pjson&outFields=*&where={0}",whereStr);
            var jsonResult = Utility.myHttpGET(queryUrl+"?"+param,Encoding.UTF8);
            myFeatureCollection featureCollection = JsonConvert.DeserializeObject<myFeatureCollection>(jsonResult);
            List<myFeature> myFeatures = featureCollection.features;
            
            return myFeatures;
        }        

        private async Task<List<Feature>> ImageQuery_MapServer(string queryField,string queryUrl, string quote)
        {
            // �զX�L�o����
            var filterStr = " 1=1 ";
            var filterFrom = txtFilterFrom.Text.Trim();
            var filterEnd = txtFilterEnd.Text.Trim();
            if( queryField.Equals("") || (filterFrom.Equals("") && filterEnd.Equals("")))
                MessageBox.Show("���A�ȵL�]�w����\n�t�αN���Ѧ������بѬd�ߡA�����~��", "�q��");
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
            // WMS �n�ΦW�٬d
            if (wmsLayers != null )
            {
                // ���e���n�� disable 
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
                // ���}�s��
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

            // ��L 
            if (ringStr.Equals("")) 
                return;

            try
            {
                await QueuedTask.Run(() =>
                {
                    // ��Ѧ� ringStr ø�s�Ωw��(�����I)
                    var pts = ringStr.Split(new string[] { ";" }, StringSplitOptions.None).Select(c => c.Split(new char[] { ',' }));
                    // �a�� point
                    var mapPts = pts.Select(c => MapPointBuilder.CreateMapPoint(
                                                    double.Parse(c.First()),
                                                    double.Parse(c[1]),
                                                    spatial));                       
                    // �� mapPts ���ͦ� polygon
                    var polygon = PolygonBuilder.CreatePolygon(mapPts);
                    // �έ��߷����I
                    var pt = GeometryEngine.Instance.Centroid(polygon);
                    // ø�s
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
                MessageBox.Show(ex.ToString(), "���~�q��");
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
                    lblFilterField.Content = "[�ϼh�W]";
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
