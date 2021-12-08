using System;
using System.Collections.Generic;
using System.Linq;
using LSSD.MongoDB;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Reflection;

namespace LSSD.Lunch.StudentImporter
{
    public class Program
    {
        private static void handleParameters(string inputfile, string configfile, string onlyschoolsnamed, bool deactivate)
        {
            if (string.IsNullOrEmpty(inputfile)) {
                Console.WriteLine("Input file path required");
                return;
            }

            if (string.IsNullOrEmpty(configfile)) {
                Console.WriteLine("Config file path required");
                return;
            }

            try {
                ConfigFile configFile = parseConfigFile(configfile);
                Console.WriteLine("Config file loaded.");

                importStudents(configFile, inputfile, onlyschoolsnamed, deactivate);
            }
            catch(Exception ex) {
                Console.WriteLine("ERROR: " + ex.Message);
            }

        }

        private static ConfigFile parseConfigFile(string configFileName)
        {
            ConfigFile returnMe = new ConfigFile();

            using (FileStream configFile = File.Open(configFileName, FileMode.Open))
            {
                if (configFile != null) {
                    ConfigFile? config = System.Text.Json.JsonSerializer.Deserialize<ConfigFile>(configFile);
                    if (config != null) {
                        returnMe = config;
                    }
                }
            }

            return returnMe;
        }


        private static void importStudents(ConfigFile Config, string inputFileName, string onlyschoolsnamed, bool deactivateunlisted)
        {
            // Parse school filter list, if present
            List<String> filteredSchools = onlyschoolsnamed.Split(new char[] { ',', ';'}).ToList().Where(x => !string.IsNullOrEmpty(x)).Select(x => x.Trim()).ToList();
            if (filteredSchools.Count() > 0)
            {
                Console.WriteLine("Will ignore any students not in filtered school list:");
                foreach(string school in filteredSchools)
                {
                    Console.WriteLine(" > " + school);
                }
            }

            
            if (deactivateunlisted) {
                Console.WriteLine("Will deactivate students not in input file");
            } else {
                Console.WriteLine("Will NOT deactivate students not in input file. Use '-d true' to change this.");
            }

            MongoDbConnection dbConnection = new MongoDbConnection(Config.ConnectionString);
            MongoRepository<Student> studentRepository = new MongoRepository<Student>(dbConnection);
            MongoRepository<School> schoolRepository = new MongoRepository<School>(dbConnection);


            Console.WriteLine("Parsing input file...");

            List<Student> importedRows = InputFileParser.ParseInputFile(inputFileName);
            int rawRowCount = importedRows.Count();

            if (filteredSchools.Count() > 0)
            {
                importedRows = importedRows.Where(x => filteredSchools.Contains(x.SchoolName)).ToList();
            }

            Dictionary<string, Student> importStudentsByStudentNumber = importedRows.ToDictionary(x => x.StudentId);

            if (filteredSchools.Count() > 0)
            {
                Console.WriteLine("Found " + importStudentsByStudentNumber.Count() + " records to process (" + (rawRowCount - importedRows.Count)  + " ignored due to filters).");
            } else {
                Console.WriteLine("Found " + importStudentsByStudentNumber.Count() + " records to process.");
            }

            // Mark all students as active, since they're in the import file
            foreach(Student student in importStudentsByStudentNumber.Values)
            {
                student.IsActive = true;
            }


            Console.WriteLine("Loading required data from database...");
            Dictionary<string, Student> existingStudentsByStudentNumber = studentRepository.GetAll().ToDictionary(x => x.StudentId);

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

            if (deactivateunlisted) 
            {
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
            if (deactivateunlisted) 
            {
                Console.WriteLine("Will deactivate " + studentsToDeactivate.Count() + " students.");
            }
            Console.WriteLine("Writing student changes to database...");

            Console.WriteLine("Performating database writes..");
            studentRepository.Update(studentsWithUpdates);
            studentRepository.Update(studentsToDeactivate);

            Console.WriteLine("Done!");
        }


        public static void Main(string[] args)
        {
            // Set up command line parameters

            RootCommand rootCommand = new RootCommand(
                description: "Imports students into the LSSD Lunch database."
            ) {
                new Option<string>(new string[] { "--inputfile", "-inputfile", "-i"} , "The path to the input file."),
                new Option<string>(new string[] { "--configfile", "-configfile", "-c" }, "The path to the configuration file."),
                new Option<string>(new string[] { "--onlyschoolsnamed", "-onlyschoolsnamed", "-filter", "-f" }, "Ignore any students from schools not listed (comma delimited, don't use spaces)"),
                new Option<bool>(new string[] { "--deactivate", "-deactivate", "-d" }, "Whether to deactivate students not in input file, or leave them untouched (default: false).")
            };

            rootCommand.Handler = CommandHandler.Create<string, string, string, bool>(handleParameters);

            rootCommand.Invoke(args);
        }
    }
}