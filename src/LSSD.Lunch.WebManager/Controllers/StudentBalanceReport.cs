using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LSSD.Lunch.Reports;
using LSSD.Lunch.WebManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LSSD.Lunch.WebManager.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StudentBalanceReport : ControllerBase
    {
        private readonly ILogger<StudentBalanceReport> _logger;
        private readonly StudentService _studentService;
        private readonly SchoolService _schoolService;
        private readonly TransactionService _transactionService;

        public StudentBalanceReport(ILogger<StudentBalanceReport> logger, StudentService studentService, SchoolService schoolService, TransactionService transactionService)
        {
            _logger = logger;
            _studentService = studentService;
            _schoolService = schoolService;
            _transactionService = transactionService;
        }

        [HttpGet("{schoolguid}")]
        public IActionResult Get(string schoolguid)
        {            

            // parse school id first. If it doesn't parse, dont bother with the rest
            
            School selectedSchool = _schoolService.Get(schoolguid);

            if (selectedSchool != null) {            
                List<Student> students = _studentService.GetAllForSchool(selectedSchool).ToList();
                List<Transaction> schoolTransactions = _transactionService.GetForStudents(students).ToList();

                byte[] fileBytes = null;
                string downloadFilename = "lunch-report-all-student-balances-" + DateTime.Today.ToLongDateString().Replace(" ", "").Replace("-","").Replace(",","") + ".xlsx";

                using (ReportFactory formFactory = new ReportFactory()) 
                {
                    string filename = formFactory.GenerateStudentBalanceReport(students, schoolTransactions, selectedSchool);

                    if (!string.IsNullOrEmpty(filename)) {
                        fileBytes = System.IO.File.ReadAllBytes(filename);
                    } else {
                        Console.WriteLine("Something went wrong, and the report could not be created.");
                    }            
                }  

                if (fileBytes != null) {
                    if (fileBytes.Length > 0) {
                        MemoryStream fileStream = new MemoryStream(fileBytes);
                        return File(fileStream, "application/octet-stream", downloadFilename);
                    } 
                }
            }

            return NotFound();
            
        }
    }
}
