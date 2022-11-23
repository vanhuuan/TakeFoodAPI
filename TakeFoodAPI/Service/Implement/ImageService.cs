﻿using TakeFoodAPI.Model.Entities.Image;
using TakeFoodAPI.Model.Repository;
using TakeFoodAPI.ViewModel.Dtos.Image;

namespace TakeFoodAPI.Service.Implement
{
    public class ImageService : IImageService
    {
        private readonly IMongoRepository<Image> ImageRepository;

        public ImageService(IMongoRepository<Image> mongoRepository)
        {
            this.ImageRepository = mongoRepository;
        }

        public async Task CreateImage(string storeID, string categoryID, ImageDto image)
        {
            Image _image = new Image()
            {
                StoreId = storeID,
                CategoryId = categoryID,
                Url = image.Url,
            };
            await ImageRepository.InsertAsync(_image);
        }

        public Task DeleteImage(string id)
        {
            throw new NotImplementedException();
        }

        public Task<List<ImageDto>> GetAllImages()
        {
            throw new NotImplementedException();
        }

        public Task<ImageDto> GetImageById(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetStoreSlug(string storeId)
        {
            return (await ImageRepository.FindOneAsync(x => x.StoreId == storeId)).Url;
        }

        public Task UpdateImage(string id, ImageDto image)
        {
            throw new NotImplementedException();
        }
    }
}
