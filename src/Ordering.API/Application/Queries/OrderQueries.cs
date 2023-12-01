﻿namespace eShop.Ordering.API.Application.Queries;

public class OrderQueries(NpgsqlDataSource dataSource, IOrderRepository orderRepository, OrderingContext context)
    : IOrderQueries
{
    public async Task<Order> GetOrderAsync(int id)
    {
        var order = await orderRepository.GetAsync(id);
        if (order is null)
            throw new KeyNotFoundException();

        return new Order
        {
            ordernumber = order.Id,
            date = order.GetOrderDate(),
            description = order.GetDescription(),
            city = order.Address.City,
            country = order.Address.Country,
            state = order.Address.State,
            street = order.Address.Street,
            zipcode = order.Address.ZipCode,
            status = order.OrderStatus.Name,
            total = order.GetTotal(),
            orderitems = order.OrderItems.Select(oi => new Orderitem
            {
                productname = oi.GetOrderItemProductName(),
                units = oi.GetUnits(),
                unitprice = (double)oi.GetUnitPrice(),
                pictureurl = oi.GetPictureUri()
            }).ToList()
        };

    }

    public async Task<IEnumerable<OrderSummary>> GetOrdersFromUserAsync(string userId)
    {
        using var connection = dataSource.OpenConnection();

        return await connection.QueryAsync<OrderSummary>("""
            SELECT o."Id" AS ordernumber, o."OrderDate" AS date, os."Name" AS status, SUM(oi."Units" * oi."UnitPrice") AS total
            FROM ordering.orders AS o
            LEFT JOIN ordering."orderItems" AS oi ON o."Id" = oi."OrderId"
            LEFT JOIN ordering.orderstatus AS os ON o."OrderStatusId" = os."Id"
            LEFT JOIN ordering.buyers AS ob ON o."BuyerId" = ob."Id"
            WHERE ob."IdentityGuid" = @userId
            GROUP BY o."Id", o."OrderDate", os."Name"
            ORDER BY o."Id"
            """,
            new { userId });
    }

    public async Task<IEnumerable<CardType>> GetCardTypesAsync() => 
        await context.CardTypes.Select(c=> new CardType { Id = c.Id, Name = c.Name }).ToListAsync();
}
