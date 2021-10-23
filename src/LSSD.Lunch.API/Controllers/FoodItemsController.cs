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
    public class FoodItemsController : ControllerBase
    {
        private readonly ILogger<FoodItemsController> _logger;
        private readonly FoodItemService _service;

        public FoodItemsController(ILogger<FoodItemsController> logger, FoodItemService service)
        {
            _logger = logger;
            _service = service;
        }
        
        [HttpGet]
        public IEnumerable<FoodItem> Get()
        {
            return _service.GetAll();
        }

        [HttpGet("{guid}")]
        public FoodItem Get(Guid guid)
        {
            return _service.Get(guid);
        }
    }
}
