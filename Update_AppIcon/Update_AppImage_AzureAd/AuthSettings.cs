using System;
using System.Globalization;

namespace Update_AppImage_AzureAd
{
    class AuthSettings
    {
        /// <summary>
        /// Auth parameters for the client credentials flow -- ensure the app has Permissions for AAD Graph application.readwrite
        /// </summary>
        public static String client_id = "{ client_id }";
        public static String client_secret = "{ client_secret }";
        public static String tenant_id = "{ tenant_id or name }";

        public static string Authority
        {
            get
            {
                return String.Format ( CultureInfo.InvariantCulture , "https://login.microsoftonline.com/{0}" , tenant_id );
            }
        }      
    }

}
