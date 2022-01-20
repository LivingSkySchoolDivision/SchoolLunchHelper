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
    public class AllActiveIDCardsController : ControllerBase
    {
        private readonly ILogger<AllActiveIDCardsController> _logger;
        private readonly StudentService _service;

        public AllActiveIDCardsController(ILogger<AllActiveIDCardsController> logger, StudentService service)
        {
            _logger = logger;
            _service = service;
        }


        [HttpGet]
        public IActionResult Get()
        {            
            List<Student> students = _service.GetAllActive().ToList();

            byte[] fileBytes = null;
            string downloadFilename = "lunchcards-" + DateTime.Today.ToLongDateString().Replace(" ", "").Replace("-","").Replace(",","") + ".docx";

            using (ReportFactory formFactory = new ReportFactory()) 
            {
                string filename = formFactory.GenerateStudentIDCardSheet(students);

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

            return NotFound();
            
        }
    }
}
