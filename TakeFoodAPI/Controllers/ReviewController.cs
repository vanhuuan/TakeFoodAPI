using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TakeFoodAPI.Service;
using TakeFoodAPI.ViewModel.Dtos.Review;

namespace TakeFoodAPI.Controllers
{
    public class ReviewController : Controller
    {
        public IReviewService ReviewService { get; set; }
        public IJwtService JwtService { get; set; }
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public ReviewController(IReviewService reviewService, IJwtService jwtService)
        {
            this.ReviewService = reviewService;
            JwtService = jwtService;
        }

        [HttpPost]
        [Route("CreateReview")]
        public async Task<IActionResult> AddReviewAsync([FromBody] CreateReviewDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    log.Error(ModelState.ErrorCount);
                    return BadRequest(ModelState.ErrorCount);
                }
                await ReviewService.CreateReview(dto, GetId());
                log.Info("Create successfully");
                return Ok();
            }
            catch (Exception e)
            {
                log?.Error(e);
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("GetReviews")]
        public async Task<IActionResult> GetReviewAsync([Required] int index, [Required] string storeId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    log.Error(ModelState.IsValid);
                    return BadRequest();
                }
                var rs = await ReviewService.GetListReview(index, storeId);
          
                return Ok(rs);
            }
            catch (Exception e)
            {
                log.Error(e);
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("GetReview")]
        public async Task<IActionResult> GetUserReviewAsync([Required] string orderId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    log.Error(ModelState.IsValid);
                    return BadRequest();
                }
                var rs = await ReviewService.GetUserReview(orderId, GetId());
                return Ok(rs);
            }
            catch (Exception e)
            {
                log.Error(e);
                return BadRequest(e.Message);
            }
        }

        public string GetId()
        {
            string id = HttpContext.Items["Id"]!.ToString()!;
            return id;
        }
    }
}
