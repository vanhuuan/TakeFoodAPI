using TakeFoodAPI.Model.Entities.Order;
using TakeFoodAPI.Model.Entities.Review;
using TakeFoodAPI.Model.Entities.Store;
using TakeFoodAPI.Model.Entities.User;
using TakeFoodAPI.Model.Repository;
using TakeFoodAPI.ViewModel.Dtos.Review;

namespace TakeFoodAPI.Service.Implement
{
    public class ReviewService : IReviewService
    {
        private IMongoRepository<Review> reviewRepository { get; set; }
        private IMongoRepository<Order> orderRepository { get; set; }
        private IMongoRepository<Store> storeRepository { get; set; }
        private IMongoRepository<User> userRepository { get; set; }
        public ReviewService(IMongoRepository<Review> reviewRepository, IMongoRepository<Order> orderRepository, IMongoRepository<Store> storeRepository,
            IMongoRepository<User> userRepository)
        {
            this.reviewRepository = reviewRepository;
            this.orderRepository = orderRepository;
            this.storeRepository = storeRepository;
            this.userRepository = userRepository;
        }

        public async Task CreateReview(CreateReviewDto dto, string uid)
        {
            var order = await orderRepository.FindOneAsync(x => x.Id == dto.OrderId);
            if (order == null || order.UserId != uid)
            {
                throw new Exception("Order's note exist");
            }
            var store = await storeRepository.FindOneAsync(x => x.Id == order.StoreId);
            store.NumReiview++;
            store.SumStar += dto.Star;
            var count = await reviewRepository.CountAsync(x => x.OrderId == dto.OrderId);
            if (count > 0)
            {
                throw new Exception("You reviewed");
            }
            var review = new Review()
            {
                Star = dto.Star,
                OrderId = dto.OrderId,
                Imgs = dto.Images,
                Description = dto.Description
            };
            await storeRepository.UpdateAsync(store);
            await reviewRepository.InsertAsync(review);
        }

        public async Task<List<ReviewDetailDto>> GetListReview(int index, string storeId)
        {
            var list = new List<ReviewDetailDto>();
            var orderIds = orderRepository.FindAsync(x => x.StoreId == storeId).Result.Select(x => x.Id);
            var reviews = reviewRepository.FindAsync(x => orderIds.Contains(x.OrderId)).Result.Take(index * 10).TakeLast(10);
            foreach (var review in reviews)
            {
                var order = await orderRepository.FindByIdAsync(review.OrderId);
                if (order == null) continue;
                var user = await userRepository.FindByIdAsync(order.UserId);
                var detail = new ReviewDetailDto()
                {
                    Description = review.Description,
                    Images = review.Imgs,
                    Star = review.Star,
                };

                if (user != null)
                {
                    detail.UserName = user.Name;
                }
                else
                {
                    detail.UserName = "Unknow";
                }
                list.Add(detail);
            }
            return list;
        }

        public async Task<ReviewDetailDto> GetUserReview(string orderId, string uid)
        {
            var order = await orderRepository.FindByIdAsync(orderId);
            var user = await userRepository.FindByIdAsync(uid);
            if (order == null || user == null || order.UserId != uid)
            {
                throw new Exception("Order khong ton tai");
            }
            var review = await reviewRepository.FindOneAsync(x => x.OrderId == orderId);
            var reviewDetail = new ReviewDetailDto()
            {
                Description = review.Description,
                Images = review.Imgs,
                Star = review.Star,
                UserName = user.Name
            };
            return reviewDetail;
        }
    }
}
