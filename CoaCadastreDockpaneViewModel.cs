using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using FGISAddin3.CoaCadastre;

namespace FGISAddin3
{
    internal class CoaCadastreDockpane_ShowButton : Button
    {
        protected override void OnClick()
        {
            CoaCadastreDockpaneViewModel.Show();

        }
    }

    internal class CoaCadastreDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "FGISAddin3_CoaCadastreDockpane";
        private List<IDisposable> _graphic;
        private CIMTextSymbol _TextSymbol = null;
        private CIMPolygonSymbol _PolygonSymbol = null;

        protected CoaCadastreDockpaneViewModel()
        {
            _graphic = Utility.Graphics("CadastreQuery");

            QueuedTask.Run(() =>
            {
                CIMStroke outline = SymbolFactory.Instance.ConstructStroke(CIMColor.CreateRGBColor(255, 0, 0), 2.0, SimpleLineStyle.Solid);
                List<CIMSymbolLayer> symbolLayers = new List<CIMSymbolLayer>();
                symbolLayers.Add(outline);
                _PolygonSymbol = new CIMPolygonSymbol()
                {
                    SymbolLayers = symbolLayers.ToArray()
                };
            });
        }

        protected override Task InitializeAsync() {
            return base.InitializeAsync();
        }        
        
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;
            pane.Activate();
        }

        private string _Key;
        public string Key
        {
            get => _Key;
            set => SetProperty(ref _Key, value, () => Key);
        }

        public IEnumerable<CadastreData> _Cadastres;
        public IEnumerable<CadastreData> CadastreDatas
        {
            get { return _Cadastres; }
            set
            {
                SetProperty(ref _Cadastres, value, () => CadastreDatas);
            }
        }

        private CadastreData _selectedCadastre;
        public CadastreData SelectedCadastre
        {
            get { return _selectedCadastre; }
            set
            {
                SetProperty(ref _selectedCadastre, value, () => SelectedCadastre);
            }
        }

        private ICommand _cmdSearch;
        public ICommand CmdSearch => _cmdSearch ?? (_cmdSearch = new RelayCommand(() =>
        {
            QueuedTask.Run(async () =>
            {
                CadastreDatas = null;
                var data = await APISource.GetCadastres(Key);
                CadastreDatas = data;
                if (data == null)
                {
                    MessageBox.Show("地籍查詢伺服器無回應");
                }
            });
        }, () => !string.IsNullOrEmpty(Key)));

        private ICommand _cmdLocate;
        public ICommand CmdLocate => _cmdLocate ?? (_cmdLocate = new RelayCommand(() =>
        {
            Locate();
        }, () => SelectedCadastre != null));

        private ICommand _cmdClear;
        public ICommand CmdClear => _cmdClear ?? (_cmdClear = new RelayCommand(() =>
        {
            _graphic.ForEach(x => x.Dispose());
            _graphic.Clear();
        }, () => _graphic.Count > 0));

        public void Locate()
        {
            var data = SelectedCadastre;
            try
            {
                QueuedTask.Run(() =>
                {
                    MapPoint pt = MapPointBuilder.CreateMapPoint(double.Parse(data.ReturnCenterX),
                    double.Parse(data.ReturnCenterY),
                    SpatialReferenceBuilder.CreateSpatialReference(data.wkid));

                    var val = data.ReturnPolygon.Substring(1);
                    val = val.Substring(0, val.Length - 1);
                    var pts = val.Split(new string[] { "],[" }, StringSplitOptions.None).Select(c => c.Split(new char[] { ',' }));
                    var mapPts = pts.Select(c => MapPointBuilder.CreateMapPoint(double.Parse(c.First()), double.Parse(c[1]),
                        SpatialReferenceBuilder.CreateSpatialReference(data.wkid)));

                    var no = int.Parse(data.Return14SecNo.Substring(6, 4));
                    var subno = int.Parse(data.Return14SecNo.Substring(10, 4));

                    var txt = new CIMTextGraphic()
                    {
                        Symbol = _TextSymbol.MakeSymbolReference(),
                        Text = string.Format("{0}{1}", no.ToString(), subno > 0 ? "-" + subno.ToString() : ""),
                        Shape = pt
                    };
                    var polygon = PolygonBuilder.CreatePolygon(mapPts);

                    _graphic.Add(MapView.Active.AddOverlay(polygon, _PolygonSymbol.MakeSymbolReference()));
                    _graphic.Add(MapView.Active.AddOverlay(txt));
                    MapView.Active.PanTo(pt);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }

}
