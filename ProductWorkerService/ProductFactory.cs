using Google.Protobuf.WellKnownTypes;
using ProductGrpc.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductWorkerService
{
    public class ProductFactory
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;

        public ProductFactory(ILogger<Worker> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task<AddProductRequest> Generate()
        {
            var productName = _config.GetValue<string>("WorkerService:ProductName") + DateTimeOffset.Now;
            var productRequest = new AddProductRequest
            {
                Product = new ProductModel
                {
                    Name = productName,
                    Description = $"{productName}_Description",
                    Price = new Random().Next(1000),
                    Status = ProductStatus.Instock,
                    CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                }
            };

            return await Task.FromResult(productRequest);
        }
    }
}
