using AutoMapper;
using Microshop.Catalog.Domain.Entities;
using Micrsoshop.Catalog.Application.Dtos.Product;

namespace Micrsoshop.Catalog.Application.Profiles;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductReadDto>()
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));
        CreateMap<ProductCreateDto, Product>();
    }
}