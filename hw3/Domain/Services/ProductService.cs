using Domain.Dtos;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using FluentValidation;

namespace Domain.Services;

public sealed class ProductService : IProductService
{
    private readonly IValidator<UpdatePriceDto> _priceUpdateValidator;
    private readonly IValidator<ProductCreateDto> _productCreateValidator;

    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository, IValidator<ProductCreateDto> productCreateValidator,
        IValidator<UpdatePriceDto> priceUpdateValidator)
    {
        _repository = repository;
        _productCreateValidator = productCreateValidator;
        _priceUpdateValidator = priceUpdateValidator;
    }

    public Product Create(ProductCreateDto productCreateDto)
    {
        _productCreateValidator.ValidateAndThrow(productCreateDto);
        var product = _repository.Create(productCreateDto);
        return product;
    }

    public PaginatedListDto<Product> GetList(FilterProductsDto filter)
    {
        var paginatedList = _repository.GetList(filter);
        return paginatedList;
    }

    public Product GetById(int id)
    {
        var product = _repository.GetById(id);
        if (product is null) throw new ProductNotFoundException(id);
        return product;
    }

    public Product UpdatePrice(int id, UpdatePriceDto updatePriceDto)
    {
        _priceUpdateValidator.ValidateAndThrow(updatePriceDto);

        var forUpdate = _repository.GetById(id);
        if (forUpdate is null) throw new ProductNotFoundException(id);

        var product = _repository.Update(id, forUpdate with { Price = updatePriceDto.Price });
        return product;
    }
}