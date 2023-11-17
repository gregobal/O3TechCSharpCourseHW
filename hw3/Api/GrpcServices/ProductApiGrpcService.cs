using AutoMapper;
using Domain.Dtos;
using Domain.Interfaces;
using Grpc.Core;
using GrpcService;

namespace Api.GrpcServices;

public class ProductApiGrpcService : ProductGrpcService.ProductGrpcServiceBase
{
    private readonly IMapper _mapper;
    private readonly IProductService _productService;

    public ProductApiGrpcService(IProductService productService, IMapper mapper)
    {
        _productService = productService;
        _mapper = mapper;
    }

    public override Task<GetListResponse> GetList(GetListRequest request, ServerCallContext context)
    {
        var filter = _mapper.Map<FilterProductsDto>(request);
        var paginatedList = _productService.GetList(filter);

        var response = _mapper.Map<GetListResponse>(paginatedList);

        return Task.FromResult(response);
    }

    public override Task<GetByIdResponse> GetById(GetByIdRequest request, ServerCallContext context)
    {
        var product = _productService.GetById(request.Id);

        var result = _mapper.Map<ProductModel>(product);
        var response = new GetByIdResponse
        {
            Product = result
        };

        return Task.FromResult(response);
    }

    public override Task<CreateResponse> Create(CreateRequest request, ServerCallContext context)
    {
        var productCreateDto = _mapper.Map<ProductCreateDto>(request);
        var product = _productService.Create(productCreateDto);

        var result = _mapper.Map<ProductModel>(product);
        var response = new CreateResponse
        {
            Product = result
        };

        return Task.FromResult(response);
    }

    public override Task<UpdatePriceResponse> UpdatePrice(UpdatePriceRequest request, ServerCallContext context)
    {
        var updatePriceDto = _mapper.Map<UpdatePriceDto>(request);
        var product = _productService.UpdatePrice(request.Id, updatePriceDto);

        var result = _mapper.Map<ProductModel>(product);
        var response = new UpdatePriceResponse
        {
            Product = result
        };

        return Task.FromResult(response);
    }
}