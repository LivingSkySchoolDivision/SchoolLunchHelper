using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LSSD.Lunch.Reports;
using LSSD.Lunch.WebManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LSSD.Lunch.WebManager.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TransactionLogController : ControllerBase
    {
        private readonly ILogger<TransactionLogController> _logger;
        private readonly StudentService _studentService;
        private readonly SchoolService _schoolService;
        private readonly TransactionService _transactionService;
        private IConfiguration _configuration;

        public TransactionLogController(ILogger<TransactionLogController> logger, StudentService studentService, SchoolService schoolService, TransactionService transactionService, IConfiguration configuration)
        {
            _logger = logger;
            _studentService = studentService;
            _schoolService = schoolService;
            _transactionService = transactionService;
            _configuration = configuration;
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
                string downloadFilename = "lunch-transaction-report-" + DateTime.Today.ToLongDateString().Replace(" ", "").Replace("-","").Replace(",","") + ".xlsx";

                using (ReportFactory formFactory = new ReportFactory()) 
                {
                    string filename = formFactory.GnerateTransactionLog(schoolTransactions, selectedSchool, _configuration["Settings:TimeZone"] ?? string.Empty);

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
