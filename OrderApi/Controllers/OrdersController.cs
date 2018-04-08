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
    [Route("api/Orders")]
    public class OrdersController : Controller
    {
        private readonly IRepository<Order> repository;
        private readonly string URL_PRODUCT_API = "http://localhost:55556/api/products/";
        //private readonly string URL_PRODUCT_API = "http://productapi/api/products/";
        private readonly string URL_CUSTOMER_API = "http://localhost:3115/api/customers/";
        //private readonly string URL_CUSTOMER_API = "http://customerapi/api/products/";

        public OrdersController(IRepository<Order> repos)
        {
            repository = repos;
        }

        // GET: api/orders
        [HttpGet]
        public IEnumerable<Order> Get()
        {
            return repository.GetAll();
        }

        // GET api/orders/5
        [HttpGet("{id}", Name = "GetOrder")]
        public IActionResult Get(int id)
        {
            var item = repository.Get(id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        // POST api/orders
        [HttpPost]
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

        // DELETE api/orders/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (repository.Get(id) == null)
            {
                return NotFound();
            }

            repository.Remove(id);
            return new NoContentResult();
        }

        // GET: api/orders/customer/ordersFromCustomer
        [HttpHead("{CustomerId}", Name = "GetOrdersFromCustomer")]
        public IEnumerable<Order> GetOrdersFromCustomer(int CustomerId)
        {
            return repository.GetAllFromCustomer(CustomerId);
        }


        // PUT api/orders/5
        [HttpPut("{id}")]
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

    }
}
