
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
        // ���I����ϥ�
        private CIMPointSymbol markShowSymbol = null;
        private string  markShowLayerName = @"VertexTemp";
        private GraphicsLayerCreationParams markShowLayerCreationParams = null;
        // �ثe������I
        private CIMPointSymbol markSelectSymbol = null;
        private string  markSelectLayerName = @"VertexSelectTemp";
        private GraphicsLayerCreationParams markSelectLayerCreationParams = null;
        private InputSimulator _inputSimulator;

        // ���n�ե��u���l��
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

        // �����@�~������
        void AutoAreaWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            autoAreaWindow.Hide();            
            SimulateKeyPress();
        }

        // �� AutoAreaTool �u����J��(�Y Ribbon ���s���U)
        protected override Task OnToolActivateAsync(bool active)
        {
            if ( autoAreaWindow == null )
                CreateAutoAreaWindow();
            autoAreaWindow.Show();

            return base.OnToolActivateAsync(active);
        }

        // �ƹ��I�U��
        protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e)
        {
            // ���s���U�~�B�z
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                e.Handled = true;
        }

        private double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
        }

        // �ƹ����U������
        protected override Task HandleMouseDownAsync(MapViewMouseButtonEventArgs e)
        {
            // �@�~���������ɤ��B�z
            if ( autoAreaWindow != null && !autoAreaWindow.IsVisible) {
                return Task.FromResult(0);
            }

            // ���U�ƹ���m�ഫ�� Map Point �ϼx�A����B�z
            return QueuedTask.Run(() =>
            {
                var posPoint = MapView.Active.ClientToMap(e.ClientPoint);
                MapPoint mapPoint = MapPointBuilder.CreateMapPoint(posPoint.X, posPoint.Y, MapView.Active.Map.SpatialReference);
                var mapPoint_4326 = GeometryEngine.Instance.Project(mapPoint, SpatialReferences.WGS84) as MapPoint;
                string sb = mapPoint_4326.X.ToString() + "," + mapPoint_4326.Y.ToString();

                //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("�I�諸�a�Ϧ�m:"+sb);

                // �}�l�Φ��I�d�� polygon �Ψ䳻�I(���ثe�� button �Ҧ��өw)                
                if ( AutoAreaWindow.mouseMode == 1 )          // �� polygon
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

                            // �ЫتŶ��d�߱���
                            var spatialQuery = new SpatialQueryFilter
                            {
                                // �ϥ��I�X��@���d�߱���
                                FilterGeometry = mapPoint,
                                // ���w�Ŷ����Y�A�o�̨ϥΥ涰
                                SpatialRelationship = SpatialRelationship.Intersects
                            };

                            // �ϥΪŶ��d�߱���d��ϼx
                            var selection = featureLayer.Select(spatialQuery, SelectionCombinationMethod.New);
                            var featuresList = selection.GetObjectIDs()?.ToList();

                            // ���ϼx�B�z����
                            if (featuresList.Count > 0)
                            {
                                findbo = true;

                                // �M�����e�j�M
                                myGraphicLayer.RemoveElements(null);
                                AutoAreaWindow.nowVertexPoints.Clear();
                                AutoAreaWindow.nowSelectVertex = -1;

                                // �O�d�� layer id ���(����Φ� search ��s�ե��� polygon)
                                AutoAreaWindow.nowFeatureLayer = featureLayer;
                                AutoAreaWindow.nowFeatureID = selection.GetObjectIDs().FirstOrDefault();

                                //MessageBox.Show($"�襤���h��Ϊ� ID�G{AutoAreaWindow.nowFeatureID}");

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
                                            // �ϸ�y�Шt�n�ର�a�Ϯy��
                                            var polygon_map = GeometryEngine.Instance.Project(polygon, MapView.Active.Map.SpatialReference) as Polygon;
                                            // �@�֨��o����
                                            AutoAreaWindow.nowCentroid = GeometryEngine.Instance.Centroid(polygon_map);
                                            var elem = new CIMPointGraphic
                                            {
                                                Location = AutoAreaWindow.nowCentroid,
                                                Symbol = markShowSymbol.MakeSymbolReference()
                                            };
                                            myGraphicLayer.AddElement(elem);
                                            // �u�� polygon �̥~�h part
                                            var vertices = Utility.GetOutermostRings(polygon_map).Points;
                                            foreach (var vertex in vertices)
                                            {
                                                // �N���I�[�� vertexPoints�A�ǳƤ���P�_�P�ץ�
                                                AutoAreaWindow.nowVertexPoints.Add(vertex);
                                                var cimGraphicElement = new CIMPointGraphic
                                                {
                                                    Location = vertex,
                                                    Symbol = markShowSymbol.MakeSymbolReference()
                                                };
                                                myGraphicLayer.AddElement(cimGraphicElement);
                                            }
                                            // �p�⭱�n
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
                                //MessageBox.Show($"���I�ơG{mapPoints.Count}");
                            }
                        }
                        // �h�ӹϼh�ŦX�Ȩ��@��
                        if( findbo )
                            break;
                    }
                    if (!findbo)
                        MessageBox.Show("���I����� Polygon �ϼx","�q��");
                }
                else if ( AutoAreaWindow.mouseMode == 2 )          // �ﳻ�I
                {
                    var mySelectGraphicLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<GraphicsLayer>()
                                              .Where(e => e.Name == markSelectLayerName).FirstOrDefault();            
                    if (mySelectGraphicLayer == null)
                    {
                        mySelectGraphicLayer = LayerFactory.Instance.CreateLayer<GraphicsLayer>(
                            markSelectLayerCreationParams, MapView.Active.Map);
                    }
                    mySelectGraphicLayer.RemoveElements(null);
                    // ���H�ثe�ƹ��I�U��m��̪񪺤@���I
                    int near_index = -1;
                    double near_distance = 999;
                    for( var i=0; i<AutoAreaWindow.nowVertexPoints.Count; i++)
                    {
                        var xx = AutoAreaWindow.nowVertexPoints[i].X;
                        var yy = AutoAreaWindow.nowVertexPoints[i].Y;
                        var distance = Distance(mapPoint.X,mapPoint.Y,xx,yy);
                        // 5 ���ؤ����~�ŦX
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
                        //MessageBox.Show("���I��쳻�I�A�i���ۨϥέץ��\��", "���I�I��");
                        // �N��������I�H�ϰv�[��a��
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
                        MessageBox.Show("���I�������I�A�����A��", "���I�I��");
                    }
                }
            });
        }


    }
}
