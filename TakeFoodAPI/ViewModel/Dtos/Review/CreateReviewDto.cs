using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TakeFoodAPI.ViewModel.Dtos.Review
{
    public class CreateReviewDto
    {
        [JsonPropertyName("orderId")]
        [Required]
        public string OrderId { get; set; }
        /// <summary>
        /// Review description
        /// </summary>
        [JsonPropertyName("description")]
        [Required]
        public string Description { get; set; }
        /// <summary>
        /// Start for store
        /// </summary>
        [JsonPropertyName("star")]
        [Required]
        public int Star { get; set; }
        /// <summary>
        /// List review images
        /// </summary>
        [JsonPropertyName("images")]
        public List<String> Images { get; set; }
    }
}
