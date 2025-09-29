using AutoMapper;
using Microshop.Catalog.Domain.Entities;
using Micrsoshop.Catalog.Application.Dtos.Category;

namespace Micrsoshop.Catalog.Application.Profiles;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<Category, CategoryReadDto>()
            .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Products));
        CreateMap<CategoryCreateDto, Category>();
    }
}