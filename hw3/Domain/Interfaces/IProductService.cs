using Domain.Dtos;
using Domain.Entities;

namespace Domain.Interfaces;

public interface IProductService
{
    Product Create(ProductCreateDto productCreateDto);

    PaginatedListDto<Product> GetList(FilterProductsDto filterProductsDto);

    Product GetById(int id);

    Product UpdatePrice(int id, UpdatePriceDto updatePriceDto);
}