using System.Text.Json.Serialization;

namespace EventSourcingExample.Events;

// The Student aggregate root - this represents the current state of a student
// built by applying all events in chronological order.
// 
// This is what we call a "materialized view" - the current state reconstructed
// from the complete event history. Instead of storing the current state directly,
// we build it by replaying all events that have happened to this student.
// 
// The great way to explain this is if you pull up your phone and you go into your 
// bank account and you see that you have $10, then those $10 are not just $10. 
// It was zero when you opened the bank account. Someone sent you $100, so at the 
// time it was $100. Then you paid for something that cost $50, so your bank account 
// was $50, and then $10 was added, so $60. Then $50 was removed and you're back at $10. 
// So these are all events in a given moment in time where you got money or you spent money.
// 
// To get the final number, all you have to do is say: okay, what are all my transactions 
// in my bank account? Oh, I have these 10 transactions. Let's go ahead and add or remove 
// all that money from whatever it was in the beginning of the balance. Let's say $0.
public class Student
{
    [JsonPropertyName("pk")] public string Pk => $"{Id.ToString()}_view";
    [JsonPropertyName("sk")] public string Sk => $"{Id.ToString()}_view";
    
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public List<string> EnrolledCourses { get; set; } = new();
    public DateTime DateOfBirth { get; set; }
    
    // Applies a StudentCreated event to initialize the student's basic information.
    private void Apply(StudentCreated studentCreated)
    {
        Id = studentCreated.StudentId;
        FullName = studentCreated.FullName;
        Email = studentCreated.Email;
        DateOfBirth = studentCreated.DateOfBirth;
    }
    
    // Applies a StudentUpdated event to modify the student's information.
    private void Apply(StudentUpdated studentUpdated)
    {
        FullName = studentUpdated.FullName;
        Email = studentUpdated.Email;
    }
    
    // Applies a StudentEnrolled event to add a course to the student's enrollment list.
    private void Apply(StudentEnrolled studentEnrolled)
    {
        if (!EnrolledCourses.Contains(studentEnrolled.CourseName))
        {
            EnrolledCourses.Add(studentEnrolled.CourseName);
        }
    }
    
    // Applies a StudentUnEnrolled event to remove a course from the student's enrollment list.
    private void Apply(StudentUnEnrolled studentUnEnrolled)
    {
        if (EnrolledCourses.Contains(studentUnEnrolled.CourseName))
        {
            EnrolledCourses.Remove(studentUnEnrolled.CourseName);
        }
    }
    
    // The main Apply method that routes events to their specific handlers.
    // This is the heart of event sourcing - we take each event that happened
    // and apply it to build up the current state. If we want to reverse that in the future,
    // then we'll go back and have a reversal event but that thing still happened.
    public void Apply(Event @event)
    {
        switch (@event)
        {
            case StudentCreated studentCreated:
                Apply(studentCreated);
                break;
            case StudentUpdated studentUpdated:
                Apply(studentUpdated);
                break;
            case StudentEnrolled studentEnrolled:
                Apply(studentEnrolled);
                break;
            case StudentUnEnrolled studentUnEnrolled:
                Apply(studentUnEnrolled);
                break;
        }
    }
}