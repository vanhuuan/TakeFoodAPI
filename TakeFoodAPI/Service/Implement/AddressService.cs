using TakeFoodAPI.Model.Entities.Address;
using TakeFoodAPI.Model.Repository;
using TakeFoodAPI.ViewModel.Dtos.Address;

namespace TakeFoodAPI.Service.Implement;

public class AddressService : IAddressService
{
    private IMongoRepository<Address> addressRepository;
    private IMongoRepository<UserAddress> userAddressRepository;
    public AddressService(IMongoRepository<Address> addressRepository, IMongoRepository<UserAddress> userAddressRepository)
    {
        this.userAddressRepository = userAddressRepository;
        this.addressRepository = addressRepository;
    }

    public async Task DeleteAddressAsync(string addressId, string uid)
    {
        var userAddress = await userAddressRepository.FindOneAsync(x => x.UserId == uid && x.AddressId == addressId);
        if (userAddress == null)
        {
            return;
        }
        await userAddressRepository.RemoveAsync(userAddress.Id);
        await addressRepository.RemoveAsync(userAddress.AddressId);
    }

    public async Task<List<AddressDto>> GetUserAddressAsync(string uid)
    {
        var addressId = userAddressRepository.FindAsync(x => x.UserId == uid).Result.Select(x => x.AddressId);
        var addresses = await addressRepository.FindAsync(x => addressId.Contains(x.Id));

        var rs = new List<AddressDto>();
        foreach (var address in addresses)
        {
            rs.Add(new AddressDto()
            {
                AddressId = address.Id,
                Address = address.Addrress,
                AddressType = address.AddressType,
                Information = address.Information,
                Lat = address.Lat,
                Lng = address.Lng,
            });
        }
        return rs;
    }

}
