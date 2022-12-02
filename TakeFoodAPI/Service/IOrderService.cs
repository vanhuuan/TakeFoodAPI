
using TakeFoodAPI.ViewModel.Dtos.Order;

namespace TakeFoodAPI.Service;

public interface IOrderService
{
    Task CreateOrderAsync(CreateOrderDto dto, string userId);
}
