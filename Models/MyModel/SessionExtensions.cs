using System.Text.Json;
using Microsoft.AspNetCore.Http;
namespace RahalWeb.Models.MyModel
{
    public static class SessionExtensions
    {
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // Retrieves an object from Session by deserializing JSON
        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);

            return value is null ? default! : JsonSerializer.Deserialize<T>(value);
        }
    }
}
