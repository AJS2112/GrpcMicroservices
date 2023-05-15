using DiscountCrpc.Data;
using DiscountCrpc.Protos;
using Grpc.Core;

namespace DiscountCrpc.Services
{
    public class DiscountService : DiscountProtoService.DiscountProtoServiceBase
    {
        private readonly ILogger<DiscountService> _logger;

        public DiscountService(ILogger<DiscountService> logger)
        {
            _logger = logger;
        }

        public override Task<DiscountModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
        {
            var discount = DiscountContext.Discounts.FirstOrDefault(d => d.Code == request.DiscountCode);
            _logger.LogInformation($"Discount is operated with code {discount.Code} and amount {discount.Amount}");

            return Task.FromResult(new DiscountModel
            {
                Code = discount.Code,
                DiscountId = discount.DiscountId,
                Amount = discount.Amount
            }); 
        }

    }
}
