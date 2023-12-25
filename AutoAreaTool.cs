
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessageBox = System.Windows.MessageBox;
using WindowsInput;

namespace FGISAddin3
{
    internal class AutoAreaTool : MapTool
    {
        private AutoAreaWindow autoAreaWindow = null;
        // 頂點選取使用
        private CIMPointSymbol markShowSymbol = null;
        private string  markShowLayerName = @"VertexTemp";
        private GraphicsLayerCreationParams markShowLayerCreationParams = null;
        // 目前選取頂點
        private CIMPointSymbol markSelectSymbol = null;
        private string  markSelectLayerName = @"VertexSelectTemp";
        private GraphicsLayerCreationParams markSelectLayerCreationParams = null;
        private InputSimulator _inputSimulator;

        // 面積校正工具初始化
        public AutoAreaTool() : base()
        {            
            if (autoAreaWindow != null)
            {
                autoAreaWindow.Show();
                return;
            }
            CreateAutoAreaWindow();
            autoAreaWindow.Show();
            if( markShowSymbol == null )
            {
                QueuedTask.Run(() =>
                {
                    markShowSymbol = SymbolFactory.Instance.ConstructPointSymbol(
                        CIMColor.CreateRGBColor(255, 0, 0), 5.0, 
                        SimpleMarkerStyle.X);
                });
            }
            if (markShowLayerCreationParams == null)
            {
                markShowLayerCreationParams = new GraphicsLayerCreationParams
                {
                    Name = markShowLayerName,
                    MapMemberPosition = MapMemberPosition.AutoArrange
                };
            }
            if( markSelectSymbol == null )
            {
                QueuedTask.Run(() =>
                {
                    markSelectSymbol = SymbolFactory.Instance.ConstructPointSymbol(
                        CIMColor.CreateRGBColor(255,0,0), 10.0, 
                        SimpleMarkerStyle.Star);
                });
            }
            if (markSelectLayerCreationParams == null)
            {
                markSelectLayerCreationParams = new GraphicsLayerCreationParams
                {
                    Name = markSelectLayerName,
                    MapMemberPosition = MapMemberPosition.AutoArrange
                };
            }
            
        }

        private void SimulateKeyPress()
        {            
            _inputSimulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.ESCAPE);
        }

        public void CreateAutoAreaWindow()
        {
            _inputSimulator = new InputSimulator();
            autoAreaWindow = new AutoAreaWindow();
            autoAreaWindow.Owner = FrameworkApplication.Current.MainWindow;
            autoAreaWindow.Closing += AutoAreaWindow_Closing;
            autoAreaWindow.Closed += (o, e) => {
                //autoAreaWindow = null;                
            };
        }

