using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Configuration;

namespace TestAPI
{
    public static class ApiTestsHelper
    {
        private static HttpClient apiClient;
        public static HttpClient ApiClient { get { return apiClient; } private set { apiClient = value; } }


        public static void Init()
        {
            apiClient = new HttpClient();
            apiClient.BaseAddress = new Uri(GetUri());
        }

        private static string GetUri()
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var ApiUri = configFile.AppSettings.Settings["test"].Value.ToString();
            return ApiUri;
        }

    }
}
