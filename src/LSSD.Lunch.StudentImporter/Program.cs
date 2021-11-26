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

                    Console.WriteLine("Loading required data from database...");
                    Dictionary<string, Student> existingStudentsByStudentNumber = studentRepository.GetAll().ToDictionary(x => x.StudentId);

                    Console.WriteLine("Parsing input file...");
                    Dictionary<string, Student> importStudentsByStudentNumber = InputFileParser.ParseInputFile("input.txt").ToDictionary(x => x.StudentId);


                    // Mark all students as active, since they're in the import file
                    foreach(Student student in importStudentsByStudentNumber.Values) 
                    {
                        student.IsActive = true;
                    }

                    Console.WriteLine("Finding schools to add...");
                    List<string> existingSchoolNames = schoolRepository.GetAll().Select(x => x.Name).ToList();
                    List<School> schoolsToAdd = new List<School>();
                    List<string> schoolNamesToAdd = new List<string>(); // Just for keeping track of what we're adding

                    foreach(KeyValuePair<string, Student> kvp in importStudentsByStudentNumber)
                    {
                        if (!string.IsNullOrEmpty(kvp.Value.SchoolName)) {
                            if (!existingSchoolNames.Contains(kvp.Value.SchoolName) && (!schoolNamesToAdd.Contains(kvp.Value.SchoolName)))
                            {
                                schoolsToAdd.Add(new School() { Name = kvp.Value.SchoolName });
                                schoolNamesToAdd.Add(kvp.Value.SchoolName);
                            }
                        }
                    }

                    if (schoolsToAdd.Count() > 0) {
                        Console.WriteLine("Adding " + schoolsToAdd.Count() + " schools...");
                        schoolRepository.Insert(schoolsToAdd);
                    }

                    Dictionary<string, School> existingSchoolsByName = schoolRepository.GetAll().ToDictionary(x => x.Name);

                    List<Student> newStudentsNeeded = new List<Student>();
                    List<Student> studentsToDeactivate = new List<Student>();
                    List<Student> updatedStudents = new List<Student>();

                    Console.WriteLine("Finding students to add or update...");
                    foreach(KeyValuePair<string, Student> kvp in importStudentsByStudentNumber)
                    {
                        if (!existingStudentsByStudentNumber.ContainsKey(kvp.Key))
                        {
                            newStudentsNeeded.Add(kvp.Value);
                        } else {
                            // See if we need to update anything
                            Student student = existingStudentsByStudentNumber[kvp.Key];
                            bool changes = false;

                            if (!student.HomeRoom.Equals(kvp.Value.HomeRoom))
                            {
                                student.HomeRoom = kvp.Value.HomeRoom;
                                changes = true;
                            }

                            if (!student.FirstName.Equals(kvp.Value.FirstName))
                            {
                                student.FirstName = kvp.Value.FirstName;
                                changes = true;
                            }

                            if (!student.LastName.Equals(kvp.Value.LastName))
                            {
                                student.LastName = kvp.Value.LastName;
                                changes = true;
                            }

                            if (!student.SchoolName.Equals(kvp.Value.SchoolName))
                            {
                                student.SchoolName = kvp.Value.SchoolName;
                                changes = true;
                            }

                            if (!student.IsActive == kvp.Value.IsActive)
                            {
                                student.IsActive = kvp.Value.IsActive;
                                changes = true;
                            }

                            if (changes == true)
                            {
                                updatedStudents.Add(student);
                            }
                        }
                    }

                    Console.WriteLine("Finding students to deactivate...");
                    foreach(KeyValuePair<string, Student> kvp in existingStudentsByStudentNumber)
                    {
                        if (!importStudentsByStudentNumber.ContainsKey(kvp.Key))
                        {
                            if (kvp.Value.IsActive) 
                            {
                                kvp.Value.IsActive = false;
                                studentsToDeactivate.Add(kvp.Value);
                            }
                        }
                    }


                    Console.WriteLine("Matching students to schools...");
                    List<Student> studentsWithUpdates = new List<Student>();
                    studentsWithUpdates.AddRange(newStudentsNeeded);
                    studentsWithUpdates.AddRange(updatedStudents);

                    foreach(Student student in studentsWithUpdates)
                    {
                        Guid expectedSchoolGUID = existingSchoolsByName[student.SchoolName].Id;
                        student.SchoolId = expectedSchoolGUID;
                    }

                    // Loop through students to match up school IDs for any missing them

                    Console.WriteLine("Will add " + newStudentsNeeded.Count() + " students.");
                    Console.WriteLine("Will update " + updatedStudents.Count() + " students.");
                    Console.WriteLine("Will deactivate " + studentsToDeactivate.Count() + " students.");
                    Console.WriteLine("Writing student changes to database...");

                    studentRepository.Update(studentsWithUpdates);                   

                    Console.WriteLine("Done!");
                }
            }
            catch(Exception ex) {
                Console.WriteLine("ERROR: " + ex.Message);
            }

        }
    }
}