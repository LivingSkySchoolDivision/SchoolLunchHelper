using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using Data.Models;
using System.Text.Json;
using System.Configuration;

namespace TestAPI
{
    [TestClass]
    public class StudentsControllerTests
    {
        private static HttpClient client;
        private static TestContext tc;

        [ClassInitialize]
        public static void ClassInit(TestContext tc)
        {
            StudentsControllerTests.tc = tc;
            string path = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath;
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var ApiUri = configFile.AppSettings.Settings["ApiUri"].Value.ToString();
            client = new HttpClient();
            client.BaseAddress = new Uri(ApiUri);
        }

        [TestInitialize]
        public async Task InitializeData()
        {

        }

        [TestCleanup]
        public async Task CleanUpData()
        {

        }

        private async Task<HttpResponseMessage> PostData(Student student)
        {
            string jsonString = JsonSerializer.Serialize(student);
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/Students", httpContent);
            return response;
        }

        private bool ContainsAnEqualStudent(List<Student> list, Student value)
        {
            foreach (Student i in list)
            {
                if (i.ToString().Equals(value.ToString()))
                {
                    return true;
                }
            }
            return false;
        }

        [TestMethod]
        public async Task TestGetStudents()
        {

        }

        [TestMethod]
        public async Task TestGetStudent()
        {

        }

        [TestMethod]
        public async Task TestGetStudentsBySchool()
        {

        }

        [TestMethod]
        public async Task TestPutStudent()
        {

        }

        [TestMethod]
        public async Task TestPostStudent()
        {

        }

        [TestMethod]
        public async Task TestDeleteStudent()
        {

        }

    }
}
