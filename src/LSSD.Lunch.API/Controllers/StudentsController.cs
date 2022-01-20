using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LSSD.Lunch.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LSSD.Lunch.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly ILogger<StudentsController> _logger;
        private readonly StudentService _service;

        public StudentsController(ILogger<StudentsController> logger, StudentService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpGet]
        public IEnumerable<Student> Get()
        {
            return _service.GetAll();
        }

        [HttpGet("{guid}")]
        public Student Get(Guid guid)
        {
            return _service.Get(guid);
        }
    }
}
