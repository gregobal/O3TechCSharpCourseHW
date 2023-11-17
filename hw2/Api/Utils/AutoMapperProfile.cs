using AutoMapper;
using Domain.Dtos;
using Domain.Entities;
using Google.Protobuf.WellKnownTypes;
using GrpcService;
using ProductCategory = Domain.Entities.ProductCategory;

namespace Api.Utils;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<ProductModel, Product>()
            .ForMember(d => d.CreatedAt,
                opt => opt.MapFrom(s => s.CreatedAt.ToDateTime()))
            .ReverseMap()
            .ForPath(s => s.CreatedAt,
                opt => opt.MapFrom(s => s.CreatedAt.ToTimestamp())
            );

        CreateMap<CreateRequest, ProductCreateDto>();

        CreateMap<UpdatePriceRequest, UpdatePriceDto>();

        CreateMap<NullableProductCategory, ProductCategory?>()
            .ConvertUsing((s, d) => s?.KindCase switch
            {
                NullableProductCategory.KindOneofCase.Value => (ProductCategory?)s.Value,
                _ => null
            });

        CreateMap<GetListRequest, FilterProductsDto>();

        CreateMap<PaginatedListDto<Product>, GetListResponse>();
    }
}