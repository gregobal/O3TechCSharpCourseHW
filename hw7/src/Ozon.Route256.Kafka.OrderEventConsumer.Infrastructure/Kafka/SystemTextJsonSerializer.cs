using System;
using System.Text.Json;
using Confluent.Kafka;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Kafka;

internal sealed class SystemTextJsonSerializer<T> : IDeserializer<T>, ISerializer<T>
{
    private readonly JsonSerializerOptions? _jsonSerializerOptions;

    public SystemTextJsonSerializer(JsonSerializerOptions? jsonSerializerOptions = null) => _jsonSerializerOptions = jsonSerializerOptions;

    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        if (isNull)
        {
            throw new ArgumentNullException(nameof(data), "Can't deserialize null.");
        }

        var deserialized = JsonSerializer.Deserialize<T>(data, _jsonSerializerOptions);

        if (deserialized is null)
        {
            throw new ArgumentException($"Deserialization {nameof(data)} to {typeof(T)} failed.");
        }

        return deserialized;
    }

    public byte[] Serialize(T data, SerializationContext context) => JsonSerializer.SerializeToUtf8Bytes(data, _jsonSerializerOptions);
}
