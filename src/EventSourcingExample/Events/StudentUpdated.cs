namespace EventSourcingExample.Events;

// Event that represents an update to a student's information.
// Instead of directly modifying the student record, we record the fact that
// "the student's information was updated" at a specific point in time.
public class StudentUpdated : Event
{
    public required Guid StudentId { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
    
    public override Guid StreamId => StudentId;
}