        // 關閉作業視窗時
        void AutoAreaWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            autoAreaWindow.Hide();            
            SimulateKeyPress();
        }

        // 此 AutoAreaTool 工具載入時(即 Ribbon 按鈕按下)
        protected override Task OnToolActivateAsync(bool active)
        {
            if ( autoAreaWindow == null )
                CreateAutoAreaWindow();
            autoAreaWindow.Show();

            return base.OnToolActivateAsync(active);
        }

        // 滑鼠點下時
        protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e)
        {
            // 左鈕按下才處理
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                e.Handled = true;
        }

        private double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
        }

        // 滑鼠按下完成時
        protected override Task HandleMouseDownAsync(MapViewMouseButtonEventArgs e)
        {
            // 作業視窗關閉時不處理
            if ( autoAreaWindow != null && !autoAreaWindow.IsVisible) {
                return Task.FromResult(0);
            }

            // 取下滑鼠位置轉換成 Map Point 圖徵，後續處理
            return QueuedTask.Run(() =>
            {
                var posPoint = MapView.Active.ClientToMap(e.ClientPoint);
                MapPoint mapPoint = MapPointBuilder.CreateMapPoint(posPoint.X, posPoint.Y, MapView.Active.Map.SpatialReference);
                var mapPoint_4326 = GeometryEngine.Instance.Project(mapPoint, SpatialReferences.WGS84) as MapPoint;
                string sb = mapPoint_4326.X.ToString() + "," + mapPoint_4326.Y.ToString();

                //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("點選的地圖位置:"+sb);

                // 開始用此點查找 polygon 或其頂點(視目前的 button 模式而定)                
                if ( AutoAreaWindow.mouseMode == 1 )          // 找 polygon
                {
                    var myGraphicLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<GraphicsLayer>()
                                            .Where(e => e.Name == markShowLayerName).FirstOrDefault();            
                    if (myGraphicLayer == null)
                    {
                        myGraphicLayer = LayerFactory.Instance.CreateLayer<GraphicsLayer>(
                            markShowLayerCreationParams, MapView.Active.Map);
                    }

                    var mapView = MapView.Active;                    
                    var mapLayers = mapView.Map.Layers;
                    var findbo = false;
                    foreach (var mapLayer in mapLayers)
                    {
                        if (mapLayer is FeatureLayer featureLayer)
                        {
                            var featureClass = featureLayer.GetTable() as FeatureClass;

                            // 創建空間查詢條件
                            var spatialQuery = new SpatialQueryFilter
                            {
                                // 使用點幾何作為查詢條件
                                FilterGeometry = mapPoint,
                                // 指定空間關係，這裡使用交集
                                SpatialRelationship = SpatialRelationship.Intersects
                            };

                            // 使用空間查詢條件查找圖徵
                            var selection = featureLayer.Select(spatialQuery, SelectionCombinationMethod.New);
                            var featuresList = selection.GetObjectIDs()?.ToList();

                            // 找到圖徵處理後續
                            if (featuresList.Count > 0)
                            {
                                findbo = true;

                                // 清除之前搜尋
                                myGraphicLayer.RemoveElements(null);
                                AutoAreaWindow.nowVertexPoints.Clear();
                                AutoAreaWindow.nowSelectVertex = -1;

                                // 保留此 layer id 後用(之後用此 search 更新校正後 polygon)
                                AutoAreaWindow.nowFeatureLayer = featureLayer;
                                AutoAreaWindow.nowFeatureID = selection.GetObjectIDs().FirstOrDefault();

                                //MessageBox.Show($"選中的多邊形的 ID：{AutoAreaWindow.nowFeatureID}");

                                List<long> oids = new List<long>();
                                oids.Add(AutoAreaWindow.nowFeatureID);
                                QueryFilter queryFilter = new QueryFilter()
                                {
                                    ObjectIDs = oids
                                };

                                using (RowCursor rowCursor = featureClass.Search(queryFilter))
                                {
                                    while (rowCursor.MoveNext())
                                    {
                                        using (Feature feature = rowCursor.Current as Feature)
                                        {
                                            // use feature
                                            var polygon = feature.GetShape() as ArcGIS.Core.Geometry.Polygon;                                            
                                            // 圖資座標系要轉為地圖座標
                                            var polygon_map = GeometryEngine.Instance.Project(polygon, MapView.Active.Map.SpatialReference) as Polygon;
                                            // 一併取得重心
                                            AutoAreaWindow.nowCentroid = GeometryEngine.Instance.Centroid(polygon_map);
                                            var elem = new CIMPointGraphic
                                            {
                                                Location = AutoAreaWindow.nowCentroid,
                                                Symbol = markShowSymbol.MakeSymbolReference()
                                            };
                                            myGraphicLayer.AddElement(elem);
                                            // 只取 polygon 最外層 part
                                            var vertices = Utility.GetOutermostRings(polygon_map).Points;
                                            foreach (var vertex in vertices)
                                            {
                                                // 將頂點加到 vertexPoints，準備之後判斷與修正
                                                AutoAreaWindow.nowVertexPoints.Add(vertex);
                                                var cimGraphicElement = new CIMPointGraphic
                                                {
                                                    Location = vertex,
                                                    Symbol = markShowSymbol.MakeSymbolReference()
                                                };
                                                myGraphicLayer.AddElement(cimGraphicElement);
                                            }
                                            // 計算面積
                                            //var area = GeometryEngine.Instance.Area(polygon);
                                            var area = AutoAreaWindow.CalculatePolygonArea(
                                                           AutoAreaWindow.nowVertexPoints);
                                            autoAreaWindow.SetNowArea(area);
                                            autoAreaWindow.SetAreaFrom(area-area*0.03);
                                            autoAreaWindow.SetAreaEnd(area+area*0.03);
                                            autoAreaWindow.SetAdjustArea(area);
                                        }
                                    }
                                    QueuedTask.Run(() => {
                                         myGraphicLayer.UnSelectElements();
                                    });
                                }
                                //MessageBox.Show($"頂點數：{mapPoints.Count}");
                            }
                        }
                        // 多個圖層符合僅取一個
                        if( findbo )
                            break;
                    }
                    if (!findbo)
                        MessageBox.Show("未點到任何 Polygon 圖徵","通知");
                }
                else if ( AutoAreaWindow.mouseMode == 2 )          // 選頂點
                {
                    var mySelectGraphicLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<GraphicsLayer>()
                                              .Where(e => e.Name == markSelectLayerName).FirstOrDefault();            
                    if (mySelectGraphicLayer == null)
                    {
                        mySelectGraphicLayer = LayerFactory.Instance.CreateLayer<GraphicsLayer>(
                            markSelectLayerCreationParams, MapView.Active.Map);
                    }
                    mySelectGraphicLayer.RemoveElements(null);
                    // 先以目前滑鼠點下位置找最近的一頂點
                    int near_index = -1;
                    double near_distance = 999;
                    for( var i=0; i<AutoAreaWindow.nowVertexPoints.Count; i++)
                    {
                        var xx = AutoAreaWindow.nowVertexPoints[i].X;
                        var yy = AutoAreaWindow.nowVertexPoints[i].Y;
                        var distance = Distance(mapPoint.X,mapPoint.Y,xx,yy);
                        // 5 公尺內的才符合
                        if( distance<=5 )
                        {
                            if( distance<near_distance )
                            {
                                near_index = i;
                                near_distance = distance;
                            }    
                        }
                    }
                    if( near_index != -1 ) {
                        AutoAreaWindow.nowSelectVertex = near_index;
                        //MessageBox.Show("有點選到頂點，可接著使用修正功能", "頂點點選");
                        // 將選取的頂點以圖釘加到地圖
                        var vertex = AutoAreaWindow.nowVertexPoints[near_index];
                        var cimGraphicElement = new CIMPointGraphic
                        {
                            Location = vertex,
                            Symbol = markSelectSymbol.MakeSymbolReference()
                        };
                        mySelectGraphicLayer.AddElement(cimGraphicElement);
                        QueuedTask.Run( () => {
                            mySelectGraphicLayer.UnSelectElements();
                        });
                    }
                    else {
                        MessageBox.Show("未點選到任何頂點，按鍵後再試", "頂點點選");
                    }
                }
            });
        }


    }
}
