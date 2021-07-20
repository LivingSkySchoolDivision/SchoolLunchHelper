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
            //string path = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath;
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var ApiUri = configFile.AppSettings.Settings["ApiUri"].Value.ToString();
            client = new HttpClient();
            client.BaseAddress = new Uri(ApiUri);
        }

        [TestInitialize]
        public async Task InitializeData()
        {
            await PostData(new Student("1234test", "test student 1", "test1", 10.00M, "medical info"));
            await PostData(new Student("5678test", "test student 2", "test2", 20.00M, "medical info 2"));
        }

        [TestCleanup]
        public async Task CleanUpData()
        {
            await client.DeleteAsync("api/Students/1234test");
            await client.DeleteAsync("api/Students/5678test");
            await client.DeleteAsync("api/Students/4321test");
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
                if (StudentsAreEqual(i, value))
                {
                    return true;
                }
            }
            return false;
        }

        private bool StudentsAreEqual(Student student1, Student student2)
        {
            if ((student1.StudentID == student2.StudentID) && (student1.Name == student2.Name) && (student1.SchoolID == student2.SchoolID) && (student1.Balance == student2.Balance) && (student1.MedicalInfo == student2.MedicalInfo))
            {
                return true;
            }
            return false;
        }

        [TestMethod]
        public async Task TestGetStudents()
        {
            var response = await client.GetAsync("api/Students");
            List<Student> students = await response.Content.ReadAsAsync<List<Student>>();
            Assert.IsTrue(response.IsSuccessStatusCode);

            Assert.IsTrue(ContainsAnEqualStudent(students, new Student("1234test", "test student 1", "test1", 10.00M, "medical info")));
            Assert.IsTrue(ContainsAnEqualStudent(students, new Student("5678test", "test student 2", "test2", 20.00M, "medical info 2")));
        }

        [TestMethod]
        public async Task TestGetStudent()
        {
            //get student that exists
            var response = await client.GetAsync("api/Students/1234test");
            Student student1 = await response.Content.ReadAsAsync<Student>();
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsTrue(StudentsAreEqual(new Student("1234test", "test student 1", "test1", 10.00M, "medical info"), student1));

            var response2 = await client.GetAsync("api/Students/5678test");
            Student student2 = await response2.Content.ReadAsAsync<Student>();
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsTrue(StudentsAreEqual(new Student("5678test", "test student 2", "test2", 20.00M, "medical info 2"), student2));

            //get student that does not exist
            var response3 = await client.GetAsync("api/Students/thisStudentDoesNotExist");
            Assert.IsFalse(response3.IsSuccessStatusCode);
            Assert.AreEqual("NotFound", response3.StatusCode.ToString());
        }

        [TestMethod]
        public async Task TestGetStudentsBySchool()
        {
            //case where school exists
            var response = await client.GetAsync("api/Students/School/test1");
            List<Student> students = await response.Content.ReadAsAsync<List<Student>>();
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsTrue(ContainsAnEqualStudent(students, new Student("1234test", "test student 1", "test1", 10.00M, "medical info")));
            Assert.IsFalse(ContainsAnEqualStudent(students, new Student("5678test", "test student 2", "test2", 20.00M, "medical info 2")));

            var response2 = await client.GetAsync("api/Students/School/test2");
            List<Student> students2 = await response2.Content.ReadAsAsync<List<Student>>();
            Assert.IsTrue(response2.IsSuccessStatusCode);
            Assert.IsFalse(ContainsAnEqualStudent(students2, new Student("1234test", "test student 1", "test1", 10.00M, "medical info")));
            Assert.IsTrue(ContainsAnEqualStudent(students2, new Student("5678test", "test student 2", "test2", 20.00M, "medical info 2")));

            //case where school does not exist
            var response3 = await client.GetAsync("api/Students/School/aSchoolThatDoesNotExist");
            Assert.IsTrue(response3.IsSuccessStatusCode);
            List<Student> students3 = await response3.Content.ReadAsAsync<List<Student>>();
            Assert.IsFalse((students3?.Any()) == true);
        }

        [TestMethod]
        public async Task TestPutStudent()
        {
            //case where the student exists
            string jsonString = JsonSerializer.Serialize(new Student("1234test", "test student 5", "test3", 20.00M, "medical info 1"));
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await client.PutAsync("api/Students/1234test", httpContent);
            Assert.AreEqual("NoContent", response.StatusCode.ToString());

            var response2 = await client.GetAsync("api/Students");
            List<Student> students = await response2.Content.ReadAsAsync<List<Student>>();
            Assert.IsTrue(response2.IsSuccessStatusCode);
            Assert.IsTrue(ContainsAnEqualStudent(students, new Student("1234test", "test student 5", "test3", 20.00M, "medical info 1")));

            //case where the URL doesn't match the student's ID
            string jsonString2 = JsonSerializer.Serialize(new Student("1234test", "test student 5", "test3", 20.00M, "medical info 1"));
            var httpContent2 = new StringContent(jsonString2, Encoding.UTF8, "application/json");
            var response3 = await client.PutAsync("api/Students/1", httpContent2);
            Assert.AreEqual("BadRequest", response3.StatusCode.ToString());

            //case where the student does not exist
            string jsonString3 = JsonSerializer.Serialize(new Student("thisStudentDoesNotExist", "test student 0", "test3", 20.00M, "medical info 1"));
            var httpContent3 = new StringContent(jsonString3, Encoding.UTF8, "application/json");
            var response4 = await client.PutAsync("api/Students/thisStudentDoesNotExist", httpContent3);
            Assert.AreEqual("NotFound", response4.StatusCode.ToString());

        }

        [TestMethod]
        public async Task TestPostStudent()
        {
            //post a student that doesn't already exist
            string jsonString = JsonSerializer.Serialize(new Student("4321test", "test student 3", "test3", 20.00M, "medical info"));
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/Students", httpContent);
            Assert.IsTrue(response.IsSuccessStatusCode);

            var response2 = await client.GetAsync("api/Students");
            List<Student> students = await response2.Content.ReadAsAsync<List<Student>>();
            Assert.IsTrue(response2.IsSuccessStatusCode);
            Assert.IsTrue(ContainsAnEqualStudent(students, new Student("4321test", "test student 3", "test3", 20.00M, "medical info")));

            //post a student that already exists
            string jsonString2 = JsonSerializer.Serialize(new Student("4321test", "test student 3", "test3", 20.00M, "medical info"));
            var httpContent2 = new StringContent(jsonString2, Encoding.UTF8, "application/json");
            var response3 = await client.PostAsync("api/Students", httpContent2);
            Assert.AreEqual("Conflict", response3.StatusCode.ToString());
        }

        [TestMethod]
        public async Task TestDeleteStudent()
        {
            //delete an existing student
            var response = await client.DeleteAsync("api/Students/1234test");
            Assert.AreEqual("NoContent", response.StatusCode.ToString());
            var getResponse = await client.GetAsync("api/Students/1234test");
            Assert.AreEqual("NotFound", getResponse.StatusCode.ToString());

            //delete a student that does not exist
            var response2 = await client.DeleteAsync("api/Students/1234test");
            Assert.AreEqual("NotFound", response2.StatusCode.ToString());
        }

    }
}
