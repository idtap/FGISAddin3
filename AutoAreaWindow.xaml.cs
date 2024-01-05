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
    // 面積修正功能
    public partial class AutoAreaWindow : ProWindow
    {        
        public static int mouseMode = 0;     // 滑鼠點選模式 0/不處理，1/選取特徵，2/選取頂點
        // 目前選取的特徵 
        public static FeatureLayer nowFeatureLayer = null;
        public static long nowFeatureID = -1;
        public static List<MapPoint> nowVertexPoints = new List<MapPoint>();
        public static MapPoint nowCentroid = null;
        // 目前選取的頂點
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
            MessageBox.Show("多邊形點選模式開啟，請接著以滑鼠點選要校正的多邊形", "特徵選取");
        }

        private void btnSelectVertex_Click(object sender, RoutedEventArgs e)
        {
            if( nowVertexPoints.Count>0)
            {
                mouseMode = 2;
                MessageBox.Show("頂點點選模式開啟，請接著以滑鼠點選要自動校正的頂點", "頂點選取");
            }
            else {
                MessageBox.Show("無任何可點選的頂點，請先點取多邊形", "頂點選取");
            }
        }

        private async void btnAdjust_Click(object sender, RoutedEventArgs e)
        {
            if( nowVertexPoints.Count<=0 || nowSelectVertex == -1 ) {
                MessageBox.Show("尚未選取頂點，請先點選頂點","修正");
            }
            else {
                MessageBox.Show("按鍵後開始修正","修正");
                // 修正計算
                var adjustAreaValue = Convert.ToDouble(adjustArea.Text);
                var nowAreaValue = Convert.ToDouble(nowArea.Text);
                var areaFromValue = Convert.ToDouble(areaFrom.Text);
                var areaEndValue = Convert.ToDouble(areaEnd.Text);
                if( nowAreaValue<areaFromValue || nowAreaValue>areaEndValue )
                {
                    MessageBox.Show("期望校正面積必須在起訖範圍內，請調整後再試");
                    return;
                }

                double x1 = nowCentroid.X;
                double y1 = nowCentroid.Y;
                double x2 = nowVertexPoints[nowSelectVertex].X;
                double y2 = nowVertexPoints[nowSelectVertex].Y;
                // 計算斜率
                double dx = x2 - x1;
                double dy = y2 - y1;
                // 計算兩點之間的距離
                double distance = Math.Sqrt(dx * dx + dy * dy);
                // 計算單位向量
                double unitX = dx / distance;
                double unitY = dy / distance;
                double d = 0.01;                 // 每次 0.01 公尺找
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
                MessageBox.Show($"找到新位置 x:{x},y:{y}\n校正後新面積:{findArea}\n按鍵後修正圖徵");
                // 開始將修正後頂點寫回
                // 用新的各頂點產生新的 Polygon
                List<Segment> segments = new List<Segment>();
                for( var j=1; j<vertexPoints.Count;j++)
                {
                    var seg = LineBuilderEx.CreateLineSegment(vertexPoints[j-1], vertexPoints[j]);
                    segments.Add(seg);
                }
                //segments.Add(LineBuilderEx.CreateLineSegment(vertexPoints[vertexPoints.Count-1], vertexPoints[0]));
                var polygon = PolygonBuilderEx.CreatePolygon(segments, MapView.Active.Map.SpatialReference);
                // 用此 polygon 更新此圖徵
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
