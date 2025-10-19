namespace EventSourcingExample.Events;

// Base class for all events in our event sourcing system.
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
public abstract class Event
{
    // The Stream ID identifies which aggregate (entity) this event belongs to.
    // All events for a single aggregate share the same StreamId, creating a chronological
    // sequence of events that tell the complete story of that aggregate's lifecycle.
    public abstract Guid StreamId { get; }
    
    // The timestamp when this event occurred in UTC.
    public DateTime CreatedAtUtc { get; set; }
}