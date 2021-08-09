using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using Data.Models;
using System.Text.Json;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Net.Http.Formatting;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Linq;

namespace TestAPI
{
    [TestClass]
    public class TransactionsControllerTests
    {
        private static HttpClient client;
        private static TestContext tc;

        [ClassInitialize]
        public static void ClassInit(TestContext tc)
        {
            TransactionsControllerTests.tc = tc;
            string path = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath;
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var ApiUri = configFile.AppSettings.Settings["ApiUri"].Value.ToString();
            client = new HttpClient();
            client.BaseAddress = new Uri(ApiUri);
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
            await client.DeleteAsync("api/Transactions/5");
            await client.DeleteAsync("api/Transactions/6");

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
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsTrue(ContainsAnEqualTransaction(transactions, new Transaction(1.00M, "test1", "test student 1", "test1", "food1", "test0", "test school0", new DateTime(2021, 07, 07), "1")));
            Assert.IsTrue(ContainsAnEqualTransaction(transactions, new Transaction(2.00M, "test2", "test student 2", "test2", "food2", "test0", "test school0", new DateTime(2021, 07, 07), "2")));
            Assert.IsTrue(ContainsAnEqualTransaction(transactions, new Transaction(1.00M, "test3", "test student 3", "test1", "food1", "test1", "test school1", new DateTime(2021, 07, 07), "3")));
            Assert.IsTrue(ContainsAnEqualTransaction(transactions, new Transaction(2.00M, "test4", "test student 4", "test2", "food2", "test1", "test school1", new DateTime(2021, 07, 07), "4")));

        }

        [TestMethod]
        public async Task TestGetTransaction()
        {
            //existing transaction case
            var response = await client.GetAsync("api/Transactions/1");
            Assert.IsTrue(response.IsSuccessStatusCode);
            Transaction transaction1 = await response.Content.ReadAsAsync<Transaction>();
            Assert.AreEqual(new Transaction(1.00M, "test1", "test student 1", "test1", "food1", "test0", "test school0", new DateTime(2021, 07, 07), "1").ToString(), transaction1.ToString());

            var response2 = await client.GetAsync("api/Transactions/3");
            Assert.IsTrue(response2.IsSuccessStatusCode);
            Transaction transaction2 = await response2.Content.ReadAsAsync<Transaction>();
            Assert.AreEqual(new Transaction(1.00M, "test3", "test student 3", "test1", "food1", "test1", "test school1", new DateTime(2021, 07, 07), "3").ToString(), transaction2.ToString());

            //transaction that doesn't exist
            var response3 = await client.GetAsync("api/Transactions/test1");
            Assert.IsFalse(response3.IsSuccessStatusCode);
            Assert.AreEqual("NotFound", response3.StatusCode.ToString());
        }

        [TestMethod]
        public async Task TestGetTransactionsBySchool()
        {
            //case where the school exists
            var response = await client.GetAsync("api/Transactions/School/test0");
            List<Transaction> transactions = await response.Content.ReadAsAsync<List<Transaction>>();
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsTrue(ContainsAnEqualTransaction(transactions, new Transaction(1.00M, "test1", "test student 1", "test1", "food1", "test0", "test school0", new DateTime(2021, 07, 07), "1")));
            Assert.IsTrue(ContainsAnEqualTransaction(transactions, new Transaction(2.00M, "test2", "test student 2", "test2", "food2", "test0", "test school0", new DateTime(2021, 07, 07), "2")));
            Assert.IsFalse(ContainsAnEqualTransaction(transactions, new Transaction(1.00M, "test3", "test student 3", "test1", "food1", "test1", "test school1", new DateTime(2021, 07, 07), "3")));
            Assert.IsFalse(ContainsAnEqualTransaction(transactions, new Transaction(2.00M, "test4", "test student 4", "test2", "food2", "test1", "test school1", new DateTime(2021, 07, 07), "4")));

            var response2 = await client.GetAsync("api/Transactions/School/test1");
            List<Transaction> transactions2 = await response2.Content.ReadAsAsync<List<Transaction>>();
            Assert.IsTrue(response2.IsSuccessStatusCode);
            Assert.IsFalse(ContainsAnEqualTransaction(transactions2, new Transaction(1.00M, "test1", "test student 1", "test1", "food1", "test0", "test school0", new DateTime(2021, 07, 07), "1")));
            Assert.IsFalse(ContainsAnEqualTransaction(transactions2, new Transaction(2.00M, "test2", "test student 2", "test2", "food2", "test0", "test school0", new DateTime(2021, 07, 07), "2")));
            Assert.IsTrue(ContainsAnEqualTransaction(transactions2, new Transaction(1.00M, "test3", "test student 3", "test1", "food1", "test1", "test school1", new DateTime(2021, 07, 07), "3")));
            Assert.IsTrue(ContainsAnEqualTransaction(transactions2, new Transaction(2.00M, "test4", "test student 4", "test2", "food2", "test1", "test school1", new DateTime(2021, 07, 07), "4")));

            //case where the school does not exist
            var response3 = await client.GetAsync("api/Transactions/School/aSchoolThatDoesNotExist");
            Assert.IsTrue(response3.IsSuccessStatusCode);
            List<Transaction> transactions3 = await response3.Content.ReadAsAsync<List<Transaction>>();
            Assert.IsFalse((transactions3?.Any()) == true);
        }

