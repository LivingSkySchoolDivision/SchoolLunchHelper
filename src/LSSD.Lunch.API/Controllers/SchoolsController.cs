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
    public class SchoolsController : ControllerBase
    {
        private readonly ILogger<SchoolsController> _logger;
        private readonly SchoolService _service;

        public SchoolsController(ILogger<SchoolsController> logger, SchoolService service)
        {
            _logger = logger;
            _service = service;
        }
        
        [HttpPost]
        public IActionResult Post(School school) 
        {
            try {
                _service.InsertOrUpdate(school);
                return NoContent();
            }  
            catch {
                return BadRequest();
            }
        }

        [HttpGet]
        public IEnumerable<School> Get()
        {
            return _service.GetAll();
        }

        [HttpGet("{guid}")]
        public School Get(Guid guid)
        {
            return _service.Get(guid);
        }
    }
}
