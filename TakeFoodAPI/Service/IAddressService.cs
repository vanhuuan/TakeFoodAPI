using TakeFoodAPI.ViewModel.Dtos.Address;

namespace TakeFoodAPI.Service;

public interface IAddressService
{
    Task<List<AddressDto>> GetUserAddressAsync(string uid);

    Task DeleteAddressAsync(string addressId, string uid);
}
