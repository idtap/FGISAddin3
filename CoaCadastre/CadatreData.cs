using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGISAddin3.CoaCadastre
{
    public class CadastreRes
    {
        public string ReturnDescription { get; set; }
        public CadastreData[] ReturnResult { get; set; }
        public bool Status { get; set; }
    }

    public class CadastreData
    {
        public string ReturnPolygon { get; set; }
        public string Return14SecNo { get; set; }
        public string ReturnCenterX { get; set; }
        public string ReturnCenterY { get; set; }
        public string ReturnLandAddress { get; set; }
        public string ReturnLargeLandNo { get; set; }
        public int wkid { set; get; }
    }
}
