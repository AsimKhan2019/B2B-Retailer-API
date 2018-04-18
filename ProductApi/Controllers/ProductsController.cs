using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Products")]
    public class ProductsController : Controller
    {
        private readonly IRepository<Product> repository;

        public ProductsController(IRepository<Product> repos)
        {
            repository = repos;
        }


        /// <summary>
        /// Returns all the products stored in the system 
        /// </summary>
        /// <returns>All the products in the system</returns>
        /// <response code="200">If the request has been done successfully</response>   
        // GET: api/products
        [ProducesResponseType(200)]
        [HttpGet]
        public IEnumerable<Product> Get()
        {
            return repository.GetAll();
        }

        /// <summary>
        /// Returns a product by its identifier
        /// </summary>
        /// <param name="id"> The identifier of the fetched product</param>
        /// <returns>A fetched product</returns>
        /// <response code="404">If the product is not found</response>
        /// <response code="200">If the request has been done successfully</response>   
        // GET api/products/5
        [HttpGet("{id}", Name = "GetProduct")]
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
        /// Creates a new product
        /// </summary>
        /// <remarks>
        /// api/products:
        ///
        ///     {
        ///          "id": 1,
        ///          "name": "Prudct Name",
        ///          "description": "Product Description",
        ///          "category": "Product Category",
        ///          "price": 20,
        ///          "itemsInStock": 2
        ///      }
        ///
        /// 
        /// </remarks>
        /// <param name="product"></param>
        /// <returns>A newly customer</returns>
        /// <response code="400">If all the parameters are not set correctly</response>
        /// <response code="201">If the product was created successfully</response>
        // POST api/products
        [HttpPost]
        [ProducesResponseType(400)]
        [ProducesResponseType(201)]
        public IActionResult Post([FromBody]Product product)
        {
            if (product == null)
            {
                return BadRequest();
            }

            var newProduct = repository.Add(product);

            return CreatedAtRoute("GetProduct", new { id = newProduct.Id }, newProduct);
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
        /// <param name="product"></param>
        /// <returns>A new product</returns>
        /// <response code="400">If all the parameters are not set correctly</response>
        /// <response code="404">If the product to update does not exist</response>
        /// <response code="200">If the request has been done successfully</response>   
         // PUT api/products/5
         [HttpPut("{id}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        // PUT api/products/5
        public IActionResult Put(int id, [FromBody]Product product)
        {
            if (product == null || product.Id != id)
            {
                return BadRequest();
            }

            var modifiedProduct = repository.Get(id);

            if (modifiedProduct == null)
            {
                return NotFound();
            }

            modifiedProduct.Name = product.Name;
            modifiedProduct.Price = product.Price;
            modifiedProduct.ItemsInStock = product.ItemsInStock;
            modifiedProduct.Description = product.Description;
            modifiedProduct.Category = product.Category;


            repository.Edit(modifiedProduct);
            return new NoContentResult();
        }

        /// <summary>
        /// Deletes an existing product
        /// </summary>
        /// <param name="id"> The identifier of the product to delete</param>
        /// <response code="404">If the product to delete does not exist</response>
        /// <response code="200">If the request has been done successfully</response>   
        // DELETE api/products/5
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
