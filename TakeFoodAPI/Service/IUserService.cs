using AuthenticationService.ViewModel.Dtos;
using AuthenticationService.ViewModel.Dtos.User;

namespace AuthenticationService.Service;

/// <summary>
/// IUserService
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Get user by user id
    /// </summary>
    /// <returns></returns>
    Task<UserViewDto> GetUserByIdAsync(string id);

    /// <summary>
    /// Create new user
    /// </summary>
    /// <returns></returns>
    Task<UserViewDto> CreateUserAsync(CreateUserDto createUserDto);

    /// <summary>
    /// Sign in by phone number or email
    /// </summary>
    /// <returns></returns>
    Task<UserViewDto> SignIn(LoginDto loginDto);

    /// <summary>
    /// Active user via active link
    /// </summary>
    /// <returns></returns>
    void Active(string token);

    /// <summary>
    /// UpdateUserInfo
    /// </summary>
    /// <returns></returns>
    Task<UserViewDto> UpdateUserInfo(UpdateUserDto updateDto, string uid);
    Task<List<NewsUserDto>> GetNewsUser();
    Task<UserPagingData> GetPagingUser(GetPagingUserDto getPagingUserDto);
    Task<List<ShowUserDto>> GetAllUser(string status);
    Task<DetailsUserDto> GetUserByID(string id);
    Task<IEnumerable<ShowUserDto>> FilterByKey(string status, string key);
    Task<bool> DeleteUser(string id);
    Task ChangeUserStatus(string uid);
}
