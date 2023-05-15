using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ShoppingCartGrpc.Data;
using ShoppingCartGrpc.Models;
using ShoppingCartGrpc.Protos;

namespace ShoppingCartGrpc.Services
{
    public class ShoppingCartService : ShoppingCartProtoService.ShoppingCartProtoServiceBase
    {
        private readonly ShoppingCartContext _shoppingCartContext;
        private readonly IMapper _mapper;
        private readonly ILogger<ShoppingCartService> _logger;
        private readonly DiscountService _discountService;
        public ShoppingCartService(ShoppingCartContext shoppingCartContext, IMapper mapper, ILogger<ShoppingCartService> logger, DiscountService discountService)
        {
            _shoppingCartContext = shoppingCartContext;
            _mapper = mapper;
            _logger = logger;
            _discountService = discountService;
        }

        public override async Task<ShoppingCartModel> GetShoppingCart(GetShoppingCartRequest request, ServerCallContext context)
        {
            var shoppingCart = await _shoppingCartContext.ShoppingCart
                .FirstOrDefaultAsync(s => s.UserName == request.Username);

            if (shoppingCart == null)
                throw new RpcException(new Status(StatusCode.NotFound, $"Shopping Cart with username: {request.Username} was not found"));

            var shoppingCartModel = _mapper.Map<ShoppingCartModel>(shoppingCart);
            return shoppingCartModel;
        }

        public override async Task<ShoppingCartModel> CreateShoppingCart(ShoppingCartModel request, ServerCallContext context)
        {
            var shoppingCart = _mapper.Map<ShoppingCart>(request);

            var isExist = await _shoppingCartContext.ShoppingCart
                .AnyAsync(s => s.UserName == request.Username);
            if (isExist)
            {
                _logger.LogError($"Invalid username for Shopping Cart creation. Username {shoppingCart.UserName}");
                throw new RpcException(new Status(StatusCode.NotFound, $"ShoppingCart with username {request.Username} is already created"));
            }

            await _shoppingCartContext.ShoppingCart.AddAsync(shoppingCart);
            await _shoppingCartContext.SaveChangesAsync();

            _logger.LogInformation($"Shopping Cart is successfully created for username: {shoppingCart.UserName}");

            var shoppingCartModel = _mapper.Map<ShoppingCartModel>(shoppingCart);
            return shoppingCartModel;
        }

        public override async Task<RemoveItemItemShoppingCartResponse> RemoveItemItemShoppingCart(RemoveItemItemShoppingCartRequest request, ServerCallContext context)
        {
            var shoppingCart = await _shoppingCartContext.ShoppingCart
                .FirstOrDefaultAsync(s => s.UserName == request.Username);

            if(shoppingCart == null)
                throw new RpcException(new Status(StatusCode.NotFound, $"Shopping Cart with username: {request.Username} was not found"));

            var removeCartItem = shoppingCart.Items.FirstOrDefault(i => i.ProductId == request.NewCarItem.ProductId);
            if (null == removeCartItem)
                throw new RpcException(new Status(StatusCode.NotFound, $"Shopping Cart Item with ProductId: {request.NewCarItem.ProductId} was not found"));

            shoppingCart.Items.Remove(removeCartItem);
            var removeCount = await _shoppingCartContext.SaveChangesAsync();

            var response = new RemoveItemItemShoppingCartResponse
            {
                Success = removeCount > 0
            };

            return response;
        }

        public override async Task<AddItemItemShoppingCartResponse> AddItemItemShoppingCart(IAsyncStreamReader<AddItemItemShoppingCartRequest> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var shoppingCart = await _shoppingCartContext.ShoppingCart
                   .FirstOrDefaultAsync(s => s.UserName == requestStream.Current.Username);

                if (shoppingCart == null)
                    throw new RpcException(new Status(StatusCode.NotFound, $"Shopping Cart with username: {requestStream.Current.Username} was not found"));

                var newAddedCartItem = _mapper.Map<ShoppingCartItem>(requestStream.Current.NewCarItem);
                var cartItem = shoppingCart.Items.FirstOrDefault(i => i.ProductId == newAddedCartItem.ProductId);
                if (null != cartItem)
                {
                    cartItem.Quantity++;
                }
                else
                {
                    var discount = await _discountService.GetDiscount(requestStream.Current.DiscountCode);
                    newAddedCartItem.Price -= discount.Amount;
                    shoppingCart.Items.Add(newAddedCartItem);
                }
            }

            var insertCount = await _shoppingCartContext.SaveChangesAsync();
            var response = new AddItemItemShoppingCartResponse
            {
                Success = insertCount > 0,
                InsertCount = insertCount
            };

            return response;
        }
    }
}
