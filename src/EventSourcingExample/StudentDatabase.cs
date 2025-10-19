using EventSourcingExample.Events;

namespace EventSourcingExample;

// In-memory event store and projection system for our event sourcing example.
// 
// The main idea around event sourcing is that state in our application is not just a row in a database 
// but every time something happens, I store that action, I store that thing, I store that event in the 
// database in an append only fashion. I never go and say update this thing that happened in the past 
// because that thing happened in the past and in a given moment in time it was true.
// 
// This also gives us a form of auditing by default - every change is recorded with timestamps.
public class StudentDatabase
{
    // The event store - a dictionary where each key is a StreamId (student ID) and the value
    // is a SortedList of events ordered by timestamp. This ensures events are always retrieved
    // in chronological order, which is crucial for correct state reconstruction.
    private readonly Dictionary<Guid, SortedList<DateTime, Event>> _studentEvents = new();
    
    // The projection store - a materialized view that contains the current state of each student.
    // This is what's called a projection - a pre-computed view of the current state
    // that avoids the cost of rebuilding state from all events every time.
    private readonly Dictionary<Guid, Student> _students = new();
    
    // Appends a new event to the event store and updates the projection.
    // 
    // The great way to explain this is if you pull up your phone and you go into your bank account 
    // and you see that you have $10, then those $10 are not just $10. It was zero when you opened 
    // the bank account. Someone sent you $100, so at the time it was $100. Then you paid for something 
    // that cost $50, so your bank account was $50, and then $10 was added, so $60. Then $50 was removed 
    // and you're back at $10. So these are all events in a given moment in time where you got money 
    // or you spent money.
    public void Append(Event @event)
    {
        var stream = _studentEvents!.GetValueOrDefault(@event.StreamId, null);
        if (stream is null)
        {
            _studentEvents[@event.StreamId] = new SortedList<DateTime, Event>();
        }
        
        // I'm saying add this at the bottom of this sorted list based on the date it was added.
        @event.CreatedAtUtc = DateTime.UtcNow;
        _studentEvents[@event.StreamId].Add(@event.CreatedAtUtc, @event);
        
        // Synchronous projection. It's synchronous because we're going to be updating that state
        // as we append new events.
        // Now whether we're going to do it synchronously or asynchronously depends on the scenario we're into.
        // If we're in a read heavy scenario, or if we're in a write heavy scenario, or also if we can
        // deal with some eventual consistency or if we absolutely need strong consistency.
        // NOTE: if you're in a real database scenario, you would have to wrap this into a transaction
        // because the addition to the database of that event and the update of your current state
        // should happen at the exact same time, and they should succeed together or fail together,
        // so it should be atomic.
        _students[@event.StreamId] = GetStudent(@event.StreamId)!;
    }

    // Gets the current state of a student from the projection (materialized view).
    public Student? GetStudentView(Guid studentId)
    {
        return _students!.GetValueOrDefault(studentId, null);
    }
    
    // Rebuilds the current state of a student by applying all events in chronological order.
    // 
    // To get the final number, all you have to do is say: okay, what are all my transactions 
    // in my bank account? Oh, I have these 10 transactions. Let's go ahead and add or remove 
    // all that money from whatever it was in the beginning of the balance. Let's say $0.
    // 
    // Now if that sounds inefficient to do every single time in a real application, there're 
    // ways around it but the fundamental concept is that. Things happen and you build State 
    // out of the things that happened.
    public Student? GetStudent(Guid studentId)
    {
        if (!_studentEvents.ContainsKey(studentId))
        {
            return null;
        }
        
        // Start with a fresh student and apply all events in order
        var student = new Student();
        var studentEvents = _studentEvents[studentId];
        
        // Apply each event in chronological order to build the current state
        foreach (var studentEvent in studentEvents)
        {
            student.Apply(studentEvent.Value);
        }

        return student;
    }
}