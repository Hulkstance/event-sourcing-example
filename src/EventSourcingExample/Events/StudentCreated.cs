namespace EventSourcingExample.Events;

// Event that represents the creation of a new student in the system.
// This is typically the first event in a student's event stream.
public class StudentCreated : Event
{
    public required Guid StudentId { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public required DateTime DateOfBirth { get; init; }
    
    public override Guid StreamId => StudentId;
}