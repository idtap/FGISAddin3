using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Segment = ArcGIS.Core.Geometry.Segment;

namespace FGISAddin3
{
    // ���n�ץ��\��
    public partial class AutoAreaWindow : ProWindow
    {        
        public static int mouseMode = 0;     // �ƹ��I��Ҧ� 0/���B�z�A1/����S�x�A2/������I
        // �ثe������S�x 
        public static FeatureLayer nowFeatureLayer = null;
        public static long nowFeatureID = -1;
        public static List<MapPoint> nowVertexPoints = new List<MapPoint>();
        public static MapPoint nowCentroid = null;
        // �ثe��������I
        public static int nowSelectVertex = -1;

        public AutoAreaWindow()
        {
            InitializeComponent();
        }
        
        public void  SetNowArea( double area )
        {
            this.Dispatcher.Invoke(() =>
            {
                nowArea.Text = Math.Round(area,1).ToString();
            });
        }

        public void  SetAdjustArea( double area )
        {
            this.Dispatcher.Invoke(() =>
            {
                adjustArea.Text = Math.Round(area,1).ToString();
            });
        }

        public void  SetAreaFrom( double area )
        {
            this.Dispatcher.Invoke(() =>
            {
                areaFrom.Text = Math.Round(area,1).ToString();
            });
        }

        public void  SetAreaEnd( double area )
        {
            this.Dispatcher.Invoke(() =>
            {
                areaEnd.Text = Math.Round(area,1).ToString();
            });
        }

        public static double CalculatePolygonArea_self(List<MapPoint> xyList)
        {
           double area = 0;
           int n = xyList.Count;

           for (int i = 0; i < n - 1; i++)
           {
               var point1 = xyList[i];
               var point2 = xyList[i + 1];
               area += (point1.X * point2.Y - point2.X * point1.Y);
           }

           area = Math.Abs(area) / 2.0;
           return area;
        }

        public static async Task<double> CalculatePolygonArea(List<MapPoint> mapPoints)
        {
            double area = 0;
            await QueuedTask.Run(() =>
            {
                PolygonBuilder polygonBuilder = new PolygonBuilder(mapPoints);
                Polygon polygon = polygonBuilder.ToGeometry();
                area = GeometryEngine.Instance.Area(polygon);
            });

            return area;
        }
        

        private void btnSelectPolygon_Click(object sender, RoutedEventArgs e)
        {
            mouseMode = 1;
            MessageBox.Show("�h����I��Ҧ��}�ҡA�б��ۥH�ƹ��I��n�ե����h���", "�S�x���");
        }

        private void btnSelectVertex_Click(object sender, RoutedEventArgs e)
        {
            if( nowVertexPoints.Count>0)
            {
                mouseMode = 2;
                MessageBox.Show("���I�I��Ҧ��}�ҡA�б��ۥH�ƹ��I��n�۰ʮե������I", "���I���");
            }
            else {
                MessageBox.Show("�L����i�I�諸���I�A�Х��I���h���", "���I���");
            }
        }

        private async void btnAdjust_Click(object sender, RoutedEventArgs e)
        {
            if( nowVertexPoints.Count<=0 || nowSelectVertex == -1 ) {
                MessageBox.Show("�|��������I�A�Х��I�ﳻ�I","�ץ�");
            }
            else {
                MessageBox.Show("�����}�l�ץ�","�ץ�");
                // �ץ��p��
                var adjustAreaValue = Convert.ToDouble(adjustArea.Text);
                var nowAreaValue = Convert.ToDouble(nowArea.Text);
                var areaFromValue = Convert.ToDouble(areaFrom.Text);
                var areaEndValue = Convert.ToDouble(areaEnd.Text);
                if( nowAreaValue<areaFromValue || nowAreaValue>areaEndValue )
                {
                    MessageBox.Show("����ե����n�����b�_�W�d�򤺡A�нվ��A��");
                    return;
                }

                double x1 = nowCentroid.X;
                double y1 = nowCentroid.Y;
                double x2 = nowVertexPoints[nowSelectVertex].X;
                double y2 = nowVertexPoints[nowSelectVertex].Y;
                // �p��ײv
                double dx = x2 - x1;
                double dy = y2 - y1;
                // �p����I�������Z��
                double distance = Math.Sqrt(dx * dx + dy * dy);
                // �p����V�q
                double unitX = dx / distance;
                double unitY = dy / distance;
                double d = 0.01;                 // �C�� 0.01 ���ا�
                var vertexPoints = new List<MapPoint>(nowVertexPoints);
                double findArea = await CalculatePolygonArea(vertexPoints);
                double x = x2;
                double y = y2;
                if( adjustAreaValue<nowAreaValue )
                {
                    while( findArea>adjustAreaValue )
                    {
                        x = x - d * unitX;
                        y = y - d * unitY;
                        var mapPoint = MapPointBuilder.CreateMapPoint(x, y, MapView.Active.Map.SpatialReference);
                        vertexPoints[nowSelectVertex] = mapPoint;
                        findArea = await CalculatePolygonArea(vertexPoints);
                    }
                }
                else
                {
                    while( findArea<adjustAreaValue )
                    {
                        x = x + d * unitX;
                        y = y + d * unitY;
                        var mapPoint = MapPointBuilder.CreateMapPoint(x, y, MapView.Active.Map.SpatialReference);
                        vertexPoints[nowSelectVertex] = mapPoint;
                        findArea = await CalculatePolygonArea(vertexPoints);
                    }
                }
                MessageBox.Show($"���s��m x:{x},y:{y}\n�ե���s���n:{findArea}\n�����ץ��ϼx");
                // �}�l�N�ץ��᳻�I�g�^
                // �ηs���U���I���ͷs�� Polygon
                List<Segment> segments = new List<Segment>();
                for( var j=1; j<vertexPoints.Count;j++)
                {
                    var seg = LineBuilderEx.CreateLineSegment(vertexPoints[j-1], vertexPoints[j]);
                    segments.Add(seg);
                }
                //segments.Add(LineBuilderEx.CreateLineSegment(vertexPoints[vertexPoints.Count-1], vertexPoints[0]));
                var polygon = PolygonBuilderEx.CreatePolygon(segments, MapView.Active.Map.SpatialReference);
                // �Φ� polygon ��s���ϼx
                QueuedTask.Run(() =>
                {
                    var op = new EditOperation();
                    op.Name = "Update AutoArea Vertexs";
                    op.SelectModifiedFeatures = true;
                    op.SelectNewFeatures = false;
                    Dictionary<string, object> newAtts = new Dictionary<string, object>();
                    newAtts.Add("SHAPE", polygon);
                    op.Modify(nowFeatureLayer, nowFeatureID, newAtts);
                    op.Execute();
                });
            }
        }
    }
}
