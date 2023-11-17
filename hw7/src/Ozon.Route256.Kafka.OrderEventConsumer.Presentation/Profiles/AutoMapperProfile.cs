using AutoMapper;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Dtos;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities.ValueObjects;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Profiles;

public sealed class AutoMapperProfile : Profile
{
    private const decimal NanosToDecimalDelimiter = 1_000_000_000m;

    public AutoMapperProfile()
    {
        CreateMap<OrderEventDto.Money, Money>()
            .ForMember(
                s => s.Value,
                options =>
                    options.MapFrom(s => s.Units + s.Nanos / NanosToDecimalDelimiter));
        CreateMap<OrderEventDto.OrderEventPosition, OrderEventPosition>();
        CreateMap<OrderEventDto, OrderEvent>();
    }
}
