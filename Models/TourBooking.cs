using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Midterms.Models;

/// <summary>
/// Represents a tourist booking reservation for an Egyptian pyramid tour.
/// Maps to the 'tour_bookings' table in Supabase.
/// </summary>
[Table("tour_bookings")]
public class TourBooking : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("pyramid_id")]
    public int PyramidId { get; set; }

    [Column("pyramid_name")]
    public string PyramidName { get; set; } = string.Empty;

    [Column("tour_date")]
    public DateTime TourDate { get; set; } = DateTime.Today.AddDays(1);

    [Column("number_of_tickets")]
    public int NumberOfTickets { get; set; } = 1;

    [Column("total_price")]
    public decimal TotalPrice { get; set; }

    [Column("status")]
    public string Status { get; set; } = "Confirmed";
}
