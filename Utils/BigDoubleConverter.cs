using Newtonsoft.Json;
using BreakInfinity;

public class BigDoubleConverter : JsonConverter<BigDouble>
{
    public override void WriteJson(JsonWriter writer, BigDouble value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
    public override BigDouble ReadJson(JsonReader reader,
        Type objectType,
        BigDouble existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null || reader.Value == null)
            return BigDouble.Zero;

        string s = reader.Value.ToString();
        if (string.IsNullOrWhiteSpace(s))
            return BigDouble.Zero;

        return BigDouble.Parse(s);
    }

}
