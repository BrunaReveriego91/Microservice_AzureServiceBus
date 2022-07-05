using Microservice_AzureServiceBus.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using System.Text;
using System.Text.Json;

namespace Microservice_AzureServiceBus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly string connectionString;

        public ProductsController(IConfiguration config)
        {
            _config = config;
            connectionString = this._config.GetValue<string>("AzureServiceBus");
        }

        [HttpPost]
        public async Task<IActionResult> Post(Product product)
        {
            await SendMessageQueue(product);
            return Ok(product);
        }

        private async Task SendMessageQueue(Product product)
        {
            string queueName = "product";
            var client = new QueueClient(connectionString, queueName, ReceiveMode.PeekLock);
            string messageBody = JsonSerializer.Serialize(product);

            var message = new Message(Encoding.UTF8.GetBytes(messageBody));

            await client.SendAsync(message);
            await client.CloseAsync();


        }
    }
}
