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
    public class IndividualIDCardController : ControllerBase
    {
        private readonly ILogger<IndividualIDCardController> _logger;
        private readonly StudentService _service;

        public IndividualIDCardController(ILogger<IndividualIDCardController> logger, StudentService service)
        {
            _logger = logger;
            _service = service;
        }


        [HttpGet("{studentGUID}")]
        public IActionResult Get(string studentGUID)
        {
            // Look up the student
            
            Student selectedStudent = _service.Get(studentGUID);

            if (selectedStudent != null) {
                byte[] fileBytes = null;
                string downloadFilename = "lunchcard-" + selectedStudent.StudentId + "-" + selectedStudent.Name.ToLower().Replace(" ", "").Replace("'","").Replace(",","") + ".docx";

                using (ReportFactory formFactory = new ReportFactory()) 
                {
                    string filename = formFactory.GenerateStudentIDCardSheet(new List<Student>() { selectedStudent });

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


