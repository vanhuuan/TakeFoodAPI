﻿
using TakeFoodAPI.ViewModel.Dtos;
using TakeFoodAPI.ViewModel.Dtos.Order;

namespace TakeFoodAPI.Service;

public interface IOrderService
{
    Task CreateOrderAsync(CreateOrderDto dto, string userId);
    Task CancelOrderAsync(String orderId, string userId);
    Task<List<OrderCardDto>> GetUserOrders(string userId, int index);
    Task<OrderDetailDto> GetOrderDetail(string userId, string orderId);
}
