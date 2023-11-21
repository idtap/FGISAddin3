using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGISAddin3.CoaCadastre
{
    public class APIData
    {
        public bool isSuccess { get; set; }
        public string Msg { get; set; }
        public Object other { get; set; }
    }

    public class SecData 
    {
        public long OBJECTID { get; set; }
        public string SCNAME { get; set; }
    }

    public class CountyData 
    {
        public string County { get; set; }
    }

    public class TownData
    {
        public string Town { get; set; }
    }

}
