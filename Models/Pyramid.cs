using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Midterms.Models;

/// <summary>
/// Represents a historic Egyptian pyramid promoted for tourism.
/// Maps to the 'pyramids' table in Supabase.
/// </summary>
[Table("pyramids")]
public class Pyramid : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("location")]
    public string Location { get; set; } = string.Empty;

    [Column("height")]
    public double Height { get; set; } // Height in meters

    [Column("built_era")]
    public string BuiltEra { get; set; } = string.Empty;

    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("entry_fee_usd")]
    public decimal EntryFeeUSD { get; set; }

    [Column("image_url")]
    public string ImageUrl { get; set; } = string.Empty;

    [Column("rating")]
    public double Rating { get; set; }
}
