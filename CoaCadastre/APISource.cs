
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FGISAddin3.CoaCadastre
{
    public class APISource
    {
        private const string _ApiRoot = "https://coagis.colife.org.tw/arcgis/";
        private static Token Token = null;

        public static async Task<IEnumerable<Feature>> GetCounties()
        {
            return await Utility.GetFeatures(string.Format("{0}rest/services/CadastralMapUtil/Util/MapServer",
                _ApiRoot), "1", "1=1", "OBJECTID,CountyName");
        }

        public static async Task<Feature> GetCounty(long oid)
        {
            var res = await Utility.GetFeature(new List<long> { oid },
                string.Format("{0}rest/services/CadastralMapUtil/Util/MapServer",
                _ApiRoot), "1");
            return res.FirstOrDefault();
        }

        public static async Task<IEnumerable<Feature>> GetTowns(string county)
        {
            return await Utility.GetFeatures(string.Format("{0}rest/services/CadastralMapUtil/Util/MapServer",
                _ApiRoot), "0", string.Format("CountyName='{0}'", county), "OBJECTID,CountyName,TownName");
        }

        public static async Task<Feature> GetTown(long oid)
        {
            var res = await Utility.GetFeature(new List<long> { oid },
                string.Format("{0}rest/services/CadastralMapUtil/Util/MapServer", _ApiRoot), "0");
            return res.FirstOrDefault();
        }

        public static async Task<IEnumerable<Feature>> GetSecs(string county, string town)
        {
            try
            {
                var ctys = new string[] { "連江縣", "金門縣", "澎湖縣" };

                var codes = JsonConvert.DeserializeObject<AdminCode[]>(Properties.Resources.AdminCode);
                var code = codes.FirstOrDefault(x => x.county == county && x.town == town);
                return await Utility.GetFeatures(string.Format("{0}rest/services/Section/Section_106Q4/MapServer",
                    _ApiRoot), (ctys.Contains(county) ? "0" : "1"), string.Format("CTY='{0}' and TOWN='{1}'", code.countycode, code.towncode),
                    (ctys.Contains(county) ? "OBJECTID,SCNAME,SCNO,SCNOEXT,CTY,TOWN" : "OBJECTID_1,SCNAME,SCNO,SCNOEXT,CTY,TOWN"));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<Feature> GetSec(long oid)
        {
            var res = await Utility.GetFeature(new List<long> { oid },
                string.Format("{0}rest/services/Section/Section_106Q4/MapServer", _ApiRoot), "1");
            return res.FirstOrDefault();
        }

        public static async Task<CadastreData[]> GetCadastres(string LandAddress)
        {
            var ctys = new string[] { "連江縣", "金門縣", "澎湖縣" };

            if (Token == null || Token.token==null)
                Token = await GetTokenAsync();
            using (var client = new HttpClient() { BaseAddress = new Uri(_ApiRoot) })
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var Info = new Dictionary<string, string>();
                Info.Add("token", Token?.token);
                Info.Add("LandAddress", LandAddress);
                Info.Add("LandVersion", "106Q4");
                Info.Add("CodeVersion", "106Q4");
                Info.Add("SpatialRefZone", ctys.Contains(LandAddress.Substring(0, 3)) ? "外島" : "本島");
                Info.Add("SpatialRefOutput", ctys.Contains(LandAddress.Substring(0, 3)) ? "3825" : "3826");
                Info.Add("f", "json");

                var content = new FormUrlEncodedContent(Info);
                using (var response = await client.PostAsync("rest/services/CadastralMap/SOE/MapServer/exts/CoaRESTSOE/LandAddressToLocation", content))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = await response.Content.ReadAsStringAsync();
                        var res = JsonConvert.DeserializeObject<CadastreRes>(responseData);
                        res.ReturnResult?.ToList().ForEach(x => 
                        x.wkid = ctys.Contains(LandAddress.Substring(0, 3)) ? 3825 : 3826);
                        return res.ReturnResult;
                    }
                    return null;
                }
            }
        }

        private static async Task<Token> GetTokenAsync()
        {
            var ip = await GetIP();
            using (var client = new HttpClient() { BaseAddress = new Uri(_ApiRoot) })
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var Info = new Dictionary<string, string>();
                //Info.Add("username", "aercuser");
                //Info.Add("password", "Aerc2apiuse");
                Info.Add("username", "User_CadastralMap");
                Info.Add("password", "User_CadastralMap2017coa");
                Info.Add("ip", ip);
                Info.Add("expiration", "60");
                Info.Add("f", "json");
                var content = new FormUrlEncodedContent(Info);
                using (var response = await client.PostAsync("tokens/generateToken", content))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<Token>(responseData);
                    }
                    return null;
                }
            }
        }

        private async static Task<string> GetIP()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri("http://icanhazip.com") })
            {
                using (var res = await client.GetAsync(""))
                {
                    if (res.IsSuccessStatusCode)
                    {
                        return await res.Content.ReadAsStringAsync();
                    }
                    return "";
                }
            }
        }


    }
}
