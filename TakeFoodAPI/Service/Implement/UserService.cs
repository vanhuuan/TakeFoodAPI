
using MongoDB.Bson;
using MongoDB.Driver;
using TakeFoodAPI.Model.Entities.Address;
using TakeFoodAPI.Model.Entities.Role;
using TakeFoodAPI.Model.Entities.User;
using TakeFoodAPI.Model.Repository;
using TakeFoodAPI.ViewModel.Dtos;
using TakeFoodAPI.ViewModel.Dtos.User;
using BC = BCrypt.Net.BCrypt;

namespace TakeFoodAPI.Service.Implement;

public class UserService : IUserService
{
    /// <summary>
    /// UserRepository
    /// </summary>
    private readonly IMongoRepository<User> userRepository;

    /// <summary>
    /// AccountRepository
    /// </summary>
    private readonly IMongoRepository<Account> accountRepository;

    /// <summary>
    /// RoleRepository
    /// </summary>
    private readonly IMongoRepository<Role> roleRepository;

    /// <summary>
    /// UserRefreshTokenRepository
    /// </summary>
    private readonly IMongoRepository<UserRefreshToken> userRefreshTokenRepository;

    private readonly IMongoRepository<Address> addressRepository;

    /// <summary>
    /// Jwt Service
    /// </summary>
    private readonly IJwtService jwtService;
    private IAddressService addressService;

    /// <summary>
    /// User service constructor
    /// </summary>
    /// <param name="userRepository"></param>
    /// <param name="roleRepository"></param>
    /// 
    private readonly IMongoRepository<UserAddress> userAddressRepository;
    public UserService(IMongoRepository<User> userRepository,
                       IMongoRepository<UserRefreshToken> userRefreshTokenRepository,
                       IMongoRepository<Role> roleRepository,
                       IMongoRepository<Account> accountRepository,
                       IJwtService jwtService,
                       IAddressService addressService,
                       IMongoRepository<Address> addressRepository,
                       IMongoRepository<UserAddress> userAddressRepository)
    {
        this.userRepository = userRepository;
        this.roleRepository = roleRepository;
        this.userRefreshTokenRepository = userRefreshTokenRepository;
        this.accountRepository = accountRepository;
        this.jwtService = jwtService;
        this.addressService = addressService;
        this.addressRepository = addressRepository;
        this.userAddressRepository = userAddressRepository;
    }

    /// <summary>
    /// Get user by user id
    /// </summary>
    /// <returns></returns>
    public async Task<UserViewDto> GetUserByIdAsync(string id)
    {
        var user = await userRepository.FindByIdAsync(id);
        var account = await accountRepository.FindOneAsync(x => x.UserId == id);

        var view = new UserViewDto()
        {
            Name = user.Name,
            Email = account.Email,
            Photo = user.Avatar,
            Id = user.Id,
            Phone = user.PhoneNumber

        };
        var role = new List<String>();
        var listRole = await roleRepository.GetAllAsync();
        foreach (var i in listRole)
        {
            if (user.RoleIds.Contains(i.Id))
            {
                role.Add(i.Name!);
            }
        }
        view.Roles = role;
        return view;
    }

    /// <summary>
    /// Create new user
    /// </summary>
    /// <returns></returns>
    public async Task<UserViewDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        var users = await userRepository.FindAsync(x => x.PhoneNumber == createUserDto.PhoneNumber);
        if (users.Count != 0)
        {
            throw new Exception("Trung SDT");
        }
        var accounts = await accountRepository.FindAsync(x => x.Email == createUserDto.Email);
        if (accounts.Count != 0)
        {
            throw new Exception("Trung Email");
        }

