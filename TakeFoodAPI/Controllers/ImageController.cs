/*using Microsoft.AspNetCore.Mvc;
using TakeFoodAPI.Service;
using TakeFoodAPI.ViewModel.Dtos.Image;

namespace TakeFoodAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : BaseController
    {
        private readonly IImageService Imageservice;

        public ImageController(IImageService imageservice)
        {
            Imageservice = imageservice;
        }

        [HttpPost]
        public async Task<IActionResult> createImage(string StoreID, string categoryID, ImageDto image)
        {
            await Imageservice.CreateImage(StoreID, categoryID, image);

            return Ok(image);
        }
    }
}
*/