﻿using System.Text.Json.Serialization;

namespace TakeFoodAPI.ViewModel.Dtos.Category
{
    public class CategoryDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
