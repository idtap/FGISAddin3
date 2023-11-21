using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace FGISAddin3
{
    internal class FGISAddin3Module : Module
    {
        private static FGISAddin3Module _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static FGISAddin3Module Current
        {
            get
            {
                return _this ?? (_this = (FGISAddin3Module)FrameworkApplication.FindModule("FGISAddin3_Module"));
            }
        }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

    }
}
