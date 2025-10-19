using System.Text.Json.Serialization;

namespace EventSourcingExample.DynamoDB.Events;

// We should let this class know that it is inherited by some other classes, so the
// JSON serializer/deserializer knows how to give us the right type back when we say
// serialize this event back into a Student event.
[JsonPolymorphic]
[JsonDerivedType(typeof(StudentCreated), nameof(StudentCreated))]
[JsonDerivedType(typeof(StudentUpdated), nameof(StudentUpdated))]
[JsonDerivedType(typeof(StudentEnrolled), nameof(StudentEnrolled))]
[JsonDerivedType(typeof(StudentUnEnrolled), nameof(StudentUnEnrolled))]
public abstract class Event
{
    public abstract Guid StreamId { get; }

    public DateTime CreatedAtUtc { get; set; }
    
    [JsonPropertyName("pk")] public string Pk => StreamId.ToString();
 
    [JsonPropertyName("sk")] public string Sk => CreatedAtUtc.ToString("O"); // ISO 8601
}