using System.ComponentModel.DataAnnotations;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
public class SaleAnalyticsController : ControllerBase
{
    private readonly ISaleAnalyticsService _service;

    public SaleAnalyticsController(ISaleAnalyticsService service)
    {
        _service = service;
    }

    [HttpGet("ads/{id:int}")]
    public IResult GetAds(int id)
    {
        return Results.Ok(
            new { Ads = _service.CalculateAds(id) }
        );
    }

    [HttpGet("prediction/{id:int}")]
    public IResult GetPrediction(
        int id,
        [FromQuery, Required, Range(1, int.MaxValue)]
        int days
    )
    {
        return Results.Ok(
            new { Prediction = _service.CalculateSalesPrediction(id, days) }
        );
    }

    [HttpGet("demand/{id:int}")]
    public IResult GetDemand(
        int id,
        [FromQuery, Required, Range(1, int.MaxValue)]
        int days,
        [FromQuery] DateOnly? supplyDate)
    {
        return Results.Ok(
            new { Demand = _service.CalculateDemand(id, days, supplyDate) }
        );
    }
}