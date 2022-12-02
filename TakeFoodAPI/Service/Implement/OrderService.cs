using TakeFoodAPI.Model.Entities.Address;
using TakeFoodAPI.Model.Entities.Food;
using TakeFoodAPI.Model.Entities.Order;
using TakeFoodAPI.Model.Entities.Store;
using TakeFoodAPI.Model.Entities.Topping;
using TakeFoodAPI.Model.Entities.Voucher;
using TakeFoodAPI.Model.Repository;
using TakeFoodAPI.ViewModel.Dtos.Order;
using FoodOrder = TakeFoodAPI.Model.Entities.Order.FoodOrder;
using Order = TakeFoodAPI.Model.Entities.Order.Order;
using ToppingOrder = TakeFoodAPI.Model.Entities.Order.ToppingOrder;

namespace TakeFoodAPI.Service.Implement;

public class OrderService : IOrderService
{
    private readonly IMongoRepository<Store> storeRepository;
    private readonly IMongoRepository<Address> addressRepository;
    private readonly IMongoRepository<Order> orderRepository;
    private readonly IMongoRepository<Food> foodRepository;
    private readonly IMongoRepository<FoodOrder> foodOrderRepository;
    private readonly IMongoRepository<ToppingOrder> toppingOrderRepository;
    private readonly IMongoRepository<Topping> toppingRepository;
    private readonly IMongoRepository<FoodTopping> foodToppingRepository;
    private readonly IMongoRepository<Voucher> voucherRepository;

    public OrderService(IMongoRepository<Order> orderRepository, IMongoRepository<Food> foodRepository, IMongoRepository<FoodOrder> foodOrderRepository,
        IMongoRepository<Topping> toppingRepository, IMongoRepository<FoodTopping> foodToppingRepository, IMongoRepository<Address> addressRepository, 
        IMongoRepository<ToppingOrder> toppingOrderRepository, IMongoRepository<Store> storeRepository, IMongoRepository<Voucher> voucherRepository)
    {
        this.orderRepository = orderRepository;
        this.addressRepository = addressRepository;
        this.foodRepository = foodRepository;
        this.foodOrderRepository = foodOrderRepository;
        this.foodToppingRepository = foodToppingRepository;
        this.foodRepository = foodRepository;
        this.toppingRepository = toppingRepository;
        this.toppingOrderRepository = toppingOrderRepository;
        this.storeRepository = storeRepository;
        this.voucherRepository = voucherRepository;
    }
    public async Task CreateOrderAsync(CreateOrderDto dto, string userId)
    {
        var store = await storeRepository.FindByIdAsync(dto.StoreId);
        if (store == null)
        {
            throw new Exception("Store's not exist!");
        }
        var order = new Order();
        order.UserId = userId;
        if (dto.AddressId == null || (await addressRepository.FindByIdAsync(dto.AddressId)) == null)
        {
            Address address = new Address()
            {
                AddressType = "Home",
                Addrress = dto.Address,
                Lat = 0,
                Lng = 0
            };
            address = await addressRepository.InsertAsync(address);
            dto.AddressId = address.Id;
        }
        order.AddressId = dto.AddressId;
        order.PaymentMethod = dto.PaymentMethod;
        order.Sate = "Ordered";
        order.Mode = "1";
        order.PhoneNumber = dto.PhongeNumber;
        order.Note = dto.Note != null && dto.Note != "" ? dto.Note : "không có lời nhắn";
        order.StoreId = dto.StoreId;
        order.ReceiveTime = DateTime.MinValue;
        var foodsStoreId = foodRepository.FindAsync(x => x.StoreId == order.StoreId).Result.Select(x => x.Id);
        order = await orderRepository.InsertAsync(order);
        double money = 0;
        foreach (var foodItem in dto.ListFood)
        {
            if (foodItem.Quantity <= 0) continue;
            if (foodsStoreId.Contains(foodItem.FoodId))
            {
                var food = await foodRepository.FindByIdAsync(foodItem.FoodId);
                money += food.Price * foodItem.Quantity;
                var foodOrder = await foodOrderRepository.InsertAsync(new FoodOrder()
                {
                    FoodId = foodItem.FoodId,
                    OrderId = order.Id,
                    Quantity = foodItem.Quantity
                });
                var listFoodTopping = foodToppingRepository.FindAsync(x => x.FoodId == foodItem.FoodId).Result.Select(x => x.ToppingId);

                foreach (var toppingItem in foodItem.ListToppings)
                {
                    if (toppingItem.Quantity <= 0) continue;
                    if (listFoodTopping.Contains(toppingItem.ToppingId))
                    {
                        var topping = await toppingRepository.FindOneAsync(x => x.Id == toppingItem.ToppingId);
                        money += topping.Price * toppingItem.Quantity;
                        var toppingOrder = await toppingOrderRepository.InsertAsync(new ToppingOrder()
                        {
                            Quantity = toppingItem.Quantity,
                            ToppingId = topping.Id,
                            FoodOrderId = foodOrder.Id
                        });
                    }
                }
            }
        }

        var voucher = dto.VoucherId != null && dto.VoucherId != "" ? await voucherRepository.FindByIdAsync(dto.VoucherId) : null;
        if (voucher != null && money >= voucher.MinSpend && voucher.StartDay <= DateTime.UtcNow && voucher.ExpireDay <= DateTime.UtcNow)
        {
            var discount = money * (voucher.Amount / 100);
            if (discount > voucher.MaxDiscount)
            {
                discount = voucher.MaxDiscount;
            }
            money = money - discount;
            order.Discount = discount;
        }
        order.Total = money;
        await orderRepository.UpdateAsync(order);
        // Call api to notify owner
    }
}
