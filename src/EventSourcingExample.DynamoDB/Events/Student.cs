namespace EventSourcingExample.DynamoDB.Events;

public class Student
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public List<string> EnrolledCourses { get; set; } = new();
    public DateTime DateOfBirth { get; set; }
    
    private void Apply(StudentCreated studentCreated)
    {
        Id = studentCreated.StudentId;
        FullName = studentCreated.FullName;
        Email = studentCreated.Email;
        DateOfBirth = studentCreated.DateOfBirth;
    }
    
    private void Apply(StudentUpdated studentUpdated)
    {
        FullName = studentUpdated.FullName;
        Email = studentUpdated.Email;
    }
    
    private void Apply(StudentEnrolled studentEnrolled)
    {
        if (!EnrolledCourses.Contains(studentEnrolled.CourseName))
        {
            EnrolledCourses.Add(studentEnrolled.CourseName);
        }
    }
    
    private void Apply(StudentUnEnrolled studentUnEnrolled)
    {
        if (EnrolledCourses.Contains(studentUnEnrolled.CourseName))
        {
            EnrolledCourses.Remove(studentUnEnrolled.CourseName);
        }
    }
    
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