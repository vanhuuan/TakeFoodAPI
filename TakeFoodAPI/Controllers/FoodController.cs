using log4net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver.Core.WireProtocol.Messages;
using Sentry;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
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

        public async void LogStart()
        {
            var body = await new StreamReader(Request.Body).ReadToEndAsync();
            var logRequest = new LogInfo()
            {
                Type = "Request",
                Url = Request.GetDisplayUrl(),
                Body = body
            };

            var requestMessage = JsonSerializer.Serialize(logRequest);
            GlobalContext.Properties["REQUEST_TYPE"] = "Request";
            GlobalContext.Properties["URL"] = Request.GetDisplayUrl();
            if (!body.IsNullOrEmpty())
            {
                GlobalContext.Properties["BODY"] = body;
            }
            log.Info(requestMessage);
        }

        public async void LogEnd(string body)
        {
                var logRequest = new LogInfo()
                {
                    Type = "Response",
                    Url = Request.GetDisplayUrl(),
                    Body = body
                };
                logRequest.Type = "Response";
                logRequest.Body = body;
                GlobalContext.Properties["REQUEST_TYPE"] = "Response";
                GlobalContext.Properties["URL"] = Request.GetDisplayUrl();
                if (!body.IsNullOrEmpty())
                {
                    GlobalContext.Properties["BODY"] = body;
                }
                var requestMessage = JsonSerializer.Serialize(logRequest);
                log.Info(requestMessage);
        }


        [HttpPost("{StoreID}")]
        /*[Authorize(roles: Roles.ShopeOwner)]*/
        public async Task<IActionResult> CreateFood(string StoreID, CreateFoodDto food)
        {
            await _FoodService.CreateFood(StoreID, food);
            log.Info("Create Food " + food.Name);

            return Ok(food);
        }

        [HttpPut("UpdateFood")]
        //[Authorize(roles: Roles.ShopeOwner)]
        public async Task<IActionResult> UpdateFood(string FoodID, CreateFoodDto foodUpdate)
        {
            await _FoodService.UpdateFood(FoodID, foodUpdate);
            log.Info("Update Food " + foodUpdate.Name);

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
                log.Error(e.Message);
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
                LogStart();
                FoodViewMobile fMoble = await _FoodService.GetFoodByID(FoodID);
                var food = new JsonResult(fMoble);
                LogEnd(food.Value.ToString());
                return food;
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                JsonResult error = new(e.Message);
                SentrySdk.CaptureException(e);
                return error;
            }
        }
    }
    public class LogInfo
    {
        public string Type { get; set; }
        public string Url { get; set; }
        public string Body { get; set; }
    }
}
