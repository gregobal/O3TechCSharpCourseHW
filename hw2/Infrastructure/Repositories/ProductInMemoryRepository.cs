using Domain.Dtos;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Extensions;

namespace Infrastructure.Repositories;

public sealed class ProductInMemoryRepository : IProductRepository, IDisposable
{
    private const int RecordsPerPageDefault = 10;

    private readonly ReaderWriterLockSlim _repositoryRwLock = new();

    private readonly SortedDictionary<int, Product> _storage = new();

    private int _lastCreatedId;

    public void Dispose()
    {
        _repositoryRwLock.Dispose();
    }

    public PaginatedListDto<Product> GetList(FilterProductsDto filter)
    {
        var (dateFrom, dateTo, category, warehouseId, _, _) = filter;

        var dateFromBool = (Product p) => !dateFrom.HasValue || p.CreatedAt >= dateFrom;
        var dateToBool = (Product p) => !dateTo.HasValue || p.CreatedAt <= dateTo;
        var categoryBool = (Product p) => !category.HasValue || p.Category == category;
        var warehouseBool = (Product p) => !warehouseId.HasValue || p.WarehouseId == warehouseId;

        _repositoryRwLock.EnterReadLock();
        try
        {
            var result = _storage.Values.AsQueryable()
                .Where(x => dateFromBool(x) && dateToBool(x) && categoryBool(x) && warehouseBool(x))
                .Paginate(filter, RecordsPerPageDefault);

            return result;
        }
        finally
        {
            _repositoryRwLock.ExitReadLock();
        }
    }

    public Product? GetById(int id)
    {
        _repositoryRwLock.EnterReadLock();
        try
        {
            var result = _storage.GetValueOrDefault(id);

            return result;
        }
        finally
        {
            _repositoryRwLock.ExitReadLock();
        }
    }

    public Product Create(ProductCreateDto productCreateDto)
    {
        _repositoryRwLock.EnterWriteLock();
        try
        {
            var id = ++_lastCreatedId;
            var createdAt = DateTime.Now.ToUniversalTime();
            var (name, price, weight, category, warehouseId) = productCreateDto;
            var product = new Product(id, name, price, weight, category, warehouseId, createdAt);

            _storage.Add(id, product);

            return product;
        }
        finally
        {
            _repositoryRwLock.ExitWriteLock();
        }
    }

    public Product Update(int id, Product product)
    {
        _repositoryRwLock.EnterWriteLock();
        try
        {
            _storage[id] = product;

            return product;
        }
        finally
        {
            _repositoryRwLock.ExitWriteLock();
        }
    }
}