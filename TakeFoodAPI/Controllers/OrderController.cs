using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TakeFoodAPI.Service;
using TakeFoodAPI.ViewModel.Dtos.Order;

namespace TakeFoodAPI.Controllers;

public class OrderController : BaseController
{
    public IOrderService OrderService { get; set; }
    public IJwtService JwtService { get; set; }
    public OrderController(IOrderService orderService, IJwtService jwtService)
    {
        OrderService = orderService;
        JwtService = jwtService;
    }

    [HttpPost]
    //[Authorize]
    [Route("CreateOrder")]
    public async Task<IActionResult> AddOrderAsync([FromBody] CreateOrderDto dto)
    {
        try
        {
            /*if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.ErrorCount);
            }*/
            await OrderService.CreateOrderAsync(dto, GetId());
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPut]
    [Route("CancelOrder")]
    public async Task<IActionResult> CancelOrderAsync([Required] string orderId)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await OrderService.CancelOrderAsync(orderId, GetId());
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet]
    [Route("GetOrders")]
    public async Task<IActionResult> GetOrdersAsync([Required] int index)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var rs = await OrderService.GetUserOrders(GetId(), index);
            return Ok(rs);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet]
    [Route("GetOrderdetail")]
    public async Task<IActionResult> GetOrderDetailAsync([Required] string orderId)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var rs = await OrderService.GetOrderDetail(GetId(), orderId);
            return Ok(rs);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    public string GetId()
    {
        String token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last()!;
        return JwtService.GetId(token);
    }
}
