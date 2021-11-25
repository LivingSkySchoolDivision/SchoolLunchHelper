using System;
using System.Collections.Generic;
using System.Linq;
using LSSD.MongoDB;

namespace LSSD.Lunch.StudentImporter
{
    public class Program
    {
        static string dbConnectionString = string.Empty;

        public static void Main(string[] args)
        {
            try {
                FileStream configFile = File.Open("config.json", FileMode.Open);
                if (configFile != null) {
                    ConfigFile config = System.Text.Json.JsonSerializer.Deserialize<ConfigFile>(configFile);

                    // For now just ingest "input.txt"

                    MongoDbConnection dbConnection = new MongoDbConnection(config.ConnectionString);

                    MongoRepository<Student> studentRepository = new MongoRepository<Student>(dbConnection);
                    MongoRepository<School> schoolRepository = new MongoRepository<School>(dbConnection);

                    List<School> allSchools = schoolRepository.GetAll().ToList();
                    List<Student> allStudents = studentRepository.GetAll().ToList();

                    // Put existing students in a dictionary for easier processing

                    Dictionary<string, Student> studentsByStudentNumber = allStudents.ToDictionary(x => x.StudentId);
                    List<Student> newStudentsNeeded = new List<Student>();


                    foreach(Student student in InputFileParser.ParseInputFile("input.txt"))
                    {
                        Console.WriteLine(student.StudentId + ": " + student.Name + " (" + student.HomeRoom + ")");
                    }

                    

                    // Get list of students from DB

                    // Find new students

                    // Find students to make inactive

                    // Find schools to add

                }
            }
            catch(Exception ex) {
                Console.WriteLine("ERROR: " + ex.Message);
            }


        }


    }
}