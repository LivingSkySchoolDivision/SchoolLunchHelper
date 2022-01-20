using System;

namespace LSSD.Lunch.StudentImporter {

    public static class InputFileParser
    {
        private const string delimiter = "|";

        public static List<Student> ParseInputFile(string filePath)
        {
            List<Student> returnMe = new List<Student>();

            foreach(string line in System.IO.File.ReadLines(filePath))
            {
                Student parsed = parseLine(line);
                if (parsed != null) {
                    if (!string.IsNullOrEmpty(parsed.StudentId)) // Ignore students with empty student IDs
                    {
                        returnMe.Add(parsed);
                    }
                }
            }

            return returnMe;
        }


        private static Student parseLine(string line) 
        {
            // Skip the header line
            if (line.Contains("School|StudentID|FirstName|LastName|HomeRoom")) 
            {
                return null;
            }
            
            try {
                string[] parsedline = line.Split(delimiter);
                return new Student() {
                    SchoolName = parsedline[0],
                    StudentId = parsedline[1],
                    FirstName = parsedline[2],
                    LastName = parsedline[3],
                    HomeRoom = parsedline[4]                    
                };
            }
            catch {}

            return null;            
        }
        
    }

}