        [TestMethod]
        public async Task TestPutTransaction()
        {
            //case where the cost changes
            string jsonString = JsonSerializer.Serialize(new Transaction(5.00M, "test1", "test student 1", "test1", "food1", "test0", "test school0", new DateTime(2021, 07, 07), "1"));
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await client.PutAsync("api/Transactions/1", httpContent);
            Assert.AreEqual("NoContent", response.StatusCode.ToString());

            //make sure the transaction was changed properly in the database
            var getResponse = await client.GetAsync("api/Transactions/1");
            Transaction actualTransaction = await getResponse.Content.ReadAsAsync<Transaction>();
            tc.WriteLine(getResponse.StatusCode.ToString());
            Transaction expectedTransaction = new Transaction(5.00M, "test1", "test student 1", "test1", "food1", "test0", "test school0", new DateTime(2021, 07, 07), "1");
            Assert.AreEqual(expectedTransaction.ToString(), actualTransaction.ToString());

            //make sure the student's balance was updated
            var studentResponse = await client.GetAsync("api/Students/test1");
            Student student1 = await studentResponse.Content.ReadAsAsync<Student>();
            Assert.AreEqual(5.00M, student1.Balance);


            //case where the studentID and cost changes (when the studentID changes the controller assumes the student name and school if applicable are being updated in the same put request)
            string jsonString2 = JsonSerializer.Serialize(new Transaction(1.00M, "test3", "test student 3", "test2", "food2", "test1", "test school1", new DateTime(2021, 07, 07), "2"));
            var httpContent2 = new StringContent(jsonString2, Encoding.UTF8, "application/json");
            var response2 = await client.PutAsync("api/Transactions/2", httpContent2); 
            Assert.AreEqual("NoContent", response2.StatusCode.ToString());

            //make sure the transaction was changed properly in the database
            var getResponse2 = await client.GetAsync("api/Transactions/2");
            Transaction actualTransaction2 = await getResponse2.Content.ReadAsAsync<Transaction>();
            tc.WriteLine(getResponse.StatusCode.ToString());
            Transaction expectedTransaction2 = new Transaction(1.00M, "test3", "test student 3", "test2", "food2", "test1", "test school1", new DateTime(2021, 07, 07), "2");
            Assert.AreEqual(expectedTransaction2.ToString(), actualTransaction2.ToString());

            //make sure the students' balances were updated
            var studentResponse2 = await client.GetAsync("api/Students/test2");
            Student student2 = await studentResponse2.Content.ReadAsAsync<Student>();
            Assert.AreEqual(10.00M, student2.Balance);

            var studentResponse3 = await client.GetAsync("api/Students/test3");
            Student student3 = await studentResponse3.Content.ReadAsAsync<Student>();
            Assert.AreEqual(8.00M, student3.Balance);


            //case where the studentID changes but the new student doesn't exist
            string jsonString3 = JsonSerializer.Serialize(new Transaction(100.00M, "thisStudentDoesNotExist", "test student 4", "test2", "food8", "test1", "test school9", new DateTime(2021, 07, 07), "4"));
            var httpContent3 = new StringContent(jsonString3, Encoding.UTF8, "application/json");
            var response3 = await client.PutAsync("api/Transactions/4", httpContent3);
            Assert.AreEqual("BadRequest", response3.StatusCode.ToString());

            //make sure the transaction was not changed in the database
            var getResponse3 = await client.GetAsync("api/Transactions/4");
            Transaction actualTransaction3 = await getResponse3.Content.ReadAsAsync<Transaction>();
            tc.WriteLine(getResponse.StatusCode.ToString());
            Transaction expectedTransaction3 = new Transaction(2.00M, "test4", "test student 4", "test2", "food2", "test1", "test school1", new DateTime(2021, 07, 07), "4");
            Assert.AreEqual(expectedTransaction3.ToString(), actualTransaction3.ToString());


            //case where the transaction doesn't exist
            string jsonString5 = JsonSerializer.Serialize(new Transaction(2.00M, "test4", "test student 4", "test2", "food2", "test1", "test school1", new DateTime(2021, 07, 07), "thisTransactionDoesNotExist"));
            var httpContent5 = new StringContent(jsonString5, Encoding.UTF8, "application/json");
            var response5 = await client.PutAsync("api/Transactions/thisTransactionDoesNotExist", httpContent5);
            Assert.AreEqual("NotFound", response5.StatusCode.ToString());

            //make sure the student's balance wasn't changed
            var studentResponse4 = await client.GetAsync("api/Students/test4");
            Student student4 = await studentResponse4.Content.ReadAsAsync<Student>();
            Assert.AreEqual(8.00M, student4.Balance);

            //make sure the transaction was not added to the database
            var getResponse4 = await client.GetAsync("api/Transactions/thisTransactionDoesNotExist");
            Assert.AreEqual("NotFound", getResponse4.StatusCode.ToString());


        }

