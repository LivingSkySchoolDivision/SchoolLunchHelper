using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using Data.Models;
using System.Text.Json;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestAPI
{
    [TestClass]
    public class TransactionsControllerTests
    {
        private HttpClient client;

        [ClassInitialize]
        public static void ClassInit(TestContext tc)
        {
            //initialize the client
            ApiTestsHelper.Init();
            TransactionsControllerTests.client = ApiTestsHelper.ApiClient;
        }

        [TestInitialize]
        public async Task InitializeData() //runs before each test
        {
            await PostData(new Student("test1", "test student 1", "test0", 10.00M, "medical info"));
            await PostData(new Student("test2", "test student 2", "test0", 10.00M, "medical info"));
            await PostData(new Student("test3", "test student 3", "test1", 10.00M, "medical info"));
            await PostData(new Student("test4", "test student 4", "test1", 10.00M, "medical info"));
            await PostData(new Student("test5", "test student 5", "test3", 10.00M, "medical info"));

            await PostData(new Transaction(1.00M, "test1", "test student 1", "test1", "food1", "test0", "test school0", new DateTime(2021, 07, 07), "1"));
            await PostData(new Transaction(2.00M, "test2", "test student 2", "test2", "food2", "test0", "test school0", new DateTime(2021, 07, 07), "2"));
            await PostData(new Transaction(1.00M, "test3", "test student 3", "test1", "food1", "test1", "test school1", new DateTime(2021, 07, 07), "3"));
            await PostData(new Transaction(2.00M, "test4", "test student 4", "test2", "food2", "test1", "test school1", new DateTime(2021, 07, 07), "4"));
        }

        [TestCleanup]
        public async Task CleanUpData() //runs after each test
        {
            await client.DeleteAsync("api/Transactions/1");
            await client.DeleteAsync("api/Transactions/2");
            await client.DeleteAsync("api/Transactions/3");
            await client.DeleteAsync("api/Transactions/4");

            await client.DeleteAsync("api/Students/test1");
            await client.DeleteAsync("api/Students/test2");
            await client.DeleteAsync("api/Students/test3");
            await client.DeleteAsync("api/Students/test4");
            await client.DeleteAsync("api/Students/test5");

        }

        private async Task<HttpResponseMessage> PostData(Transaction transaction)
        {
            string jsonString = JsonSerializer.Serialize(transaction);
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/Transactions", httpContent);
            return response;
        }

        private async Task<HttpResponseMessage> PostData(Student student)
        {
            string jsonString = JsonSerializer.Serialize(student);
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/Students", httpContent);
            return response;
        }

        private bool ContainsAnEqualTransaction(List<Transaction> list, Transaction value)
        {
            foreach (Transaction i in list)
            {
                if (i.ToString().Equals(value.ToString()))
                {
                    return true;
                }
            }
            return false;
        }


        [TestMethod]
        public async Task TestGetTransactions()
        {
            var response = await client.GetAsync("api/Transactions");
            List<Transaction> transactions = await response.Content.ReadAsAsync<List<Transaction>>();

            Assert.IsTrue(ContainsAnEqualTransaction(transactions, new Transaction(1.00M, "test1", "test student 1", "test1", "food1", "test0", "test school0", new DateTime(2021, 07, 07), "1")));
            Assert.IsTrue(ContainsAnEqualTransaction(transactions, new Transaction(2.00M, "test2", "test student 2", "test2", "food2", "test0", "test school0", new DateTime(2021, 07, 07), "2")));
            Assert.IsTrue(ContainsAnEqualTransaction(transactions, new Transaction(1.00M, "test3", "test student 3", "test1", "food1", "test1", "test school1", new DateTime(2021, 07, 07), "3")));
            Assert.IsTrue(ContainsAnEqualTransaction(transactions, new Transaction(2.00M, "test4", "test student 4", "test2", "food2", "test1", "test school1", new DateTime(2021, 07, 07), "4")));

        }

        [TestMethod]
        public async Task TestGetTransaction()
        {
            var response = await client.GetAsync("api/Transactions/test1");
            Transaction transaction1 = await response.Content.ReadAsAsync<Transaction>();
            Assert.AreEqual(new Transaction(1.00M, "test1", "test student 1", "test1", "food1", "test0", "test school0", new DateTime(2021, 07, 07), "1"), transaction1);

            var response2 = await client.GetAsync("api/Transactions/test3");
            Transaction transaction2 = await response2.Content.ReadAsAsync<Transaction>();
            Assert.AreEqual(new Transaction(1.00M, "test3", "test student 3", "test1", "food1", "test1", "test school1", new DateTime(2021, 07, 07), "3"), transaction2);

        }

        [TestMethod]
        public async Task TestGetTransactionsBySchool()
        {
            var response = await client.GetAsync("api/Transactions/School/test0");
            List<Transaction> transactions = await response.Content.ReadAsAsync<List<Transaction>>();

            Assert.IsTrue(ContainsAnEqualTransaction(transactions, new Transaction(1.00M, "test1", "test student 1", "test1", "food1", "test0", "test school0", new DateTime(2021, 07, 07), "1")));
            Assert.IsTrue(ContainsAnEqualTransaction(transactions, new Transaction(2.00M, "test2", "test student 2", "test2", "food2", "test0", "test school0", new DateTime(2021, 07, 07), "2")));
            Assert.IsFalse(ContainsAnEqualTransaction(transactions, new Transaction(1.00M, "test3", "test student 3", "test1", "food1", "test1", "test school1", new DateTime(2021, 07, 07), "3")));
            Assert.IsFalse(ContainsAnEqualTransaction(transactions, new Transaction(2.00M, "test4", "test student 4", "test2", "food2", "test1", "test school1", new DateTime(2021, 07, 07), "4")));

            var response2 = await client.GetAsync("api/Transactions/School/test1");
            List<Transaction> transactions2 = await response2.Content.ReadAsAsync<List<Transaction>>();

            Assert.IsFalse(ContainsAnEqualTransaction(transactions2, new Transaction(1.00M, "test1", "test student 1", "test1", "food1", "test0", "test school0", new DateTime(2021, 07, 07), "1")));
            Assert.IsFalse(ContainsAnEqualTransaction(transactions2, new Transaction(2.00M, "test2", "test student 2", "test2", "food2", "test0", "test school0", new DateTime(2021, 07, 07), "2")));
            Assert.IsTrue(ContainsAnEqualTransaction(transactions2, new Transaction(1.00M, "test3", "test student 3", "test1", "food1", "test1", "test school1", new DateTime(2021, 07, 07), "3")));
            Assert.IsTrue(ContainsAnEqualTransaction(transactions2, new Transaction(2.00M, "test4", "test student 4", "test2", "food2", "test1", "test school1", new DateTime(2021, 07, 07), "4")));
        }

        /*commented out so other tests can run
        [TestMethod]
        public async void TestPutTransaction()
        {
            //case where the cost changes
            string jsonString = JsonSerializer.Serialize(new Transaction(5.00M, "test1", "test student 1", "test1", "food1", "test0", "test school0", new DateTime(2021, 07, 07), "1"));
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            //var response = await client.PutAsync("api/Transactions/1");

            //case where the studentID changes (when the studentID changes the controller assumes the student name and school if applicable are being updated in the same put request)

        }
        */

        [TestMethod]
        public async Task TestPostTransaction()
        {
            //post a transaction that doesn't already exist in the database
            string jsonString = JsonSerializer.Serialize(new Transaction(1.00M, "test5", "test student 5", "test3", "food3", "test3", "test school3", new DateTime(2021, 07, 07), "5"));
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/Transactions", httpContent);

            Assert.IsTrue(response.IsSuccessStatusCode);

            var studentResponse = await client.GetAsync("api/Students/test5");
            Student student5 = await studentResponse.Content.ReadAsAsync<Student>();
            Assert.AreEqual(9.00M, student5.Balance);

            var getResponse = await client.GetAsync("api/Transactions");
            List<Transaction> transactions = await getResponse.Content.ReadAsAsync<List<Transaction>>();
            Assert.IsTrue(ContainsAnEqualTransaction(transactions, new Transaction(1.00M, "test5", "test student 5", "test3", "food3", "test3", "test school3", new DateTime(2021, 07, 07), "5")));
        }

        [TestMethod]
        public async Task TestDeleteTransaction()
        {
            //delete existing transaction
            var response = await client.DeleteAsync("api/Transactions/1");
            Assert.AreEqual("NoContent", response.StatusCode);
            var student1Response = await client.GetAsync("api/Students/test1");
            Student student1 = await student1Response.Content.ReadAsAsync<Student>();
            Assert.AreEqual(10.00M, student1.Balance);

            var response2 = await client.DeleteAsync("api/Transactions/2");
            Assert.AreEqual("NoContent", response2.StatusCode);
            var student2Response = await client.GetAsync("api/Students/test2");
            Student student2 = await student2Response.Content.ReadAsAsync<Student>();
            Assert.AreEqual(10.00M, student2.Balance);

            var response3 = await client.DeleteAsync("api/Transactions/3");
            Assert.AreEqual("NoContent", response3.StatusCode);
            var student3Response = await client.GetAsync("api/Students/test3");
            Student student3 = await student3Response.Content.ReadAsAsync<Student>();
            Assert.AreEqual(10.00M, student3.Balance);

            var response4 = await client.DeleteAsync("api/Transactions/4");
            Assert.AreEqual("NoContent", response4.StatusCode);
            var student4Response = await client.GetAsync("api/Students/test4");
            Student student4 = await student4Response.Content.ReadAsAsync<Student>();
            Assert.AreEqual(10.00M, student4.Balance);

            //delete transaction that doesn't exist
            var response5 = await client.DeleteAsync("api/Transactions/1");
            Assert.AreEqual("NotFound", response5.StatusCode);
            var studentResponse = await client.GetAsync("api/Students/test1");
            Student student = await studentResponse.Content.ReadAsAsync<Student>();
            Assert.AreEqual(10.00M, student.Balance);
        }

    }
}
