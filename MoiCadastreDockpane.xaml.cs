
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

            // �ǳƥ� nlsc �A��Ū�� xml �� parser
            //var xmlResult = Utility.myHttpGET(apiRoot_NLSC+"other/ListCounty",Encoding.UTF8);
            //if (xmlResult.Substring(0, 5).Equals("error"))
            //{
            //    MessageBox.Show("��g��ø����(nlsc)���A���L�^���A�N�L�k�ϥΦa�F�q�a�y�d��", "�t�γq��");
            //    return;
            //}
            //XDocument doc = XDocument.Parse(xmlResult);
            //List<CountyItem> items = doc.Root.Elements("countyItem").Select(e => new CountyItem
            //{
            //    countycode = (string)e.Element("countycode"),
            //    countyname = (string)e.Element("countyname")
            //}).ToList();

            // �]��g��ø ListCounty �`��(�i��j�a�{����l���s��)�A���B�g��
            List<CountyItem> items = new List<CountyItem>
            {
                new CountyItem { countycode = "A", countyname = "�O�_��" },
                new CountyItem { countycode = "B", countyname = "�O����" },
                new CountyItem { countycode = "C", countyname = "�򶩥�" },
                new CountyItem { countycode = "D", countyname = "�O�n��" },
                new CountyItem { countycode = "E", countyname = "������" },
                new CountyItem { countycode = "F", countyname = "�s�_��" },
                new CountyItem { countycode = "G", countyname = "�y����" },
                new CountyItem { countycode = "H", countyname = "��饫" },
                new CountyItem { countycode = "I", countyname = "�Ÿq��" },
                new CountyItem { countycode = "J", countyname = "�s�˿�" },
                new CountyItem { countycode = "K", countyname = "�]�߿�" },
                new CountyItem { countycode = "M", countyname = "�n�뿤" },
                new CountyItem { countycode = "N", countyname = "���ƿ�" },
                new CountyItem { countycode = "O", countyname = "�s�˥�" },
                new CountyItem { countycode = "P", countyname = "���L��" },
                new CountyItem { countycode = "Q", countyname = "�Ÿq��" },
                new CountyItem { countycode = "T", countyname = "�̪F��" },
                new CountyItem { countycode = "U", countyname = "�Ὤ��" },
                new CountyItem { countycode = "V", countyname = "�O�F��" },
                new CountyItem { countycode = "W", countyname = "������" },
                new CountyItem { countycode = "X", countyname = "���" },
                new CountyItem { countycode = "Z", countyname = "�s����" }
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
                MessageBox.Show("��g��ø����(nlsc)���A���L�^��", "�t�γq��");
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
                MessageBox.Show("��g��ø����(nlsc)���A���L�^��", "�t�γq��");
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
            // �q�p�q�D��,�Ȥ��������
        }                

        private void LstMoiCadastre_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // �����d�ߵ��G ListBox ���خɭn���ʦa�ϦܸӳB
            if (lstMoiCadastre.SelectedItem == null) return;
            // ���U���� posStr, ø�s����
            renderQueryResult(lstMoiCadastre.SelectedValue.ToString(),
                              ((MoiCadastreItem)lstMoiCadastre.SelectedItem).showStr);
        }

        // ø�s�Ѧr�ꦡ�� x1,y1;x2,y2;... �զ��� polygon �ϸ�
        private void renderQueryResult( string ringStr, string displayStr )
        {
            //MessageBox.Show(ringStr);

            try
            {
                QueuedTask.Run(() =>
                {
                    // ��Ѧ� ringStr ø�s�Ωw��(�����I)
                    var pts = ringStr.Split(new string[] { ";" }, StringSplitOptions.None).Select(c => c.Split(new char[] { ',' }));
                    // �a�� point
                    var mapPts = pts.Select(c => MapPointBuilder.CreateMapPoint(
                                                    double.Parse(c.First()),
                                                    double.Parse(c[1]),
                                                    SpatialReferences.WGS84));   // �a�F�q�θg�n��
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
                MessageBox.Show(ex.ToString(),"���~�q��");
            }
        }
        

        private void txtLandNo_TextChanged(object sender, TextChangedEventArgs e)
        {
            // �ˬd��J�O�_�ŦX 00010000,1-1,1 
        }

        // �d��
        private void MoiQueryButton_Click(object sender, RoutedEventArgs e)
        {
            // �����z�L���O�p��I�s nlsc MAP_0001 api                                             
            var url = "https://gis.epa.gov.tw/EPAWebGIS/Map/GetCadasMap/";
            url += "?city=" + cmbCounty.SelectedValue.ToString() + 
                   "&sect=" + cmbSec.SelectedValue.ToString() + 
                   "&sectno=" + txtLandNo.Text.Trim();
            MessageBox.Show("�����е��ݦܵ��G�X�{!!","�t�γq��");
            var xmlResult = Utility.myHttpGET(url, Encoding.UTF8);
            if (xmlResult.Substring(0, 5).Equals("error"))
            {
                MessageBox.Show("��g��ø����(nlsc)���A���L�^��", "�t�γq��");
                return;
            }
            // �^�Ǫ���Ƭ� gml �榡,�n�ۦ� parser
            var err = xmlResult.Contains("<error>");
            if (err)
            {
                MessageBox.Show("�d�L���a��", "�t�γq��");
                return;
            }
            var a = xmlResult.IndexOf("<gml:featureMember>");
            var a1 = xmlResult.IndexOf("<gml:coordinates>", a);
            var a2 = xmlResult.IndexOf("</gml:coordinates>", a);
            string posStr = "";
            if (a1 != -1 && a2 != -1)
            {
                var astr = xmlResult.Substring(a1 + 17, a2-(a1+17));
                // ���r��, �զX [x,y]
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
            // �N��[�� lstMoiItems
            lstMoiItems.Add(moiItem);
            // �w��즹
            renderQueryResult(posStr,showStr);
                              

        }

        // �M���d�ߵ��G
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
