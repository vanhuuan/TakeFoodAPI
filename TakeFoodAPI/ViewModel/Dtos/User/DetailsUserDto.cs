namespace TakeFoodAPI.ViewModel.Dtos.User
{
    public class DetailsUserDto : ShowUserDto
    {
        public string avatar { get; set; }
        public DateTime? createdDate { get; set; }
    }
}
