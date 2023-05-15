using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using ProductGrpc.Protos;
using System.Runtime.CompilerServices;

namespace ProductWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;
        private readonly ProductFactory _productFactory;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, ProductFactory productFactory)
        {
            _logger = logger;
            _config = configuration;
            _productFactory = productFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Waiting for the server is running...");
            Thread.Sleep(3000);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                using var chanel = GrpcChannel.ForAddress(_config.GetValue<string>("WorkerService:ServerUrl"));
                var client = new ProductProtoService.ProductProtoServiceClient(chanel);

                await AddNewProduct(client, _productFactory, _logger);

                await Task.Delay(_config.GetValue<int>("WorkerService:TaskInterval"), stoppingToken);
            }
        }

        private static async Task AddNewProduct(ProductProtoService.ProductProtoServiceClient client, ProductFactory productFactory, ILogger logger)
        {
            logger.LogInformation("AddProductAsync started...");
            var request = await productFactory.Generate();
            var response = await client.AddProductAsync(request);
            logger.LogInformation("AddProductAsync response:" + response.ToString());
        }
    }
}