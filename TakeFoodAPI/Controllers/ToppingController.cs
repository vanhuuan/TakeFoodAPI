using Microsoft.AspNetCore.Mvc;
using TakeFoodAPI.Middleware;
using System.ComponentModel;
using TakeFoodAPI.Model.Entities;
using TakeFoodAPI.Service;
using TakeFoodAPI.ViewModel.Dtos.Topping;

namespace TakeFoodAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ToppingController : Controller
    {
        public readonly IToppingService _toppingService;

        public ToppingController(IToppingService toppingService)
        {
            _toppingService = toppingService;
        }

        [HttpPost("{StoreID}")]
        //[Authorize(roles: Roles.ShopeOwner)]
        public async Task<IActionResult> CreateTopping(string StoreID, CreateToppingDto createToppingDto)
        {
            await _toppingService.CreateTopping(StoreID, createToppingDto);

            return Ok();
        }

        [HttpGet("/GetAllTopping/{StoreID}")]
        public async Task<List<ToppingViewDto>> getAllToppping(string StoreID)
        {
            return await _toppingService.GetAllToppingByStoreID(StoreID, "");
        }

        [HttpGet("/GetToppingActive/{StoreID}")]
        [Authorize(roles: Roles.ShopeOwner)]
        public async Task<List<ToppingViewDto>> getAllToppingActive(string StoreID)
        {
            return await _toppingService.GetAllToppingByStoreID(StoreID, "Active");
        }

        [HttpGet("/GetToppingDeActive/{StoreID}")]
        [Authorize(roles: Roles.ShopeOwner)]
        public async Task<List<ToppingViewDto>> getAllToppingDeActive(string StoreID)
        {
            return await _toppingService.GetAllToppingByStoreID(StoreID, "DeActive");
        }

        [HttpPut]
        [Authorize(roles: Roles.ShopeOwner)]
        public async Task<IActionResult> Update(string id, CreateToppingDto updateTopping)
        {
            await _toppingService.UpdateTopping(id, updateTopping);

            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(roles: Roles.ShopeOwner)]
        public async Task<IActionResult> Delete(String id)
        {
            if (await _toppingService.DeleteTopping(id)) return Ok();

            return BadRequest("không tồn tại topping này");
        }
    }
}
