using log4net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Sentry;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using TakeFoodAPI.Middleware;
using TakeFoodAPI.Model.Entities;
using TakeFoodAPI.Service;
using TakeFoodAPI.Utilities.Extension;
using TakeFoodAPI.ViewModel.Dtos.Store;


namespace TakeFoodAPI.Controllers
{
    public class StoreController : Controller
    {
        private ITakeFoodAPI _TakeFoodAPI;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public StoreController(ITakeFoodAPI TakeFoodAPI)
        {
            _TakeFoodAPI = TakeFoodAPI;
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

        [HttpPost]
        /*[Authorize(roles: Roles.User)]*/
        [Route("CreateStore")]
        public async Task<IActionResult> CreateStoreAsync(string OwnerID, [FromBody] CreateStoreDto store)
        {
            try
            {
                await _TakeFoodAPI.CreateStore(OwnerID, store);
                log.Info("Create Store successfully");
                return Ok();
            }
            catch (Exception err)
            {
                log.Error(err.Message);
                return BadRequest(err.Message);
            }
        }

        [HttpPost]
        [Authorize]
        [Route("GetNearBy")]
        public async Task<ActionResult<List<CardStoreDto>>> GetStoreNearByAsync([FromBody] GetStoreNearByDto dto)
        {
            try
            {
                LogStart();
                var list = await _TakeFoodAPI.GetStoreNearByAsync(dto);
                log.Info("Get Store Near By");
                LogEnd(list.ToJsonString());
                return list;
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                log.Error(err.Message);
                SentrySdk.CaptureException(err);
                throw;
            }
        }

        [HttpPost]
        [Authorize]
        [Route("FilterNearByWithCategory")]
        public async Task<IActionResult> FilterStoreNearByAsync([FromBody] FilterStoreByCategoryId dto)
        {
            try
            {
                var list = await _TakeFoodAPI.FilterStoreNearByAsync(dto);
                return Ok(list);
            }
            catch (Exception err)
            {
                log.Error(err.Message);
                return BadRequest(err.Message);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("FindStore")]
        public async Task<IActionResult> FindStoreAsync([Required] string name, [Required] double lat, [Required] double lng, [Required] int start)
        {
            try
            {
                var list = await _TakeFoodAPI.FindStoreByNameAsync(name, lat, lng, start);
                log.Info("Find Store");
                return Ok(list);
            }
            catch (Exception err)
            {
                log.Error(err.Message);
                return BadRequest(err.Message);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("GetStore")]
        public async Task<IActionResult> GetStoreById([Required] string storeId, [Required] double lat, [Required] double lng)
        {
            try
            {
                LogStart();
                var store = await _TakeFoodAPI.GetStoreDetailAsync(storeId, lat, lng);
                LogEnd(store.ToJsonString());
                return Ok(store);
            }
            catch (Exception err)
            {
                log.Error(err.Message);
                SentrySdk.CaptureException(err);
                return BadRequest(err.Message);
            }
        }

        [HttpGet]
        [Authorize(roles: Roles.Admin)]
        [Route("InsertStore")]
        public async Task<IActionResult> InsertStoreAsync()
        {
            await _TakeFoodAPI.InertCrawlData();

            return Ok();
        }

        [HttpGet]
        [Authorize(roles: Roles.Admin)]
        [Route("InsertMenu")]
        public async Task<IActionResult> InsertMenuStoreAsync()
        {
            await _TakeFoodAPI.InertMenuCrawlDataAsync();

            return Ok();
        }
    }
}
