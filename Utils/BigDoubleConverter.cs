using Newtonsoft.Json;
using BreakInfinity;

public class BigDoubleConverter : JsonConverter<BigDouble>
{
    public override void WriteJson(JsonWriter writer, BigDouble value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public override BigDouble ReadJson(JsonReader reader, Type objectType, BigDouble existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        string s = (string)reader.Value;
        return BigDouble.Parse(s);
    }
}
