using TakeFoodAPI.ViewModel.Dtos;
using TakeFoodAPI.ViewModel.Dtos.User;

namespace TakeFoodAPI.Service;

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
    Task<DetailsUserDto> GetUserByID(string id);
    Task<bool> DeleteUser(string id);
    Task ChangeUserStatus(string uid);
}
