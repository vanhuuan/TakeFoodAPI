using log4net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Sentry;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using TakeFoodAPI.Hubs;
using TakeFoodAPI.Middleware;
using TakeFoodAPI.Service;
using TakeFoodAPI.Utilities.Extension;
using TakeFoodAPI.ViewModel.Dtos.Order;

namespace TakeFoodAPI.Controllers;

public class OrderController : Controller
{
    public IOrderService OrderService { get; set; }
    public IJwtService JwtService { get; set; }
    private readonly IHubContext<NotificationHub> notificationUserHubContext;
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    public OrderController(IOrderService orderService, IJwtService jwtService, IHubContext<NotificationHub> hubContext)
    {
        OrderService = orderService;
        JwtService = jwtService;
        notificationUserHubContext = hubContext;
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
    [Authorize]
    [Route("CreateOrder")]
    public async Task<IActionResult> AddOrderAsync([FromBody] CreateOrderDto dto)
    {
        try
        {
            LogStart();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.ErrorCount);
            }
            await OrderService.CreateOrderAsync(dto, GetId());
            LogEnd("");
            return Ok();
        }
        catch (Exception e)
        {
            SentrySdk.CaptureException(e);
            log.Error(e.Message);
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
    [Authorize]
    [Route("GetOrders")]
    public async Task<IActionResult> GetOrdersAsync([Required] int index)
    {
        try
        {
            LogStart();
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var rs = await OrderService.GetUserOrders(GetId(), index);
            LogEnd(rs.ToJsonString());
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
            LogStart();
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var rs = await OrderService.GetOrderDetail(GetId(), orderId);
            LogEnd(rs.ToJsonString());
            return Ok(rs);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet]
    [Route("Notify")]
    public async Task<IActionResult> NotifyOrderStateChangeAsync([Required] string orderId)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var rs = await OrderService.GetNotifyInfo(orderId);
            foreach (var connectionId in NotificationHub._connections.GetConnections(rs.UserId))
            {
                await notificationUserHubContext.Clients.Client(connectionId).SendAsync("sendToUser", rs.Header, rs.Message);
            }
            return Ok();
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
