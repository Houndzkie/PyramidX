using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Midterms.Models;

/// <summary>
/// Represents an authenticated or registered user in the Egypt Pyramids Tourism portal.
/// Maps to the 'users' table in Supabase.
/// </summary>
[Table("users")]
public class User : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    [Column("password")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// User's chosen travel interest tags (e.g. Archaeology, Photography, Sunset Tours).
    /// </summary>
    [Column("preferences")]
    public List<string> Preferences { get; set; } = new();
}
