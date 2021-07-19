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
    public class FoodItemsControllerTests
    {
        private static HttpClient client;
        private static TestContext tc;

        [ClassInitialize]
        public static void ClassInit(TestContext tc)
        {
            FoodItemsControllerTests.tc = tc;
            //string path = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath;
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var ApiUri = configFile.AppSettings.Settings["ApiUri"].Value.ToString();
            client = new HttpClient();
            client.BaseAddress = new Uri(ApiUri);
        }

        [TestInitialize]
        public async Task InitializeData()
        {
            await PostData(new FoodItem("test food 1", "test1", "testSchool1", 1.00M, "description"));
            await PostData(new FoodItem("test food 2", "test2", "testSchool2", 2.00M, "description"));
        }

        [TestCleanup]
        public async Task CleanUpData()
        {
            await client.DeleteAsync("api/FoodItems/test1");
            await client.DeleteAsync("api/FoodItems/test2");
            await client.DeleteAsync("api/FoodItems/test3");
        }

        private async Task<HttpResponseMessage> PostData(FoodItem food)
        {
            string jsonString = JsonSerializer.Serialize(food);
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/FoodItems", httpContent);
            return response;
        }

        private bool ContainsAnEqualFoodItem(List<FoodItem> list, FoodItem value)
        {
            foreach (FoodItem i in list)
            {
                if (FoodItemsAreEqual(i, value))
                {
                    return true;
                }
            }
            return false;
        }

        private bool FoodItemsAreEqual(FoodItem food1, FoodItem food2)
        {
            if ((food1.ID == food2.ID) && (food1.Name == food2.Name) && (food1.SchoolID == food2.SchoolID) && (food1.Cost == food2.Cost) && (food1.Description == food2.Description))
            {
                return true;
            }
            return false;
        }

        [TestMethod]
        public async Task TestGetFoodItems()
        {
            var response = await client.GetAsync("api/FoodItems");
            List<FoodItem> foodItems = await response.Content.ReadAsAsync<List<FoodItem>>();
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsTrue(ContainsAnEqualFoodItem(foodItems, new FoodItem("test food 1", "test1", "testSchool1", 1.00M, "description")));
            Assert.IsTrue(ContainsAnEqualFoodItem(foodItems, new FoodItem("test food 2", "test2", "testSchool2", 2.00M, "description")));
        }

        [TestMethod]
        public async Task TestGetFoodItem()
        {
            //get food item that exists
            var response = await client.GetAsync("api/FoodItems/test1");
            FoodItem food1 = await response.Content.ReadAsAsync<FoodItem>();
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsTrue(FoodItemsAreEqual(new FoodItem("test food 1", "test1", "testSchool1", 1.00M, "description"), food1));

            //get food item that does not exist
            var response2 = await client.GetAsync("api/FoodItems/thisFoodItemDoesNotExist");
            Assert.IsFalse(response2.IsSuccessStatusCode);
            Assert.AreEqual("NotFound", response2.StatusCode.ToString());
        }

        [TestMethod]
        public async Task TestGetFoodItemsBySchool()
        {
            //case where school exists
            var response = await client.GetAsync("api/FoodItems/School/testSchool1");
            List<FoodItem> foodItems = await response.Content.ReadAsAsync<List<FoodItem>>();
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsTrue(ContainsAnEqualFoodItem(foodItems, new FoodItem("test food 1", "test1", "testSchool1", 1.00M, "description")));
            Assert.IsFalse(ContainsAnEqualFoodItem(foodItems, new FoodItem("test food 2", "test2", "testSchool2", 2.00M, "description")));

            var response2 = await client.GetAsync("api/FoodItems/School/testSchool2");
            List<FoodItem> foodItems2 = await response2.Content.ReadAsAsync<List<FoodItem>>();
            Assert.IsTrue(response2.IsSuccessStatusCode);
            Assert.IsFalse(ContainsAnEqualFoodItem(foodItems2, new FoodItem("test food 1", "test1", "testSchool1", 1.00M, "description")));
            Assert.IsTrue(ContainsAnEqualFoodItem(foodItems2, new FoodItem("test food 2", "test2", "testSchool2", 2.00M, "description")));

            //case where school does not exist
            var response3 = await client.GetAsync("api/FoodItems/School/aSchoolThatDoesNotExist");
            Assert.IsTrue(response3.IsSuccessStatusCode);
            List<FoodItem> foodItems3 = await response3.Content.ReadAsAsync<List<FoodItem>>();
            Assert.IsFalse((foodItems3?.Any()) == true); //checks that the list is empty
        }

        [TestMethod]
        public async Task TestPutFoodItem()
        {
            //case where food item exists
            string jsonString = JsonSerializer.Serialize(new FoodItem("test food", "test1", "testSchool1", 1.00M, "description 1"));
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await client.PutAsync("api/FoodItems/test1", httpContent);
            Assert.AreEqual("NoContent", response.StatusCode.ToString());

            var response2 = await client.GetAsync("api/FoodItems");
            List<FoodItem> foodItems = await response2.Content.ReadAsAsync<List<FoodItem>>();
            Assert.IsTrue(response2.IsSuccessStatusCode);
            Assert.IsTrue(ContainsAnEqualFoodItem(foodItems, new FoodItem("test food", "test1", "testSchool1", 1.00M, "description 1")));

            //case where the URL doesn't match the food item's ID
            string jsonString2 = JsonSerializer.Serialize(new FoodItem("test food", "test1", "testSchool1", 1.00M, "description 1"));
            var httpContent2 = new StringContent(jsonString2, Encoding.UTF8, "application/json");
            var response3 = await client.PutAsync("api/FoodItems/1", httpContent2);
            Assert.AreEqual("BadRequest", response3.StatusCode.ToString());

            //case where the food item does not exist
            string jsonString3 = JsonSerializer.Serialize(new FoodItem("test food 0", "thisFoodItemDoesNotExist", "testSchool1", 1.00M, "description 1"));
            var httpContent3 = new StringContent(jsonString3, Encoding.UTF8, "application/json");
            var response4 = await client.PutAsync("api/FoodItems/thisFoodItemDoesNotExist", httpContent3);
            Assert.AreEqual("NotFound", response4.StatusCode.ToString());
        }

        [TestMethod]
        public async Task TestPostFoodItem()
        {
            //post a food item that doesn't already exist
            string jsonString = JsonSerializer.Serialize(new FoodItem("test food 3", "test3", "testSchool3", 5.00M, "description 3"));
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/FoodItems", httpContent);
            Assert.IsTrue(response.IsSuccessStatusCode);

            var response2 = await client.GetAsync("api/FoodItems");
            List<FoodItem> foodItems = await response2.Content.ReadAsAsync<List<FoodItem>>();
            Assert.IsTrue(response2.IsSuccessStatusCode);
            Assert.IsTrue(ContainsAnEqualFoodItem(foodItems, new FoodItem("test food 3", "test3", "testSchool3", 5.00M, "description 3")));

            //post a food item that already exists
            string jsonString2 = JsonSerializer.Serialize(new FoodItem("test food 3", "test3", "testSchool3", 5.00M, "description 3"));
            var httpContent2 = new StringContent(jsonString2, Encoding.UTF8, "application/json");
            var response3 = await client.PostAsync("api/FoodItems", httpContent2);
            Assert.AreEqual("Conflict", response3.StatusCode.ToString());
        }

        [TestMethod]
        public async Task TestDeleteFoodItem()
        {
            //delete an existing food item
            var response = await client.DeleteAsync("api/FoodItems/test1");
            Assert.AreEqual("NoContent", response.StatusCode.ToString());
            var getResponse = await client.GetAsync("api/FoodItems/test1");
            Assert.AreEqual("NotFound", getResponse.StatusCode.ToString());

            //delete a food item that does not exist
            var response2 = await client.DeleteAsync("api/FoodItems/test1");
            Assert.AreEqual("NotFound", response2.StatusCode.ToString());
        }

    }
}
