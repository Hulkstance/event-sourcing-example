using EventSourcingExample;
using EventSourcingExample.DynamoDB;
using EventSourcingExample.DynamoDB.Events;

var studentDatabase = new StudentDatabase();

var studentId = Guid.Parse("CA603797-30BD-44DC-B6E6-EAC104F2498B");
var studentCreated = new StudentCreated
{
    StudentId = studentId,
    Email = "john.doe@gmail.com",
    FullName = "John Doe",
    DateOfBirth = new DateTime(1990, 1, 1)
};

await studentDatabase.AppendAsync(studentCreated);

var studentEnrolled = new StudentEnrolled
{
    StudentId = studentId,
    CourseName = "From Zero to Hero: REST APIs in .NET"
};
await studentDatabase.AppendAsync(studentEnrolled);

var studentUpdated = new StudentUpdated
{
    StudentId = studentId,
    Email = "john.doe2@gmail.com",
    FullName = "John Doe"
};
await studentDatabase.AppendAsync(studentUpdated);

// Down below is a matter of retrieving it from the database and building that view.
// This idea of doing it on the fly is called a live projection, where you don't really store it
// synchronously or asyonchronously, you just calculate it on the fly.

var student = await studentDatabase.GetStudentAsync(studentId);
//
// var studentFromView = studentDatabase.GetStudentView(studentId);

Console.WriteLine();