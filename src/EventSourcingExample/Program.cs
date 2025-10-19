using EventSourcingExample;
using EventSourcingExample.Events;

// Demo application demonstrating event sourcing concepts.
// 
// The main idea around event sourcing is that state in our application is not just a row in a database 
// but every time something happens, I store that action, I store that thing, I store that event in the 
// database in an append only fashion. I never go and say update this thing that happened in the past 
// because that thing happened in the past and in a given moment in time it was true.
// 
// The great way to explain this is if you pull up your phone and you go into your bank account and you see 
// that you have $10, then those $10 are not just $10. It was zero when you opened the bank account. 
// Someone sent you $100, so at the time it was $100. Then you paid for something that cost $50, so your 
// bank account was $50, and then $10 was added, so $60. Then $50 was removed and you're back at $10. 
// So these are all events in a given moment in time where you got money or you spent money.

// Initialize our event store and projection system
var studentDatabase = new StudentDatabase();

// Create a unique identifier for our student (this is the StreamId)
var studentId = Guid.Parse("CA603797-30BD-44DC-B6E6-EAC104F2498B");

// === STEP 1: Create a Student ===
// This is the first event in our student's event stream
// It represents the fact that "a student was created" at this point in time
var studentCreated = new StudentCreated
{
    StudentId = studentId,
    Email = "john.doe@gmail.com",
    FullName = "John Doe",
    DateOfBirth = new DateTime(1990, 1, 1)
};

// Append the event to the store - this will also update the projection
studentDatabase.Append(studentCreated);

// === STEP 2: Enroll Student in a Course ===
// This is another event that happened to our student
// Notice how we're not "updating" the student record - we're recording what happened
var studentEnrolled = new StudentEnrolled
{
    StudentId = studentId,
    CourseName = "From Zero to Hero: REST APIs in .NET"
};
studentDatabase.Append(studentEnrolled);

// === STEP 3: Update Student Information ===
// Even updates are recorded as events, not direct modifications
// This preserves the history of what the email was before
var studentUpdated = new StudentUpdated
{
    StudentId = studentId,
    Email = "john.doe2@gmail.com",
    FullName = "John Doe"
};
studentDatabase.Append(studentUpdated);

// === STEP 4: Rebuild State from Events ===
// This demonstrates the core of event sourcing - we rebuild the current state
// by applying all events in chronological order
// A materialized view that represents that student
var student = studentDatabase.GetStudent(studentId);

// So we basically have a stream per aggregate to represents that student.
// And then the Student is also the aggregate root.
// We have the ability to append only what happens to that student,
// and then build the materialized view to represent that student based on what happened.

// === STEP 5: Use Projection for Performance ===
// Now, isn't it a bit costly to just go ahead and pull all the events from a stream
// for this specific aggregate (student) and build it every single time?
// Wouldn't it be easier to just sort of keep the state or a snapshot or a materialized view
// of that thing in the database or maybe in our system, and then just return that?
// The answer is yes. And that's what's called a projection.

// This gets the pre-computed state from our projection (much faster)
var studentFromView = studentDatabase.GetStudentView(studentId);

// Both methods should return the same result, but the projection is faster
// because it doesn't need to replay all events

Console.WriteLine("Event Sourcing Demo Complete!");
Console.WriteLine($"Student ID: {student?.Id}");
Console.WriteLine($"Student Name: {student?.FullName}");
Console.WriteLine($"Student Email: {student?.Email}");
Console.WriteLine($"Enrolled Courses: {string.Join(", ", student?.EnrolledCourses ?? new List<string>())}");
Console.WriteLine($"Date of Birth: {student?.DateOfBirth:yyyy-MM-dd}");