using System.Security.Claims;
using eShop.Basket.API.Repositories;
using eShop.Basket.API.Grpc;
using eShop.Basket.API.Models;
using eShop.Basket.UnitTests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using BasketItem = eShop.Basket.API.Models.BasketItem;

namespace eShop.Basket.UnitTests;

public class BasketServiceTests
{
    [Fact]
    public async Task GetBasketReturnsEmptyForNoUser()
    {
        var mockRepository = Substitute.For<IBasketRepository>();
        var service = new BasketService(mockRepository, NullLogger<BasketService>.Instance);
        var serverCallContext = TestServerCallContext.Create();
        serverCallContext.SetUserState("__HttpContext", new DefaultHttpContext());

        var response = await service.GetBasket(new GetBasketRequest(), serverCallContext);

        Assert.IsType<CustomerBasketResponse>(response);
        Assert.Empty(response.Items);
    }

    [Fact]
    public async Task GetBasketReturnsItemsForValidUserId()
    {
        var mockRepository = Substitute.For<IBasketRepository>();
        List<BasketItem> items = [new BasketItem { Id = "some-id" }];
        mockRepository.GetBasketAsync("1").Returns(Task.FromResult(new CustomerBasket { BuyerId = "1", Items = items }));
        var service = new BasketService(mockRepository, NullLogger<BasketService>.Instance);
        var serverCallContext = TestServerCallContext.Create();
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "1")]));
        serverCallContext.SetUserState("__HttpContext", httpContext);

        var response = await service.GetBasket(new GetBasketRequest(), serverCallContext);

        Assert.IsType<CustomerBasketResponse>(response);
        Assert.Single(response.Items);
    }

    [Fact]
    public async Task GetBasketReturnsEmptyForInvalidUserId()
    {
        var mockRepository = Substitute.For<IBasketRepository>();
        List<BasketItem> items = [new BasketItem { Id = "some-id" }];
        mockRepository.GetBasketAsync("1").Returns(Task.FromResult(new CustomerBasket { BuyerId = "1", Items = items }));
        var service = new BasketService(mockRepository, NullLogger<BasketService>.Instance);
        var serverCallContext = TestServerCallContext.Create();
        var httpContext = new DefaultHttpContext();
        serverCallContext.SetUserState("__HttpContext", httpContext);

        var response = await service.GetBasket(new GetBasketRequest(), serverCallContext);

        Assert.IsType<CustomerBasketResponse>(response);
        Assert.Empty(response.Items);
    }
}
