using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CustomerApi.Models;
using CustomerApi.Data;

namespace CustomerApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Customers")]
    public class CustomerController : Controller
    {

        private readonly IRepository<Customer> repository;

        public CustomerController(IRepository<Customer> repos)
        {
            repository = repos;
        }

        /// <summary>
        /// Returns all the customers stored in the system 
        /// </summary>
        /// <returns>All the customers in the system</returns>
        /// <response code="200">If the request has been done successfully</response>   
        // GET: api/customers
        [ProducesResponseType(200)]
        [HttpGet]
        public IEnumerable<Customer> Get()
        {
            return repository.GetAll();
        }


        /// <summary>
        /// Returns a customer by its identifier
        /// </summary>
        /// <param name="id"> The identifier of the fetched customer</param>
        /// <returns>A fetched customer</returns>
        /// <response code="404">If the item is not found</response>
        /// <response code="200">If the request has been done successfully</response>   
        // GET api/customers/5
        [HttpGet("{id}", Name = "GetCustomer")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public IActionResult Get(int id)
        {
            var item = repository.Get(id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        /// <summary>
        /// Creates a new customer
        /// </summary>
        /// <remarks>
        /// api/customers:
        ///
        ///     {
        ///        "companyName": "Customers SA",
        ///        "email": "samplemail@easv.dk",
        ///        "phone": "1234 5682",
        ///        "billingAddress": "Sample Address 34 B, 5343",
        ///        "shippingAddress": "Sample Address 34 B, 5343"
        ///     }
        ///
        /// 
        /// </remarks>
        /// <param name="customer"></param>
        /// <returns>A newly customer</returns>
        /// <response code="400">If all the parameters are not set correctly</response>
        /// <response code="201">If the customer was created successfully</response>
        // POST api/customers
        [HttpPost]
        [ProducesResponseType(400)]
        [ProducesResponseType(201)]
        public IActionResult Post([FromBody]Customer customer)
        {
           if (customer == null)
           {
              return BadRequest();
           }

          var newCustomer = repository.Add(customer);

          return CreatedAtRoute("GetCustomer", new { id = newCustomer.Id }, newCustomer);
        }

        /// <summary>
        /// Updates an existing customer
        /// </summary>
        /// <param name="id"> The identifier of the customer to update</param>
        /// <remarks>
        /// api/customers:
        ///
        ///     {
        ///        "id": 1,
        ///        "companyName": "Customers SA",
        ///        "email": "samplemailupdated@easv.dk",
        ///        "phone": "1234 4542",
        ///        "billingAddress": "Sample Address updated 34 B, 5343",
        ///        "shippingAddress": "Sample Address updated 34 B, 5343"
        ///     }
        ///
        /// 
        /// </remarks>
        /// <param name="customer"></param>
        /// <returns>A newly customer</returns>
        /// <response code="400">If all the parameters are not set correctly</response>
        /// <response code="404">If the customer to update does not exist</response>
        /// <response code="200">If the request has been done successfully</response>   
        // PUT api/customers/5
        [HttpPut("{id}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        public IActionResult Put(int id, [FromBody]Customer customer)
        {
            if (customer == null || customer.Id != id)
            {
                return BadRequest();
            }

            var modifiedCustomer = repository.Get(id);

            if (modifiedCustomer == null)
            {
                return NotFound();
            }

            modifiedCustomer.Email = customer.Email;
            modifiedCustomer.Phone = customer.Phone;
            modifiedCustomer.BillingAddress = customer.BillingAddress;
            modifiedCustomer.ShippingAddress = customer.ShippingAddress;

            repository.Edit(modifiedCustomer);
            return new NoContentResult();
        }

        /// <summary>
        /// Deletes an existing customer
        /// </summary>
        /// <param name="id"> The identifier of the customer to delete</param>
        /// <response code="404">If the customer to delete does not exist</response>
        /// <response code="200">If the request has been done successfully</response>   
        // DELETE api/customers/5
        [HttpDelete("{id}")]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        public IActionResult Delete(int id)
        {
            if (repository.Get(id) == null)
            {
                return NotFound();
            }

            repository.Remove(id);
            return new NoContentResult();
        }
    }
}