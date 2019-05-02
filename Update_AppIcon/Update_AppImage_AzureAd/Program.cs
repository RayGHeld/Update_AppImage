using System;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Update_AppImage_AzureAd
{
    class Program
    {
        static HttpClient client;
        static String AppId_toUpdate = ""; // the app id of the application we are trying to set the logo to
        static String NewImage_Path = ""; // the image must exist at the root of the directory (debug bin in this case)


        static void Main ( string [ ] args )
        {
            try
            {
                PostImage ().Wait ();
            } catch (Exception ex) {
                Console.WriteLine ( $"Exception in Program.cs, Line 22: {ex.Message} " );
            }

            Console.WriteLine ( "\nPress any key to continue..." );
            Console.ReadKey ();
        }

        static async Task PostImage ()
        {
            JObject app;
            JObject resp;

            AuthenticationResult authResult = await Get_ClientCredentials_AccessToken("https://graph.windows.net");
            Console.WriteLine ( $"TokenType: {authResult.AccessTokenType}\n" );
            Console.WriteLine ( $"AccessToken: {authResult.AccessToken}\n" );

            // make sure the app exists
            app = await AADGraph_Get_Application ( authResult.AccessToken , AppId_toUpdate );

            if ( app != null )
            {
                Console.WriteLine ( $"\nJSON\n\n{ app.ToString () }" );
                resp = await AADGraph_Set_AppIconImage ( authResult.AccessToken , AppId_toUpdate , NewImage_Path );
            } else
            {
                Console.WriteLine ( $"\nApp id {AppId_toUpdate} does not exist." );
            }
        }

        public static async Task<JObject> AADGraph_Get_Application ( String authToken , String appId )
        {
            JObject o = null;

            client = new HttpClient();

            client.BaseAddress = new Uri ( "https://graph.windows.net/" );
            client.DefaultRequestHeaders.Accept.Add ( new MediaTypeWithQualityHeaderValue ( "application/json" ) );
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue ( "Bearer" , authToken );

            HttpRequestMessage req = new HttpRequestMessage( HttpMethod.Get, new Uri($"{client.BaseAddress}{AuthSettings.tenant_id}/applicationsByAppId/{appId}?api-version=1.6" ) );
            HttpResponseMessage resp = await client.SendAsync ( req );

            try
            {
                resp.EnsureSuccessStatusCode ();

                String content = await resp.Content.ReadAsStringAsync();
                o = JObject.Parse(content);

                Console.WriteLine ( $"App Object Id: {o.GetValue ( "objectId" )}" );
                Console.WriteLine ( $"App Id:        {o.GetValue ( "appId" )}" );

                JArray reqResources = (JArray) o.GetValue("requiredResourceAccess");
                Console.WriteLine ( $"\nNumber of Required Resources Listed: { reqResources.Count }\n" );

                //Console.WriteLine ( $"Resource Id: {o.GetValue ( "requiredResourceAccess")[0]["resourceAppId"] }" );
                for ( int i = 0; i < reqResources.Count; i++ )
                {
                    Console.WriteLine ( $"{i + 1} - Id: {o.GetValue ( "requiredResourceAccess" ) [ i ] [ "resourceAppId" ] }" );
                }

            } catch (HttpRequestException ex )
            {
                Console.WriteLine ( $"\nError while getting application JSON: \nTS: { DateTime.Now.ToString () }\nError: { ex.Message }" );
            }

            client = null;
            return o;

        }

        public static async Task<JObject> AADGraph_Set_AppIconImage ( String authToken, String appId, String imgPath )
        {
            JObject o = null;

            client = new HttpClient ();

            client.BaseAddress = new Uri ( "https://graph.windows.net/" );
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue ( "Bearer" , authToken );
            
            HttpRequestMessage req = new HttpRequestMessage( HttpMethod.Put, new Uri($"{ client.BaseAddress }{ AuthSettings.tenant_id }/applicationsByAppId/{ appId }/mainLogo?api-version=1.6" ) );

            req.Content = new StreamContent ( new MemoryStream ( File.ReadAllBytes ( imgPath ) ) );
            req.Content.Headers.Add ( "Content-Type" , "application/octet-stream" );

            HttpResponseMessage resp = await client.SendAsync( req );

            try
            {
                resp.EnsureSuccessStatusCode ();

                String content = await resp.Content.ReadAsStringAsync();
                if (content.Length > 0 )
                {
                    o = JObject.Parse(content);
                }

                Console.WriteLine ( "Image update successful... " );

            } catch (HttpRequestException ex )
            {
                Console.WriteLine ( $"\nError updating image: \nTS: { DateTime.Now.ToString() }\nError: { ex.Message }" );
            }

            client = null;
            return o;

        }

        private static async Task<AuthenticationResult> Get_ClientCredentials_AccessToken ( string resourceId )
        {
            AuthenticationResult result = null;
            try
            {
                TokenCache cache = new TokenCache();
                ClientCredential clientCred = null;

                AuthenticationContext context = new AuthenticationContext(AuthSettings.Authority, cache);
                clientCred = new ClientCredential ( AuthSettings.client_id , AuthSettings.client_secret );

                result = await context.AcquireTokenAsync ( resourceId , clientCred );
            }
            catch ( AdalException ex )
            {
                Console.WriteLine ( $"Error while acquiring token: \nTS: { DateTime.Now.ToString () }\nError: { ex.Message }" );
            }
            return result;
        }

    }
}
