using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Data;
using OrderApi.Models;
using RestSharp;

namespace OrderApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Orders")]
    public class OrdersController : Controller
    {
        private readonly IRepository<Order> repository;
        private readonly string URL_PRODUCT_API = "http://localhost:55556/api/products/";
       // private readonly string URL_PRODUCT_API = "http://productapi/api/products/";
        private readonly string URL_CUSTOMER_API = "http://localhost:3115/api/customers/";
       // private readonly string URL_CUSTOMER_API = "http://customerapi/api/customers/";

        public OrdersController(IRepository<Order> repos)
        {
            repository = repos;
        }

        /// <summary>
        /// Returns all the orders stored in the system 
        /// </summary>
        /// <returns>All the orders in the system</returns>
        /// <response code="200">If the request has been done successfully</response>   
        // GET: api/orders
        [ProducesResponseType(200)]
        [HttpGet]
        public IEnumerable<Order> Get()
        {
            return repository.GetAll();
        }

        /// <summary>
        /// Returns a order by its identifier
        /// </summary>
        /// <param name="id"> The identifier of the fetched order</param>
        /// <returns>A fetched order</returns>
        /// <response code="404">If the order is not found</response>
        /// <response code="200">If the request has been done successfully</response>   
         // GET api/orders/5
        [HttpGet("{id}", Name = "GetOrder")]
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
        /// Creates a new order
        /// </summary>
        /// <remarks>
        /// api/orders:
        ///
        ///     {
        ///         "id": 1,
        ///         "customerId": 1,
        ///         "date": "2018-04-18T21:50:47.443Z",
        ///         "status": "shipped",
        ///         "shippingCharge": 0,
        ///         "estimatedDeliveryDate": "2018-04-18T21:50:47.444Z",
        ///         "productId": 1,
        ///         "quantity": 2
        ///     }
        ///
        /// 
        /// </remarks>
        /// <param name="order"></param>
        /// <returns>A newly order</returns>
        /// <response code="400">If all the parameters are not set correctly</response>
        /// <response code="201">If the order was created successfully</response>
        // POST api/orders
        [HttpPost]
        [ProducesResponseType(400)]
        [ProducesResponseType(201)]
        public IActionResult Post([FromBody]Order order)
        {
            if (order == null)
            {
                return BadRequest();
            }

            // Call ProductApi to get the product ordered
            RestClient p = new RestClient();
            // You may need to change the port number in the BaseUrl below
            // before you can run the request.
            p.BaseUrl = new Uri(URL_PRODUCT_API);
            var request = new RestRequest(order.ProductId.ToString(), Method.GET);
            var response = p.Execute<Product>(request);
            var orderedProduct = response.Data;

            // Call CustomerApi to get the customer who has requested the order
            RestClient c = new RestClient();
            // You may need to change the port number in the BaseUrl below
            // before you can run the request.
            c.BaseUrl = new Uri(URL_CUSTOMER_API);
            var requestCustomer = new RestRequest(order.CustomerId.ToString(), Method.GET);
            var responseCustomer = c.Execute<Customer>(requestCustomer);
           
            // If the customer does not exist yet, the order cannot be created. 
            if (responseCustomer.Data == null)
            {
                return NotFound();
            }
            // Check if customer has credit standing
            if (!hasCreditStanding(order.CustomerId))
            {
                return Unauthorized();
            }

            // Check if there are enough items of that product.
            if (order.Quantity <= orderedProduct.ItemsInStock)
            {
                order.Status = "requested";
                order.ShippingCharge = calculateShippingCharge();
                order.EstimatedDeliveryDate = calculateEstimatedDeliveryDate();

                var newOrder = repository.Add(order);
                return CreatedAtRoute("GetOrder", new { id = newOrder.Id }, newOrder);
            }

            // If the order could not be created, "return no content".
            return NoContent();
        }

        /// <summary>
        /// Deletes an existing order
        /// </summary>
        /// <param name="id"> The identifier of the order to delete</param>
        /// <response code="404">If the order to delete does not exist</response>
        /// <response code="200">If the request has been done successfully</response>   
        // DELETE api/orders/5
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


        /// <summary>
        /// Returns a order by its identifier
        /// </summary>
        /// <param name="customerId"> The identifier of the fetched Customer</param>
        /// <returns>A fetched Customer</returns>
        /// <response code="404">If the CustomerId is not found</response>
        /// <response code="200">If the request has been done successfully</response>   
        // GET: api/orders/customer/ordersFromCustomer
        [HttpHead("{CustomerId}", Name = "GetOrdersFromCustomer")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public IEnumerable<Order> GetOrdersFromCustomer(int CustomerId)
        {
            return repository.GetAllFromCustomer(CustomerId);
        }


        /// <summary>
        /// Updates an existing order
        /// </summary>
        /// <param name="id"> The identifier of the order to update</param>
        /// <remarks>
        /// api/orders:
        ///
        ///     {
        ///         "id": 1,
        ///         "customerId": 1,
        ///         "date": "2018-04-18T21:50:47.443Z",
        ///         "status": "shipped",
        ///         "shippingCharge": 0,
        ///         "estimatedDeliveryDate": "2018-04-18T21:50:47.444Z",
        ///         "productId": 1,
        ///         "quantity": 2
        ///     }
        ///
        /// 
        /// </remarks>
        /// <param name="order"></param>
        /// <returns>A new order</returns>
        /// <response code="400">If all the parameters are not set correctly</response>
        /// <response code="404">If the order to update does not exist</response>
        /// <response code="200">If the request has been done successfully</response>   
        // PUT api/orders/5
        [HttpPut("{id}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        public IActionResult Put(int id, [FromBody]Order order)
        {
            if (order == null || order.Id != id)
            {
                return BadRequest();
            }

            var modifiedOrder = repository.Get(id);

            if (modifiedOrder == null)
            {
                return NotFound();
            }

            modifiedOrder.Status = order.Status;
            if (order.Status.ToLower() == "shipped")
            {
                // Call ProductApi to get the product ordered
                RestClient c = new RestClient();
                // You may need to change the port number in the BaseUrl below
                // before you can run the request.
                c.BaseUrl = new Uri(URL_PRODUCT_API);
                var request = new RestRequest(order.ProductId.ToString(), Method.GET);
                var response = c.Execute<Product>(request);
                var orderedProduct = response.Data;

                if (order.Quantity <= orderedProduct.ItemsInStock)
                {
                    // reduce the number of items in stock for the ordered product,
                    // and create a new order.
                    orderedProduct.ItemsInStock -= order.Quantity;
                    var updateRequest = new RestRequest(orderedProduct.Id.ToString(), Method.PUT);
                    updateRequest.AddJsonBody(orderedProduct);
                    var updateResponse = c.Execute(updateRequest);
                }
            }
            repository.Edit(modifiedOrder);

            return new NoContentResult();
        }

        private decimal calculateShippingCharge()
        {
            return 6m;
        }

        private DateTime calculateEstimatedDeliveryDate()
        {
            return DateTime.Today.AddDays(5d);
        }

        private bool hasCreditStanding(int customerId)
        {
            List<Order> orders = GetOrdersFromCustomer(customerId).ToList();
            return !orders.Exists(o => o.Status == "requested");
        }

    }
}
