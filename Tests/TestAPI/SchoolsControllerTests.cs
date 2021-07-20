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
    public class SchoolsControllerTests
    {
        private static HttpClient client;
        private static TestContext tc;

        [ClassInitialize]
        public static void ClassInit(TestContext tc)
        {
            SchoolsControllerTests.tc = tc;
            //string path = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath;
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var ApiUri = configFile.AppSettings.Settings["ApiUri"].Value.ToString();
            client = new HttpClient();
            client.BaseAddress = new Uri(ApiUri);
        }

        [TestInitialize]
        public async Task InitializeData()
        {
            await PostData(new School("test school 1", "test1"));
            await PostData(new School("test school 2", "test2"));
        }

        [TestCleanup]
        public async Task CleanUpData()
        {
            await client.DeleteAsync("api/Schools/test1");
            await client.DeleteAsync("api/Schools/test2");
            await client.DeleteAsync("api/Schools/test3");
        }

        private async Task<HttpResponseMessage> PostData(School school)
        {
            string jsonString = JsonSerializer.Serialize(school);
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/Schools", httpContent);
            return response;
        }

        private bool ContainsAnEqualSchool(List<School> list, School value)
        {
            foreach (School i in list)
            {
                if (SchoolsAreEqual(i, value))
                {
                    return true;
                }
            }
            return false;
        }

        private bool SchoolsAreEqual(School school1, School school2)
        {
            if ((school1.ID == school2.ID) && (school1.Name == school2.Name))
            {
                return true;
            }
            return false;
        }

        [TestMethod]
        public async Task TestGetSchools()
        {
            var response = await client.GetAsync("api/Schools");
            List<School> schools = await response.Content.ReadAsAsync<List<School>>();
            Assert.IsTrue(response.IsSuccessStatusCode);

            Assert.IsTrue(ContainsAnEqualSchool(schools, new School("test school 1", "test1")));
            Assert.IsTrue(ContainsAnEqualSchool(schools, new School("test school 2", "test2")));
        }

        [TestMethod]
        public async Task TestGetSchool()
        {
            //get school that exists
            var response = await client.GetAsync("api/Schools/test1");
            School school1 = await response.Content.ReadAsAsync<School>();
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsTrue(SchoolsAreEqual(new School("test school 1", "test1"), school1));

            var response2 = await client.GetAsync("api/Schools/test1");
            School school2 = await response2.Content.ReadAsAsync<School>();
            Assert.IsTrue(response2.IsSuccessStatusCode);
            Assert.IsTrue(SchoolsAreEqual(new School("test school 1", "test1"), school2));

            //get school that doesn't exist
            var response3 = await client.GetAsync("api/Schools/thisSchoolDoesNotExist");
            Assert.IsFalse(response3.IsSuccessStatusCode);
            Assert.AreEqual("NotFound", response3.StatusCode.ToString());
        }

        [TestMethod]
        public async Task TestPutSchool()
        {
            //case where the school exists
            string jsonString = JsonSerializer.Serialize(new School("test school 0", "test1"));
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await client.PutAsync("api/Schools/test1", httpContent);
            Assert.AreEqual("NoContent", response.StatusCode.ToString());

            var response2 = await client.GetAsync("api/Schools");
            List<School> schools = await response2.Content.ReadAsAsync<List<School>>();
            Assert.IsTrue(response2.IsSuccessStatusCode);
            Assert.IsTrue(ContainsAnEqualSchool(schools, new School("test school 0", "test1")));

            //case where the URL doesn't match the school's ID
            string jsonString2 = JsonSerializer.Serialize(new School("test school 0", "test1"));
            var httpContent2 = new StringContent(jsonString2, Encoding.UTF8, "application/json");
            var response3 = await client.PutAsync("api/Schools/1", httpContent2);
            Assert.AreEqual("BadRequest", response3.StatusCode.ToString());

            //case where the school does not exist
            string jsonString3 = JsonSerializer.Serialize(new School("test school 0", "thisSchoolDoesNotExist"));
            var httpContent3 = new StringContent(jsonString3, Encoding.UTF8, "application/json");
            var response4 = await client.PutAsync("api/Schools/thisSchoolDoesNotExist", httpContent3);
            Assert.AreEqual("NotFound", response4.StatusCode.ToString());
        }

        [TestMethod]
        public async Task TestPostSchool()
        {
            //post a school that doesn't already exist
            string jsonString = JsonSerializer.Serialize(new School("test school 3", "test3"));
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/Schools", httpContent);
            Assert.IsTrue(response.IsSuccessStatusCode);

            var response2 = await client.GetAsync("api/Schools");
            List<School> schools = await response2.Content.ReadAsAsync<List<School>>();
            Assert.IsTrue(response2.IsSuccessStatusCode);
            Assert.IsTrue(ContainsAnEqualSchool(schools, new School("test school 3", "test3")));

            //post a school that already exists
            string jsonString2 = JsonSerializer.Serialize(new School("test school 3", "test3"));
            var httpContent2 = new StringContent(jsonString2, Encoding.UTF8, "application/json");
            var response3 = await client.PostAsync("api/Schools", httpContent2);
            Assert.AreEqual("Conflict", response3.StatusCode.ToString());
        }

        [TestMethod]
        public async Task TestDeleteSchool()
        {
            //delete an existing school
            var response = await client.DeleteAsync("api/Schools/test1");
            Assert.AreEqual("NoContent", response.StatusCode.ToString());
            var getResponse = await client.GetAsync("api/Schools/test1");
            Assert.AreEqual("NotFound", getResponse.StatusCode.ToString());

            //delete a school that does not exist
            var response2 = await client.DeleteAsync("api/Schools/test1");
            Assert.AreEqual("NotFound", response2.StatusCode.ToString());
        }
    }
}
