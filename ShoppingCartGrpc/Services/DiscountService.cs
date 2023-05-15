using DiscountCrpc.Protos;

namespace ShoppingCartGrpc.Services
{
    public class DiscountService
    {
        private readonly DiscountProtoService.DiscountProtoServiceClient _client;

        public DiscountService(DiscountProtoService.DiscountProtoServiceClient client)
        {
            _client = client;
        }

        public async Task<DiscountModel> GetDiscount(string discountCode)
        {
            var discountRequest = new GetDiscountRequest { DiscountCode = discountCode };
            return await _client.GetDiscountAsync(discountRequest);
        }
    }
}
