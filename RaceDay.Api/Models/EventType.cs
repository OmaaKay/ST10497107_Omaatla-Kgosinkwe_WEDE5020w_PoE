namespace RaceDay.Api.Models;

/// <summary>
/// The kind of race an Event is. Added for Part 2 per the brief's
/// requirement that an event captures "event type (run, walk or cycle)".
/// </summary>
public enum EventType
{
    Run = 0,
    Walk = 1,
    Cycle = 2
}
