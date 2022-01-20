using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace LSSD.Lunch.WebManager.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PingController : ControllerBase
    {

        // GET: api/<GeneralController>
        [HttpGet]
        public IActionResult Get()
        {
            return new JsonResult(new { ping = "pong" }, new System.Text.Json.JsonSerializerOptions()
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = true
                });
        }



    }
}
