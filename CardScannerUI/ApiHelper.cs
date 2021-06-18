using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CardScannerUI
{
    public static class ApiHelper
    {
        public static HttpClient ApiClient { get; private set; }

        public static void Init()
        {
            ApiClient = new HttpClient();
            //ApiClient.BaseAddress = new Uri(api address); //needs the address of the API
        }
    }
}
