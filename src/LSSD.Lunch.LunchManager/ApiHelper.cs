using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace LunchManager
{
    public static class ApiHelper
    {
        private static HttpClient apiClient;
        public static HttpClient ApiClient { get { return apiClient; } private set { apiClient = value; } }

        /**<summary>Initializes the ApiHelper.</summary>
         * <remarks>Must be called before the ApiHelper can be used.</remarks>
         * <param name="ApiUri">The URI of the API.</param>
         */
        public static void Init(string ApiUri)
        {
            apiClient = new HttpClient();
            apiClient.BaseAddress = new Uri(ApiUri);
        }
    }
}
