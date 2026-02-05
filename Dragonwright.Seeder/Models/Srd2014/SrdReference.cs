namespace Dragonwright.Seeder.Models.Srd2014;

/// <summary>
/// Common reference object used throughout the SRD JSON files.
/// </summary>
public class SrdReference
{
    public string Index { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

/// <summary>
/// Reference with a quantity for starting equipment.
/// </summary>
public class SrdCountedReference
{
    public SrdReference? Equipment { get; set; }
    public SrdReference? Of { get; set; }
    public int Quantity { get; set; }
    public int Count { get; set; }
}

/// <summary>
/// A choice selection from options.
/// </summary>
public class SrdChoice
{
    public string? Desc { get; set; }
    public int Choose { get; set; }
    public string? Type { get; set; }
    public SrdOptionSet? From { get; set; }
}

/// <summary>
/// Set of options for choices.
/// </summary>
public class SrdOptionSet
{
    public string? OptionSetType { get; set; }
    public List<SrdOption>? Options { get; set; }
    public SrdReference? EquipmentCategory { get; set; }
    public string? ResourceListUrl { get; set; }
}

/// <summary>
/// Individual option in a choice.
/// </summary>
public class SrdOption
{
    public string? OptionType { get; set; }
    public SrdReference? Item { get; set; }
    public int? Count { get; set; }
    public SrdReference? Of { get; set; }
    public SrdChoice? Choice { get; set; }
    public string? String { get; set; }
    public string? Desc { get; set; }
    public List<SrdReference>? Alignments { get; set; }
    public List<SrdOptionItem>? Items { get; set; }
    public string? Unit { get; set; }
}

public class SrdOptionItem
{
    public string? OptionType { get; set; }
    public int? Count { get; set; }
    public SrdReference? Of { get; set; }
    public string? Note { get; set; }
    public string? Unit { get; set; }
}
