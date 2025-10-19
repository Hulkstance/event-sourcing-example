namespace EventSourcingExample.Events;

// Event that represents a student unenrolling from a course.
// This is a great example of reversing things - if someone gives you a refund for a product. 
// Well, if you pay $50, and you take out $50, then eventually that refund will replenish those $50. 
// The original payment event still exists in history, but the unenrollment event records the reversal.
public class StudentUnEnrolled : Event
{
    public required Guid StudentId { get; init; }
    public required string CourseName { get; init; }
    
    public override Guid StreamId => StudentId;
}