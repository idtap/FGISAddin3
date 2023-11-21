using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace FGISAddin3
{
    public class ImageServiceTool
    {
        public static ObservableCollection<ImageServiceModel>imageServices = 
             new ObservableCollection<ImageServiceModel>();
        private static string serviceFilePath = Utility.AddinAssemblyLocation()+"/Images/services.json";

        public static void LoadServiceJson()
        {
            List<ImageServiceModel> loadedService = Utility.LoadFromJsonFile
                                                   <List<ImageServiceModel>>(serviceFilePath);
            if (loadedService != null)
            {
                imageServices.Clear();
                foreach (var service in loadedService)
                {
                    imageServices.Add((ImageServiceModel)service);
                }
            }
        }

        public static void SaveServiceJson()
        {
            Utility.SaveToJsonFile(serviceFilePath, imageServices);
        }

        public static ImageServiceModel getItemByServiceName( string service_name )
        {
            ImageServiceModel getItem = null;
            foreach (var service in imageServices)
            {
                if( service.ServiceName.ToString().Equals(service_name) ) {
                    getItem = service;
                    break;
                }
            }

            return getItem;
        }
    }

    public class ImageServiceModel
    {
        public string ServiceName { get; set; }
        public string ServiceType { get; set; }
        public string ServiceUrl { get; set; }
        public string ProxyUrl { get; set; }
        public string Token { get; set; }
        public string QueryField { get; set; }
        public string FieldType { get; set; }
    }
}
