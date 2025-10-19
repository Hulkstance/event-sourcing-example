namespace EventSourcingExample.Events;

// Event that represents a student enrolling in a course.
// This event records the fact that "a student enrolled in a specific course" 
// at a specific point in time. It's an append-only record that cannot be changed.
public class StudentEnrolled : Event
{
    public required Guid StudentId { get; init; }
    public required string CourseName { get; init; }
    
    public override Guid StreamId => StudentId;
}