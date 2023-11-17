using Domain.Dtos;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("/v2/[controller]/")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public ActionResult<PaginatedListDto<Product>> Get([FromQuery] FilterProductsDto filter)
    {
        var result = _productService.GetList(filter);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public ActionResult<Product> Get(int id)
    {
        var result = _productService.GetById(id);
        return Ok(result);
    }

    [HttpPost]
    public ActionResult<Product> Post([FromBody] ProductCreateDto productCreateDto)
    {
        var result = _productService.Create(productCreateDto);
        return Created(nameof(Post), result);
    }

    [HttpPatch("{id:int}")]
    public ActionResult<Product> Patch(int id, UpdatePriceDto updatePriceDto)
    {
        var result = _productService.UpdatePrice(id, updatePriceDto);
        return Ok(result);
    }
}