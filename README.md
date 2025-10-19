# Event Sourcing Example in .NET

The main idea around event sourcing is that state in our application is not just a row in a database but every time something happens, I store that action, I store that thing, I store that event in the database in an append only fashion. I never go and say update this thing that happened in the past because that thing happened in the past and in a given moment in time it was true. So what I'll be doing is if I want to reverse that in the future, then I'll go back and have a reversal event but that thing still happened.

The great way to explain this is if you pull up your phone and you go into your bank account and you see that you have $10, then those $10 are not just $10. It was zero when you opened the bank account. Someone sent you $100, so at the time it was $100. Then you paid for something that cost $50, so your bank account was $50, and then $10 was added, so $60. Then $50 was removed and you're back at $10. So these are all events in a given moment in time where you got money or you spent money. And a great example of reversing things is what if someone gives you a refund for a product. Well, if you pay $50, and you take out $50, then eventually that refund will replenish those $50 and to get the final number, all you have to do is say: okay, what are all my transactions in my bank account? Oh, I have these 10 transactions. Let's go ahead and add or remove all that money from whatever it was in the beginning of the balance. Let's say $0. Now if that sounds inefficient to do every single time in a real application, there're ways around it but the fundamental concept is that. Things happen and you build State out of the things that happened.

## What This Example Shows

This project demonstrates event sourcing concepts in .NET, inspired by [Nick Chapsas' video](https://www.youtube.com/watch?v=n_o-xuuVtmw). It shows how to:

- Store events in an append-only fashion - Never modify or delete past events
- Build current state from event history - Reconstruct the current state by replaying all events
- Use projections for performance - Maintain materialized views for fast reads

## How It Works

Instead of storing current state, we reconstruct it by applying all events in chronological order:

```csharp
// This is what happens under the hood
var student = new Student();
foreach (var event in studentEvents)
{
    student.Apply(event); // Apply each event to build current state
}
```

## The Code

### Creating a Student

```csharp
var studentCreated = new StudentCreated
{
    StudentId = studentId,
    Email = "john.doe@gmail.com",
    FullName = "John Doe",
    DateOfBirth = new DateTime(1990, 1, 1)
};

studentDatabase.Append(studentCreated);
```

### Enrolling in a Course

```csharp
var studentEnrolled = new StudentEnrolled
{
    StudentId = studentId,
    CourseName = "From Zero to Hero: REST APIs in .NET"
};

studentDatabase.Append(studentEnrolled);
```

### Getting Current State

```csharp
// Rebuild from all events (slower but always accurate)
var student = studentDatabase.GetStudent(studentId);

// Get from projection (faster but may be eventually consistent)
var studentFromView = studentDatabase.GetStudentView(studentId);
```

## Why Event Sourcing?

1. **Complete Audit Trail** - Every change is recorded with timestamps
2. **Time Travel** - Reconstruct state at any point in time
3. **Debugging** - See exactly what happened and when
4. **Flexibility** - Add new projections without changing event storage

## Performance

Since rebuilding state from all events can be expensive, we maintain projections:

- **Synchronous Projections** - Updated immediately when events are stored
- **Asynchronous Projections** - Updated via background processes

The DynamoDB example shows how to implement event sourcing in production with proper transactions and atomic operations.