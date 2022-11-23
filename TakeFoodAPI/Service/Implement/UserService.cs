
using AuthenticationService.Model.Entities.Address;
using AuthenticationService.Model.Entities.Role;
using AuthenticationService.Model.Entities.User;
using AuthenticationService.Model.Repository;
using AuthenticationService.ViewModel.Dtos;
using AuthenticationService.ViewModel.Dtos.User;
using MongoDB.Bson;
using MongoDB.Driver;
using BC = BCrypt.Net.BCrypt;

namespace AuthenticationService.Service.Implement;

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
    /// Mail Service
    /// </summary>
    private readonly IMailService mailService;

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
                       IMailService mailService,
                       IJwtService jwtService,
                       IAddressService addressService,
                       IMongoRepository<Address> addressRepository,
                       IMongoRepository<UserAddress> userAddressRepository)
    {
        this.userRepository = userRepository;
        this.roleRepository = roleRepository;
        this.userRefreshTokenRepository = userRefreshTokenRepository;
        this.accountRepository = accountRepository;
        this.mailService = mailService;
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

    public async Task<List<NewsUserDto>> GetNewsUser()
    {
        List<User> allUser = (List<User>)await userRepository.GetAllAsync();
        allUser.Reverse();

        List<NewsUserDto> usersDto = new();

        foreach (var user in allUser.Take(10))
        {
            NewsUserDto newsUserDto = new()
            {
                UserID = user.Id,
                Name = user.Name
            };

            usersDto.Add(newsUserDto);
        }

        return usersDto;
    }
    public async Task<UserPagingData> GetPagingUser(GetPagingUserDto getPagingUserDto)
    {
        IList<User> listUser;
        int total = 0;
        if (getPagingUserDto.QueryType == "Email")
        {
            var accounts = await accountRepository.GetPagingAsync(Builders<Account>.Filter.Where(x => x.Email.Contains(getPagingUserDto.QueryString)), getPagingUserDto.PageNumber - 1, getPagingUserDto.PageSize);
            var listAcountsId = accounts.Data.Select(x => x.UserId);
            var users = await userRepository.FindAsync(x => listAcountsId.Contains(x.Id));
            listUser = users;
            total = accounts.Count;
        }
        else
        {
            var filter = CreateUserFilter(getPagingUserDto.QueryString, getPagingUserDto.QueryType);
            var users = await userRepository.GetPagingAsync(filter, getPagingUserDto.PageNumber - 1, getPagingUserDto.PageSize);
            listUser = users.Data.ToList();
            total = users.Count;
        }
        var list = new List<UserCardDto>();
        foreach (var user in listUser)
        {
            var userAddress = addressService.GetUserAddressAsync(user.Id).Result.FirstOrDefault();
            var account = await accountRepository.FindOneAsync(x => x.UserId == user.Id);
            var address = "";
            if (userAddress != null)
            {
                address = userAddress.Address;
            }
            if (account != null)
            {
                list.Add(new UserCardDto()
                {
                    Address = address,
                    UserId = user.Id,
                    Name = user.Name,
                    Email = account.Email,
                    Gender = user.Gender == true ? "Nam" : "Nu",
                    PhoneNumber = user.PhoneNumber,
                    Status = user.State,
                });
            }
        }

        switch (getPagingUserDto.SortBy)
        {
            case "Name":
                list = list.OrderBy(x => x.Name).ToList(); break;
            case "Email":
                list = list.OrderBy(x => x.Email).ToList(); break;
            case "PhoneNumber":
                list = list.OrderBy(x => x.PhoneNumber).ToList(); break;
            default: list = list.OrderBy(x => x.Name).ToList(); break;
        }

        if (getPagingUserDto.SortType == "Desc")
        {
            list.Reverse();
        }
        int stt = 0;
        foreach (var i in list)
        {
            stt++;
            i.Stt = stt;
            i.Id = stt;
        }
        var respone = new UserPagingData()
        {
            Total = total,
            Users = list,
            PageIndex = getPagingUserDto.PageNumber,
            PageSize = getPagingUserDto.PageSize
        };
        return respone;
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
    public async Task<List<ShowUserDto>> GetAllUser(string status)
    {
        List<User> users = (List<User>)(status == "Active" || status == "Deactive" || status == "active" || status == "deActive"
                            ? await userRepository.FindAsync(x => x.State == status)
                            : await userRepository.GetAllAsync());


        List<ShowUserDto> showUserDtos = new();
        if (users.Count > 0)
        {
            foreach (var user in users)
            {
                ShowUserDto temp = new()
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = await accountRepository.FindOneAsync(x => x.UserId == user.Id) != null ? (await accountRepository.FindOneAsync(x => x.UserId == user.Id)).Email : "Chưa có dữ liệu",
                    Phone = user.PhoneNumber,
                    Gender = user.Gender == true ? "Nam" : "Nữ",
                    Status = user.State,
                };
                temp.Address = new();
                if ((await userAddressRepository.FindAsync(x => x.UserId == user.Id)).Count > 0)
                {
                    foreach (var i in await userAddressRepository.FindAsync(x => x.UserId == user.Id))
                    {
                        string address = (await addressRepository.FindByIdAsync(i.AddressId)).Addrress;
                        if (address != null) temp.Address.Add(address);
                    }
                }
                else
                {
                    temp.Address.Add("Chưa có dữ liệu");
                }
                showUserDtos.Add(temp);
            }
        }

        return showUserDtos;
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

    public async Task<IEnumerable<ShowUserDto>> FilterByKey(string status, string key)
    {
        List<ShowUserDto> userDto = await GetAllUser(status);
        IEnumerable<ShowUserDto> result = from user in userDto where user.Email.Contains(key) || user.Phone.Contains(key) select user;

        return result;
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