        var user = new User()
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = createUserDto.Name,
            PhoneNumber = createUserDto.PhoneNumber,
            RoleIds = new List<String>() { "2" },
            State = "Active",
            UpdatedDate = DateTime.Now,
            CreatedDate = DateTime.Now,
        };
        var rs = await userRepository.InsertAsync(user);

        var account = new Account()
        {
            Password = BC.HashPassword(createUserDto.Password),
            Email = createUserDto.Email,
            UserId = rs.Id
        };

        var rsAcount = await accountRepository.InsertAsync(account);

        return await GetUserByIdAsync(rs.Id);
    }

    /// <summary>
    /// Sign in by phone number or email
    /// </summary>
    /// <returns></returns>
    public async Task<UserViewDto> SignIn(LoginDto loginDto)
    {
        var account = await accountRepository.FindOneAsync(x => x.Email.Equals(loginDto.UserName));
        if (account == null)
        {
            throw new Exception("Wrong user name or password");
        }

        var user = await userRepository.FindByIdAsync(account.UserId);
        if (BC.Verify(loginDto.Password, account.Password) && user.State == "Active")
        {
            return await GetUserByIdAsync(user.Id);
        }
        else
        {
            throw new Exception("Wrong user name or password");
        }
    }

    public async void Active(string token)
    {
        var check = jwtService.ValidSecurityToken(token);
        if (!check)
        {
            throw new Exception("Invalid token");
        }
        var id = jwtService.GetId(token);
        var user = await userRepository.FindOneAsync(x => x.Id == id);
        user.State = "Active";
        await userRepository.UpdateAsync(user);
    }

    public async Task<UserViewDto> UpdateUserInfo(UpdateUserDto updateDto, string uid)
    {
        var user = await userRepository.FindByIdAsync(uid);
        var acc = await accountRepository.FindOneAsync(x => x.UserId.Equals(uid));
        if (user == null)
        {
            throw new Exception("User's not exist!");
        }
        user.Name = updateDto.Name;
        user.PhoneNumber = updateDto.PhoneNumber;
        acc.Email = updateDto.Email;

        await userRepository.UpdateAsync(user);
        await accountRepository.UpdateAsync(acc);

        return await GetUserByIdAsync(uid);
    }

    private FilterDefinition<User> CreateUserFilter(string query, string queryType)
    {
        var filter = Builders<User>.Filter.Empty;
        if (queryType != "All")
        {
            switch (queryType)
            {
                case "PhoneNumber": filter &= Builders<User>.Filter.Where(x => x.PhoneNumber.Contains(query)); break;
                case "Name": filter &= Builders<User>.Filter.Where(x => x.Name.Contains(query)); break;
                default: filter &= Builders<User>.Filter.StringIn(x => x.Name, query); break;
            }
        }
        return filter;
    }

    public async Task<DetailsUserDto> GetUserByID(string id)
    {
        User user = await userRepository.FindByIdAsync(id);
        if (user != null)
        {
            DetailsUserDto userDto = new()
            {
                Id = user.Id,
                Name = user.Name,
                Email = await accountRepository.FindOneAsync(x => x.UserId == user.Id) != null ? (await accountRepository.FindOneAsync(x => x.UserId == user.Id)).Email : "Chưa có dữ liệu",
                Phone = user.PhoneNumber,
                Gender = user.Gender == true ? "Nam" : "Nữ",
                Status = user.State,
                avatar = user.Avatar != null ? user.Avatar : "chưa có dữ liệu",
                createdDate = user.CreatedDate
            };
            userDto.Address = new();
            if ((await userAddressRepository.FindAsync(x => x.UserId == user.Id)).Count > 0)
            {
                foreach (var i in await userAddressRepository.FindAsync(x => x.UserId == user.Id))
                {
                    string address = (await addressRepository.FindByIdAsync(i.AddressId)).Addrress;
                    if (address != null) userDto.Address.Add(address);
                }
            }
            else
            {
                userDto.Address.Add("Chưa có dữ liệu");
            }
            return userDto;
        }
        else
        {
            throw new Exception("không tồn tại user này");
        }
    }

    public async Task<bool> DeleteUser(string id)
    {
        User user = await userRepository.FindByIdAsync(id);
        if (user != null)
        {
            Account account = await accountRepository.FindOneAsync(x => x.UserId == id);
            account.IsDeleted = true;
            await accountRepository.UpdateAsync(account);
            user.IsDeleted = true;
            await userRepository.UpdateAsync(user);
            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task ChangeUserStatus(string uid)
    {
        User user = await userRepository.FindByIdAsync(uid);
        if (user != null)
        {
            if (user.State == "Active")
            {
                user.State = "InActive";
            }
            else
            {
                user.State = "Active";
            }
            await userRepository.UpdateAsync(user);
            return;
        }
        else
        {
            return;
        }
    }
}
