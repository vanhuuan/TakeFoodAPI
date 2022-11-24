using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TakeFoodAPI.Middleware;
using TakeFoodAPI.Service;
using TakeFoodAPI.ViewModel.Dtos.Food;

namespace TakeFoodAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FoodController : BaseController
    {
        private IFoodService _FoodService;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public FoodController(IFoodService foodService)
        {
            _FoodService = foodService;
        }

        [HttpGet]
        [Route("test")]
        public void Test()
        {
            log.Error("GetWeatherForecast  Get - this is a nice message a test the logs");
            log.Info("THis is log info");
        }

        [HttpPost("{StoreID}")]
        /*[Authorize(roles: Roles.ShopeOwner)]*/
        public async Task<IActionResult> CreateFood(string StoreID, CreateFoodDto food)
        {
            await _FoodService.CreateFood(StoreID, food);

            return Ok(food);
        }

        [HttpPut("UpdateFood")]
        //[Authorize(roles: Roles.ShopeOwner)]
        public async Task<IActionResult> UpdateFood(string FoodID, CreateFoodDto foodUpdate)
        {
            await _FoodService.UpdateFood(FoodID, foodUpdate);

            return Ok();
        }

        [HttpPut("UpdateState")]
        //[Authorize(roles: Roles.ShopeOwner)]
        public async Task<IActionResult> UpdateState(string id, bool state)
        {
            if (await _FoodService.UpdateState(id, state)) return Ok();

            return BadRequest("không tồn tại món này");
        }

        [HttpGet("GetAllFoodByStore")]
        [Authorize]
        public async Task<IActionResult> getAllFoodByStore([Required] string StoreID)
        {
            try
            {
                var foodList = await _FoodService.GetAllFoodsByStoreID(StoreID);
                return Ok(foodList);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("GetAllFoodByCategory/{CategoryID}")]
        [Authorize]
        public async Task<List<FoodView>> GetAllFoodByCategory(string CategoryID)
        {
            return await _FoodService.GetAllFoodsByCategory(CategoryID);
        }

        [HttpGet("GetFoodViewMobile")]
        [Authorize]
        public async Task<JsonResult> GetFoodViewMobile([Required] string FoodID)
        {
            try
            {
                FoodViewMobile fMoble = await _FoodService.GetFoodByID(FoodID);
                var food = new JsonResult(fMoble);
                return food;
            }
            catch (Exception e)
            {
                JsonResult error = new(e.Message);
                return error;
            }
        }
    }
}
