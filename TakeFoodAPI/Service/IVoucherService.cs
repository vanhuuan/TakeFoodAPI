

using TakeFoodAPI.ViewModel.Dtos.Voucher;

namespace TakeFoodAPI.Service;

public interface IVoucherService
{
    Task CreateVoucherAsync(CreateVoucherDto dto, string ownerId);
    Task UpdateVoucherAsync(UpdateVoucherDto dto, string ownerId);
    Task<List<VoucherCardDto>> GetAllStoreVoucherOkeAsync(string storeId);
}
