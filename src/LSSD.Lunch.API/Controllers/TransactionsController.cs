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
    public class TransactionsController : ControllerBase
    {
        private readonly ILogger<TransactionsController> _logger;
        private readonly TransactionService _service;

        public TransactionsController(ILogger<TransactionsController> logger, TransactionService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpPost]
        public IActionResult Post(Transaction Transaction) 
        {
            try {
                _service.InsertOrUpdate(Transaction);
                return NoContent();
            }  
            catch {
                return BadRequest();
            }
        }

        [HttpGet]
        public IEnumerable<Transaction> Get()
        {
            return _service.GetAll();
        }

        [HttpGet("{guid}")]
        public Transaction Get(Guid guid)
        {
            return _service.Get(guid);
        }
    }
}
