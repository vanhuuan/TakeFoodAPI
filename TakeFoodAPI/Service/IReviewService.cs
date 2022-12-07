using TakeFoodAPI.ViewModel.Dtos.Review;

namespace TakeFoodAPI.Service
{
    public interface IReviewService
    {
        Task CreateReview(CreateReviewDto dto, string uid);
        Task<List<ReviewDetailDto>> GetListReview(int index, string storeId);
        Task<ReviewDetailDto> GetUserReview(string storeId, string uid);
    }
}
