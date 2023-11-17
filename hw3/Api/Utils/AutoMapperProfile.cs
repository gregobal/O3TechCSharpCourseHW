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
            .ReverseMap();

        CreateMap<CreateRequest, ProductCreateDto>();

        CreateMap<UpdatePriceRequest, UpdatePriceDto>();

        CreateMap<GetListRequest, FilterProductsDto>();

        CreateMap<PaginatedListDto<Product>, GetListResponse>();


        CreateMap<NullableProductCategory, ProductCategory?>()
            .ConvertUsing((s, _) => s?.KindCase switch
            {
                NullableProductCategory.KindOneofCase.Value => (ProductCategory?)s.Value,
                _ => null
            });

        CreateMap<Timestamp, DateTime?>()
            .ConvertUsing((s, _) => s?.ToDateTime());

        CreateMap<Timestamp, DateTime>()
            .ConvertUsing((s, _) => s.ToDateTime());

        CreateMap<DateTime, Timestamp>()
            .ConvertUsing((s, _) => s.ToUniversalTime().ToTimestamp());
    }
}