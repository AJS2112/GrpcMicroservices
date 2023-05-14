using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using ProductGrpc.Protos;
using System.Xml;

namespace ProductGrpcClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var chanel = GrpcChannel.ForAddress("http://localhost:5000");
            var client = new ProductProtoService.ProductProtoServiceClient(chanel);

            //GetAllProductsRefactored(client);

            //await UpdateProduct(client);
            //await DeleteProduct(client);

            await InsertBulkProduct(client);
            await GetAllProductsRefactored(client);
            Console.ReadKey();
        }

        private static async Task GetProduct(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("Press key to start GetProductAsync...");
            Console.ReadKey();


            var request = new GetProductRequest { ProductId = 1 };
            var response = await client.GetProductAsync(request);

            Console.WriteLine("GetProductAsync: " + response.ToString());
        }

        private static async Task GetAllProducts(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("Press key to start GetProductAsync...");
            Console.ReadKey();
            using (var clientData = client.GetAllProducts(new GetAllProductsRequest()))
            {
                while (await clientData.ResponseStream.MoveNext(new CancellationToken()))
                {
                    var currentProduct = clientData.ResponseStream.Current;
                    Console.WriteLine(currentProduct);
                }
            }
        }

        private static async Task GetAllProductsRefactored(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("Press key to start GetProductAsync...");
            Console.ReadKey();
            using var clientData = client.GetAllProducts(new GetAllProductsRequest());
            await foreach (var responseData in clientData.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine(responseData);
            }
        }

        private static async Task AddProduct(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("Press key to start AddProductAsync...");
            Console.ReadKey();
            var request = new AddProductRequest
            {
                Product = new ProductModel
                {
                    Name = "X",
                    Description = "IPhone X",
                    Price = 1000,
                    Status = ProductStatus.Instock,
                    CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                }
            };

            var response = await client.AddProductAsync(request);
            Console.WriteLine(response.ToString());
        }

        private static async Task UpdateProduct(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("Press key to start UpdateProductAsync...");
            Console.ReadKey();
            var request = new UpdateProductRequest
            {
                Product = new ProductModel
                {
                    ProductId = 3,
                    Name = "XXX",
                    Description = "IPhone X",
                    Price = 1000,
                    Status = ProductStatus.Instock,
                    CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                }
            };

            var response = await client.UpdateProductAsync(request);
            Console.WriteLine(response.ToString());
        }

        private static async Task DeleteProduct(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("Press key to start DeleteProductAsync...");
            Console.ReadKey();
            var request = new DeleteProductRequest
            {
                Product = new ProductModel
                {
                    ProductId = 3,
                    Name = "XXX",
                    Description = "IPhone X",
                    Price = 1000,
                    Status = ProductStatus.Instock,
                    CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                }
            };

            var response = await client.DeleteProductAsync(request);
            Console.WriteLine(response.ToString());
        }


        private static async Task InsertBulkProduct(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("Press key to start InsertBulkProduct...");
            Console.ReadKey();

            using var clientBulk = client.InsertBulkProduct();

            for (int i = 0; i < 3; i++)
            {
                var productModel = new ProductModel
                {
                    Name = $"Product {i}",
                    Description = "Buld inserted product",
                    Price = 1000,
                    Status = ProductStatus.Instock,
                    CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                };

                await clientBulk.RequestStream.WriteAsync(productModel);
            }

            await clientBulk.RequestStream.CompleteAsync();
            var responseBulk = await clientBulk;

            Console.WriteLine($"Sttus: {responseBulk.Success}, Insert Count: {responseBulk.InsertCount}");

        }
    }
}