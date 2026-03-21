using System.Text.Json;
using System.Text.Json.Serialization;

namespace CueCompanion;

[JsonConverter(typeof(ResultJsonConverterFactory))]
public sealed class Result<T>
{
    public Result()
    {
    }

    private Result(bool isSuccess, T? value, string? error, Dictionary<string, object>? meta)
    {
        (IsSuccess, Value, Error, Meta) = (isSuccess, value, error, meta);
    }

    public bool IsSuccess { get; set; }
    public T? Value { get; set; }
    public string? Error { get; set; }
    public Dictionary<string, object>? Meta { get; set; }

    public static Result<T> Success(T value, Dictionary<string, object>? meta = null)
    {
        return new Result<T>(true, value, null, meta);
    }

    public static Result<T> Failure(string error, Dictionary<string, object>? meta = null)
    {
        return new Result<T>(false, default, error, meta);
    }

    public static implicit operator Result<T>(T value)
    {
        return Success(value);
    }

    public static implicit operator Result<T>(string error)
    {
        return Failure(error);
    }

    public T GetValue(Action<string>? onError = null)
    {
        if (!IsSuccess)
        {
            if (onError != null) onError(Error ?? "Unknown error");
            return default!;
        }

        return Value!;
    }
}

// Custom converter needed to correctly serialize tuples
public sealed class ResultJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Result<>);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type valueType = typeToConvert.GetGenericArguments()[0];
        Type converterType = typeof(ResultJsonConverter<>).MakeGenericType(valueType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

public sealed class ResultJsonConverter<T> : JsonConverter<Result<T>>
{
    private static JsonSerializerOptions GetValueOptions(JsonSerializerOptions options)
    {
        if (options.IncludeFields)
            return options;

        return new JsonSerializerOptions(options)
        {
            IncludeFields = true
        };
    }

    public override Result<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected start of object when deserializing Result.");

        JsonSerializerOptions valueOptions = GetValueOptions(options);

        bool isSuccess = false;
        bool hasIsSuccess = false;
        T? value = default;
        string? error = null;
        Dictionary<string, object>? meta = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected property name when deserializing Result.");

            string propertyName = reader.GetString() ?? string.Empty;
            reader.Read();

            switch (propertyName)
            {
                case "isSuccess":
                case "IsSuccess":
                    isSuccess = JsonSerializer.Deserialize<bool>(ref reader, options);
                    hasIsSuccess = true;
                    break;
                case "value":
                case "Value":
                    value = JsonSerializer.Deserialize<T>(ref reader, valueOptions);
                    break;
                case "error":
                case "Error":
                    error = JsonSerializer.Deserialize<string>(ref reader, options);
                    break;
                case "meta":
                case "Meta":
                    meta = JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader, options);
                    break;
                default:
                    JsonSerializer.Deserialize<JsonElement>(ref reader, options);
                    break;
            }
        }

        return new Result<T>
        {
            IsSuccess = hasIsSuccess && isSuccess,
            Value = value,
            Error = error,
            Meta = meta
        };
    }

    public override void Write(Utf8JsonWriter writer, Result<T> value, JsonSerializerOptions options)
    {
        JsonSerializerOptions valueOptions = GetValueOptions(options);

        writer.WriteStartObject();
        writer.WriteBoolean("isSuccess", value.IsSuccess);
        writer.WritePropertyName("value");
        JsonSerializer.Serialize(writer, value.Value, valueOptions);
        writer.WritePropertyName("error");
        JsonSerializer.Serialize(writer, value.Error, options);
        writer.WritePropertyName("meta");
        JsonSerializer.Serialize(writer, value.Meta, options);
        writer.WriteEndObject();
    }
}

public sealed class Result
{
    public Result()
    {
    }

    private Result(bool isSuccess, string? error, Dictionary<string, object>? meta)
    {
        (IsSuccess, Error, Meta) = (isSuccess, error, meta);
    }

    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public Dictionary<string, object>? Meta { get; set; }

    public static Result Success(Dictionary<string, object>? meta = null)
    {
        return new Result(true, null, meta);
    }

    public static Result Failure(string error, Dictionary<string, object>? meta = null)
    {
        return new Result(false, error, meta);
    }

    public static implicit operator Result(string error)
    {
        return Failure(error);
    }

    public void IfError(Action<string>? action = null)
    {
        if (!IsSuccess)
            if (action != null)
                action(Error ?? "Unknown error");
    }
}