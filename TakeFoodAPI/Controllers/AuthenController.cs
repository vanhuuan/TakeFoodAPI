using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TakeFoodAPI.Service;
using TakeFoodAPI.ViewModel.Dtos;
using TakeFoodAPI.ViewModel.Dtos.User;

namespace TakeFoodAPI.Controllers;


public class AuthenController : Controller
{
    // private string url = "https://localhost:7287/";

    private string url = "https://takefoodauthentication.azurewebsites.net/";
    public IUserService UserService { get; set; }

    public IJwtService JwtService { get; set; }
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    public AuthenController(IUserService userService, IJwtService jwtService)
    {
        UserService = userService;
        JwtService = jwtService;
    }

    [HttpPost]
    [Route("SignUp")]
    public async Task<ActionResult<UserViewDto>> SignUpAsync([FromBody] CreateUserDto user)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var rs = await UserService.CreateUserAsync(user);
            if (rs == null)
            {
                log.Error("User existed");
                throw new Exception("User existed");
            }

            var accessToken = JwtService.GenerateSecurityToken(rs.Id, rs.Roles);
            /*            var mail = new MailContent();
                        mail.Subject = "Take Food Activation Email";
                        mail.To = user.Email;
                        mail.Body = url + "Active?token=" + accessToken;
                        await MailService.SendMail(mail);*/
            log.Info(rs);
            return Ok();
        }
        catch (Exception e)
        {
            log.Error(e);
            return BadRequest(e.Message);
        }
    }

    [HttpPost]
    [Route("SignIn")]
    public async Task<ActionResult<UserViewDto>> SignInAsync([FromBody] LoginDto user)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var rs = await UserService.SignIn(user);
            if (rs == null)
            {
                log.Error("Wrong username or password");
                throw new Exception("Wrong user name or password");
            }
            var refreshToken = JwtService.GenerateRefreshToken(rs.Id);
            var accessToken = JwtService.GenerateSecurityToken(rs.Id, rs.Roles);
            rs.RefreshToken = refreshToken;
            rs.AccessToken = accessToken;
            SetTokenCookie(refreshToken, accessToken);
            log.Info(rs);
            return Ok(rs);
        }
        catch (Exception e)
        {
            log.Error(e);
            Console.WriteLine(e.ToString());
            return BadRequest(e.Message);
        }
    }

    [HttpGet]
    [Route("GetAccessToken")]
    public async Task<ActionResult> GetAccessTokenAsync([Required] string token)
    {
        try
        {
            if (!JwtService.ValidRefreshToken(token))
            {
                return BadRequest("Token expired");
            }
            var id = GetId(token);
            var rs = await UserService.GetUserByIdAsync(id);
            var accessToken = JwtService.GenerateSecurityToken(id, rs.Roles);
            rs.AccessToken = accessToken;
            SetTokenCookie(token, accessToken);
            log.Info(rs);
            return Ok(rs);
        }
        catch (Exception e)
        {
            log.Error(e);
            return BadRequest(e.Message);
        }
    }

    [HttpGet]
    [Route("Active")]
    public async Task<ActionResult> ActiveUser([Required] string token)
    {
        try
        {
            UserService.Active(token);
            return Ok("OKe roi do, ve lai r dang nhap di");
        }
        catch (Exception e)
        {
            log.Error(e);
            return BadRequest(e.Message);
        }
    }

    private void SetTokenCookie(string refreshToken, string accessToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = false,
            Expires = DateTime.UtcNow.AddMonths(5)
        };
        Response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);
        Response.Cookies.Append("AccessToken", accessToken, cookieOptions);
    }


    public string GetId()
    {
        String token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last()!;
        return JwtService.GetId(token);
    }
    public string GetId(string token)
    {
        return JwtService.GetId(token);
    }
}
