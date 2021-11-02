using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace LSSD.Lunch.Reports
{
    public class ReportFactory : IDisposable
    {
        List<string> _generatedFileNames = new List<string>();
        string _tempDirPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        public ReportFactory(string TempFolderPath) {
            this._tempDirPath = TempFolderPath;
            createTempDirectory();
        }

        public ReportFactory() {
            createTempDirectory();
        }

        void createTempDirectory() {
            if (!Directory.Exists(_tempDirPath)) {
                Directory.CreateDirectory(_tempDirPath);
            }
            Console.WriteLine("Temp directory is: " + _tempDirPath);
        }


        public string GenerateStudentIDCardSheet(List<Student> Students)
        {
            // Perhaps make a better way of naming the files
            // for now random will do though
            string filename = Path.Combine(_tempDirPath, (Guid.NewGuid()).ToString() + ".docx");

            StudentIDCardSheet generator = new StudentIDCardSheet();

            // Generate the file
            generator.Generate(Students, filename);

            // Store the filename in the tracking list
            _generatedFileNames.Add(filename);
            
            // Return the filename
            return filename;
        }

        public void DeleteTempFiles() {
            List<string> deletedFiles = new List<string>();

            foreach(string filename in _generatedFileNames) {
                //try {
                    File.Delete(filename);
                    deletedFiles.Add(filename);
                //} catch {}
            }

            foreach(string filename in deletedFiles) {
                _generatedFileNames.Remove(filename);
            }
        }

        public void Dispose()
        {
            this.DeleteTempFiles();

            if (Directory.Exists(_tempDirPath)) {
                Directory.Delete(_tempDirPath);
            }
        }

        public static string SanitizeFilename(string input) {
            return Regex.Replace(input, "[^A-Za-z0-9-]", ""); 
        }
    }
}