        [TestMethod]
        public async Task TestPostTransaction()
        {
            //post a transaction that doesn't already exist in the database
            string jsonString = JsonSerializer.Serialize(new Transaction(1.00M, "test5", "test student 5", "test3", "food3", "test3", "test school3", new DateTime(2021, 07, 07), "5"));
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/Transactions", httpContent);
            //sure post returns a success response
            Assert.IsTrue(response.IsSuccessStatusCode);
            //make sure the student's balance was updated correctly
            var studentResponse = await client.GetAsync("api/Students/test5");
            Student student5 = await studentResponse.Content.ReadAsAsync<Student>();
            Assert.AreEqual(9.00M, student5.Balance);
            //make sure the transaction is in the database
            var getResponse = await client.GetAsync("api/Transactions");
            List<Transaction> transactions = await getResponse.Content.ReadAsAsync<List<Transaction>>();
            Assert.IsTrue(ContainsAnEqualTransaction(transactions, new Transaction(1.00M, "test5", "test student 5", "test3", "food3", "test3", "test school3", new DateTime(2021, 07, 07), "5")));

            //post a transaction that already exists in the database
            string jsonString2 = JsonSerializer.Serialize(new Transaction(1.00M, "test5", "test student 5", "test3", "food3", "test3", "test school3", new DateTime(2021, 07, 07), "5"));
            var httpContent2 = new StringContent(jsonString2, Encoding.UTF8, "application/json");
            var response2 = await client.PostAsync("api/Transactions", httpContent2);
            //make sure the response is Conflict
            Assert.AreEqual("Conflict", response2.StatusCode.ToString());

            //post a transaction with a negative cost
            string jsonString3 = JsonSerializer.Serialize(new Transaction(-1.00M, "test1", "test student 1", "test3", "food3", "test0", "test school0", new DateTime(2021, 07, 07), "6"));
            var httpContent3 = new StringContent(jsonString3, Encoding.UTF8, "application/json");
            var response3 = await client.PostAsync("api/Transactions", httpContent3);
            //make sure post returns a success response
            Assert.IsTrue(response3.IsSuccessStatusCode);
            //make sure the student's balance was updated correctly
            var studentResponse3 = await client.GetAsync("api/Students/test1");
            Student student1 = await studentResponse3.Content.ReadAsAsync<Student>();
            Assert.AreEqual(10.00M, student1.Balance);
            //make sure the transaction is in the database
            var getResponse2 = await client.GetAsync("api/Transactions");
            List<Transaction> transactions2 = await getResponse2.Content.ReadAsAsync<List<Transaction>>();
            Assert.IsTrue(ContainsAnEqualTransaction(transactions2, new Transaction(-1.00M, "test1", "test student 1", "test3", "food3", "test0", "test school0", new DateTime(2021, 07, 07), "6")));
        }

        [TestMethod]
        public async Task TestDeleteTransaction()
        {
            //delete existing transaction
            var response = await client.DeleteAsync("api/Transactions/1");
            Assert.AreEqual("NoContent", response.StatusCode.ToString());
            var student1Response = await client.GetAsync("api/Students/test1");
            Student student1 = await student1Response.Content.ReadAsAsync<Student>();
            Assert.AreEqual(10.00M, student1.Balance);

            var response2 = await client.DeleteAsync("api/Transactions/2");
            Assert.AreEqual("NoContent", response2.StatusCode.ToString());
            var student2Response = await client.GetAsync("api/Students/test2");
            Student student2 = await student2Response.Content.ReadAsAsync<Student>();
            Assert.AreEqual(10.00M, student2.Balance);

            var response3 = await client.DeleteAsync("api/Transactions/3");
            Assert.AreEqual("NoContent", response3.StatusCode.ToString());
            var student3Response = await client.GetAsync("api/Students/test3");
            Student student3 = await student3Response.Content.ReadAsAsync<Student>();
            Assert.AreEqual(10.00M, student3.Balance);

            var response4 = await client.DeleteAsync("api/Transactions/4");
            Assert.AreEqual("NoContent", response4.StatusCode.ToString());
            var student4Response = await client.GetAsync("api/Students/test4");
            Student student4 = await student4Response.Content.ReadAsAsync<Student>();
            Assert.AreEqual(10.00M, student4.Balance);

            //delete transaction that doesn't exist
            var response5 = await client.DeleteAsync("api/Transactions/1");
            Assert.AreEqual("NotFound", response5.StatusCode.ToString());
            var studentResponse = await client.GetAsync("api/Students/test1");
            Student student = await studentResponse.Content.ReadAsAsync<Student>();
            Assert.AreEqual(10.00M, student.Balance);
        }

    }
}