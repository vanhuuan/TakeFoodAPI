using Microsoft.AspNetCore.Mvc;
using Sentry;
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
        [Route("ObjectIsNull")]
        public void Test()
        {
            log.Error("object null or empty");
        }

        [HttpGet]
        [Route("User is existed")]
        public void Test2()
        {
            log.Error("User is existed");
        }

        [HttpGet]
        [Route("InternalServer")]
        public void Test3()
        {
            log.Error("Internal Server is error");
        }

        [HttpPost("{StoreID}")]
        /*[Authorize(roles: Roles.ShopeOwner)]*/
        public async Task<IActionResult> CreateFood(string StoreID, CreateFoodDto food)
        {
            await _FoodService.CreateFood(StoreID, food);
            log.Info("Create Food");
            return Ok(food);
        }

        [HttpPut("UpdateFood")]
        //[Authorize(roles: Roles.ShopeOwner)]
        public async Task<IActionResult> UpdateFood(string FoodID, CreateFoodDto foodUpdate)
        {
            await _FoodService.UpdateFood(FoodID, foodUpdate);
            log.Info("Update Food");
            return Ok();
        }

        [HttpPut("UpdateState")]
        //[Authorize(roles: Roles.ShopeOwner)]
        public async Task<IActionResult> UpdateState(string id, bool state)
        {
            if (await _FoodService.UpdateState(id, state))
            {
                log.Info("Update State");
                return Ok();
            } 
            log.Error("Food does not exist");
            return BadRequest("không tồn tại món này");
        }

        [HttpGet("GetAllFoodByStore")]
        [Authorize]
        public async Task<IActionResult> getAllFoodByStore([Required] string StoreID)
        {
            try
            {
                var foodList = await _FoodService.GetAllFoodsByStoreID(StoreID);
                log.Info("Get All Food By Store");
                return Ok(foodList);
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                return BadRequest(e.Message);
            }
        }

        [HttpGet("GetAllFoodByCategory/{CategoryID}")]
        [Authorize]
        public async Task<List<FoodView>> GetAllFoodByCategory(string CategoryID)
        {
            log.Info("Get All Food By Category");
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
                log.Info("Get Food View Mobile");
                return food;
            }
            catch (Exception e)
            {
                JsonResult error = new(e.Message);
                log.Error(e.Message);
                return error;
            }
        }
    }
}
