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

      /// <summary>
      /// Get all customers
      /// </summary>
      /// <returns>200</returns>
        [HttpGet, Route("GetAllCustomers", Name = "GetAllCustomers")]
        public IEnumerable<Customer> Get()
        {
            return _customers.GetCustomers();
        }

        /// <summary>
        /// Get a customer by id
        /// </summary>
        /// <returns>200</returns>
        [HttpGet, Route("customers/{id}", Name = "GetCustomer")]
        public Customer GetCustomer(string id)
        {
            return _customers.GetCustomerById(id);
        }

        /// <summary>
        /// Create a new customer
        /// </summary>
        /// <returns>200</returns>
        [HttpPost, Route("CreateCustomer", Name = "CreateCustomer")]
        public IActionResult CreateCustomer([FromBody] Customer customer)
        {

            _customers.AddBook(customer);

            return CreatedAtRoute("GetCustomer", new { id = customer.CustomerId }, customer);
        }

        /// <summary>
        /// Update a customer
        /// </summary>
        /// <returns>200</returns>
        [HttpPut, Route("customers/{id}",Name = "UpdateCustomer")]
        public void UpdateCustomer([FromBody] Customer customer)
        {
            _customers.UpdateCustomer(customer);
        }

        /// <summary>
        /// Delete a customer by id
        /// </summary>
        /// <returns>200</returns>
        [HttpDelete, Route("customers/{id}", Name = "DeleteCustomer")]
        public void DeleteCustomer(string id)
        {
            _customers.DeleteCustomer(id);
        }
    }
}
