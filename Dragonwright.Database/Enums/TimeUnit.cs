using Dragonwright.Database.Entities.Models;

namespace Dragonwright.Database.Enums;

public enum TimeUnit
{
    Action,
    BonusAction,
    Reaction,
    Round,
    Minute,
    Hour,
    Day,
    Special,
    /// <summary>
    /// Regardless of the value of <see cref="Time.Value"/>, this indicates something that happens instantly.
    /// </summary>
    Instantaneous,
    /// <summary>
    /// These are special time units that indicate something happens at the start or end of a turn.
    /// If the value of <see cref="Time.Value"/> is 0, it means it happens immediately at that turn phase,
    /// anything above 0 indicates the number of turns to wait.
    /// </summary>
    EndOfTurn,
    /// <summary>
    /// These are special time units that indicate something happens at the start or end of a turn.
    /// If the value of <see cref="Time.Value"/> is 0, it means it happens immediately at that turn phase,
    /// anything above 0 indicates the number of turns to wait.
    /// </summary>
    StartOfTurn
}