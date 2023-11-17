using Domain.Dtos;
using Domain.Entities;

namespace Domain.Interfaces;

public interface IProductRepository
{
    PaginatedListDto<Product> GetList(FilterProductsDto filter);
    Product? GetById(int id);
    Product Create(ProductCreateDto productCreateDto);
    Product Update(int id, Product product);
}