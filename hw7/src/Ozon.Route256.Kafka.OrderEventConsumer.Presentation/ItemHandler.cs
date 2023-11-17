using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Dtos;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Interfaces;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Kafka.Interfaces;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Presentation;

public class ItemHandler : IHandler<Ignore, OrderEventDto>
{
    private readonly IItemService _itemService;
    private readonly ILogger<ItemHandler> _logger;
    private readonly IMapper _mapper;

    public ItemHandler(ILogger<ItemHandler> logger, IItemService itemService, IMapper mapper)
    {
        _logger = logger;
        _itemService = itemService;
        _mapper = mapper;
    }

    public async Task Handle(IReadOnlyCollection<ConsumeResult<Ignore, OrderEventDto>> messages, CancellationToken token)
    {
        _logger.LogInformation("Consumed {Count} messages", messages.Count);

        var orderEvents = messages.Select(x => _mapper.Map<OrderEvent>(x.Message.Value))
            .ToList();

        var accountedByStatus = await _itemService.AccountingByStatus(orderEvents, token);
        _logger.LogInformation("Accounted for by status: {Count} items", accountedByStatus);

        var accountedBySeller = await _itemService.AccountingBySeller(orderEvents, token);
        _logger.LogInformation("Accounted for by seller: {Count} items", accountedBySeller);
    }
}
