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


        public static void Init(string ApiUri)
        {
            apiClient = new HttpClient();
            apiClient.BaseAddress = new Uri(ApiUri);
        }
    }
}
