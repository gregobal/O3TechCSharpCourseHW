using Domain.Dtos;
using GrpcService;
using Test.Unit.TheoryData.AutoMapperProfileTestData.Base;

namespace Test.Unit.TheoryData.AutoMapperProfileTestData;

public class MapUpdatePriceRequestToUpdatePriceDtoData : MapFromToDataBase<UpdatePriceRequest, UpdatePriceDto>
{
    protected override UpdatePriceDto MapByHand(UpdatePriceRequest request)
    {
        return new UpdatePriceDto(request.Price);
    }
}