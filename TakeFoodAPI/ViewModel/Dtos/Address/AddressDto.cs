using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TakeFoodAPI.ViewModel.Dtos.Address
{
    public class AddressDto
    {
        /// <summary>
        /// User name is phoneNumber or Email
        /// </summary>
        [JsonPropertyName("addressId")]
        [Required]
        public string AddressId { get; set; }
        /// <summary>
        /// Addtional information
        /// </summary>
        [JsonPropertyName("information")]
        [Required]
        public string Information { get; set; }
        /// <summary>
        /// Address of place
        /// </summary>
        [JsonPropertyName("address")]
        [Required]
        public string Address { get; set; }
        /// <summary>
        /// Address Type
        /// </summary>
        [JsonPropertyName("addressType")]
        [Required]
        public string AddressType { get; set; }
        /// <summary>
        /// Latitude
        /// </summary>
        [JsonPropertyName("lat")]
        [Required]
        public double Lat { get; set; }
        /// <summary>
        /// Longitude
        /// </summary>
        [JsonPropertyName("lng")]
        [Required]
        public double Lng { get; set; }
    }
}
