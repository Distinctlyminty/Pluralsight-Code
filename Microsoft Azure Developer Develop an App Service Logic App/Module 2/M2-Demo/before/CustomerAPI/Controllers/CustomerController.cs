using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomersController : ControllerBase
    {
        
        private readonly ILogger<CustomersController> _logger;
        private readonly CustomerManager _customers = CustomerManager.Instance;


        public CustomersController(ILogger<CustomersController> logger)
        {
            _logger = logger;
        }

     
    }
}
