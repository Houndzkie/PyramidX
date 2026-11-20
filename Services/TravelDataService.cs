using Midterms.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Midterms.Services;

/// <summary>
/// Scoped service providing state management for the Egypt Pyramids Tourism portal.
/// Seamlessly connects to Supabase database when configured in appsettings.json,
/// and provides local in-memory fallback data if credentials are not yet set.
/// </summary>
public class TravelDataService
{
    public event Action? OnChange;

    private readonly IConfiguration _configuration;
    private readonly ILogger<TravelDataService> _logger;
    private Supabase.Client? _supabaseClient;
    private bool _isInitializing = false;
    private bool _hasInitialized = false;

    public List<Pyramid> Pyramids { get; private set; } = new();
    public List<TourBooking> Bookings { get; private set; } = new();
    public List<User> RegisteredUsers { get; private set; } = new();
    public User? CurrentUser { get; private set; }

    public bool IsAuthenticated => CurrentUser != null;
    public bool IsSupabaseConnected { get; private set; } = false;
    public string ConnectionStatusText => IsSupabaseConnected 
        ? "Connected to Supabase Cloud Database" 
        : "Local Demo Mode (Configure Supabase in appsettings.json to sync)";

    private int _nextBookingId = 103;
    private int _nextUserId = 2;

    public TravelDataService(IConfiguration configuration, ILogger<TravelDataService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Initialize with default fallback data immediately so UI renders with 0 delay
        InitializeFallbackSeedData();
    }

    /// <summary>
    /// Fallback in-memory seed data used when Supabase is not configured or offline.
    /// </summary>
    private void InitializeFallbackSeedData()
    {
        Pyramids = new List<Pyramid>
        {
            new Pyramid
            {
                Id = 1,
                Name = "Great Pyramid of Giza",
                Location = "Giza Plateau, Greater Cairo",
                Height = 146.6,
                BuiltEra = "Old Kingdom, 4th Dynasty (~2560 BC)",
                Description = "The monumental tomb of Pharaoh Khufu. The oldest and sole surviving wonder of the original Seven Wonders of the Ancient World, constructed with over 2.3 million megalithic limestone blocks.",
                EntryFeeUSD = 25.00m,
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e7/Great_Pyramid_of_Giza_-_Pyramid_of_Khufu.jpg/1280px-Great_Pyramid_of_Giza_-_Pyramid_of_Khufu.jpg",
                Rating = 4.95
            },
            new Pyramid
            {
                Id = 2,
                Name = "Pyramid of Khafre",
                Location = "Giza Plateau, Greater Cairo",
                Height = 136.4,
                BuiltEra = "Old Kingdom, 4th Dynasty (~2570 BC)",
                Description = "Built for Pharaoh Khafre, son of Khufu. Perched on higher bedrock and still crowned by its lustrous original polished Tura limestone casing stones, guarded by the iconic Great Sphinx.",
                EntryFeeUSD = 20.00m,
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/6/69/Pyramid_of_Khafre_and_Sphinx%2C_Giza%2C_Greater_Cairo%2C_Egypt.jpg",
                Rating = 4.88
            },
            new Pyramid
            {
                Id = 3,
                Name = "Step Pyramid of Djoser",
                Location = "Saqqara Necropolis, Giza",
                Height = 62.5,
                BuiltEra = "Old Kingdom, 3rd Dynasty (~2670 BC)",
                Description = "Designed by the legendary architect Imhotep for Pharaoh Djoser. Widely celebrated as humanity's very first colossal stone monument, pioneering a revolutionary six-tiered mastaba design.",
                EntryFeeUSD = 18.00m,
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/6/6a/Sakkara_01.jpg/1280px-Sakkara_01.jpg",
                Rating = 4.82
            }
        };

        var defaultUser = new User
        {
            Id = 1,
            Username = "RamsesExplorer",
            Email = "ramses@pyramids.eg",
            Password = "Password123!",
            Preferences = new List<string> { "Archaeology", "Sunset Tours", "VIP Tomb Access", "Photography" }
        };
        RegisteredUsers.Add(defaultUser);
        CurrentUser = defaultUser;

        Bookings = new List<TourBooking>
        {
            new TourBooking
            {
                Id = 101,
                PyramidId = 1,
                PyramidName = "Great Pyramid of Giza",
                TourDate = DateTime.Today.AddDays(2),
                NumberOfTickets = 2,
                TotalPrice = 50.00m,
                Status = "Confirmed"
            },
            new TourBooking
            {
                Id = 102,
                PyramidId = 3,
                PyramidName = "Step Pyramid of Djoser",
                TourDate = DateTime.Today.AddDays(5),
                NumberOfTickets = 3,
                TotalPrice = 54.00m,
                Status = "Confirmed"
            }
        };
    }

    /// <summary>
    /// Initializes Supabase connection and fetches real-time data from PostgreSQL tables.
    /// </summary>
    public async Task EnsureInitializedAsync()
    {
        if (_hasInitialized || _isInitializing) return;

        _isInitializing = true;
        try
        {
            var url = _configuration["Supabase:Url"];
            var key = _configuration["Supabase:Key"];

            if (!string.IsNullOrWhiteSpace(url) && 
                !string.IsNullOrWhiteSpace(key) && 
                !url.Contains("YOUR_PROJECT_ID", StringComparison.OrdinalIgnoreCase))
            {
                var options = new Supabase.SupabaseOptions
                {
                    AutoRefreshToken = true,
                    AutoConnectRealtime = false
                };

                _supabaseClient = new Supabase.Client(url, key, options);
                await _supabaseClient.InitializeAsync();

                // Fetch real Pyramids from Supabase
                try
                {
                    var pyramidsRes = await _supabaseClient.From<Pyramid>().Get();
                    if (pyramidsRes.Models.Any())
                    {
                        Pyramids = pyramidsRes.Models.OrderBy(p => p.Id).ToList();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not fetch pyramids from Supabase. Using default seed.");
                }

                // Fetch real Users from Supabase
                try
                {
                    var usersRes = await _supabaseClient.From<User>().Get();
                    if (usersRes.Models.Any())
                    {
                        RegisteredUsers = usersRes.Models;
                        if (CurrentUser != null)
                        {
                            var matched = RegisteredUsers.FirstOrDefault(u => u.Username.Equals(CurrentUser.Username, StringComparison.OrdinalIgnoreCase));
                            if (matched != null) CurrentUser = matched;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not fetch users from Supabase.");
                }

                // Fetch real Bookings from Supabase
                try
                {
                    var bookingsRes = await _supabaseClient.From<TourBooking>().Get();
                    if (bookingsRes.Models.Any())
                    {
                        Bookings = bookingsRes.Models.OrderByDescending(b => b.Id).ToList();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not fetch bookings from Supabase.");
                }

                IsSupabaseConnected = true;
                _logger.LogInformation("Successfully connected to Supabase database.");
                NotifyStateChanged();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to Supabase. Running in local fallback mode.");
            IsSupabaseConnected = false;
        }
        finally
        {
            _isInitializing = false;
            _hasInitialized = true;
        }
    }

    /// <summary>
    /// Authenticate existing user by username or email.
    /// </summary>
    public async Task<(bool Success, string? Error)> LoginAsync(string usernameOrEmail, string password)
    {
        await EnsureInitializedAsync();

        if (IsSupabaseConnected && _supabaseClient != null)
        {
            try
            {
                var response = await _supabaseClient.From<User>()
                    .Where(u => u.Password == password)
                    .Get();

                var matchedUser = response.Models.FirstOrDefault(u =>
                    string.Equals(u.Username, usernameOrEmail, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(u.Email, usernameOrEmail, StringComparison.OrdinalIgnoreCase));

                if (matchedUser != null)
                {
                    CurrentUser = matchedUser;
                    NotifyStateChanged();
                    return (true, null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Supabase login query error, checking local store.");
            }
        }

        // Local store fallback
        var localUser = RegisteredUsers.FirstOrDefault(u =>
            (string.Equals(u.Username, usernameOrEmail, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(u.Email, usernameOrEmail, StringComparison.OrdinalIgnoreCase)) &&
            u.Password == password);

        if (localUser != null)
        {
            CurrentUser = localUser;
            NotifyStateChanged();
            return (true, null);
        }

        return (false, "Invalid credentials. Please verify your username/email and password.");
    }

    /// <summary>
    /// Synchronous login wrapper for existing page bindings.
    /// </summary>
    public bool Login(string usernameOrEmail, string password, out string? errorMessage)
    {
        var task = LoginAsync(usernameOrEmail, password);
        var (success, error) = task.GetAwaiter().GetResult();
        errorMessage = error;
        return success;
    }

    /// <summary>
    /// Register a new user and sync with Supabase.
    /// </summary>
    public async Task<(bool Success, string? Error)> RegisterAsync(User newUser)
    {
        await EnsureInitializedAsync();

        // Check duplicates locally first
        if (RegisteredUsers.Any(u => string.Equals(u.Username, newUser.Username, StringComparison.OrdinalIgnoreCase)))
        {
            return (false, "This username is already taken. Please choose another one.");
        }

        if (RegisteredUsers.Any(u => string.Equals(u.Email, newUser.Email, StringComparison.OrdinalIgnoreCase)))
        {
            return (false, "An account with this email address already exists.");
        }

        if (IsSupabaseConnected && _supabaseClient != null)
        {
            try
            {
                var insertRes = await _supabaseClient.From<User>().Insert(newUser);
                if (insertRes.Models.Any())
                {
                    newUser = insertRes.Models.First();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert user into Supabase. Adding to local cache.");
            }
        }
        else
        {
            newUser.Id = _nextUserId++;
        }

        RegisteredUsers.Add(newUser);
        CurrentUser = newUser;
        NotifyStateChanged();
        return (true, null);
    }

    /// <summary>
    /// Synchronous register wrapper for existing page bindings.
    /// </summary>
    public bool Register(User newUser, out string? errorMessage)
    {
        var task = RegisterAsync(newUser);
        var (success, error) = task.GetAwaiter().GetResult();
        errorMessage = error;
        return success;
    }

    /// <summary>
    /// Log out the current user session.
    /// </summary>
    public void Logout()
    {
        CurrentUser = null;
        NotifyStateChanged();
    }

    /// <summary>
    /// Create and record a new tour booking in Supabase and local cache.
    /// </summary>
    public async Task<TourBooking?> CreateBookingAsync(int pyramidId, DateTime tourDate, int numberOfTickets)
    {
        await EnsureInitializedAsync();

        var pyramid = Pyramids.FirstOrDefault(p => p.Id == pyramidId);
        if (pyramid == null || numberOfTickets < 1)
        {
            return null;
        }

        var booking = new TourBooking
        {
            PyramidId = pyramid.Id,
            PyramidName = pyramid.Name,
            TourDate = tourDate,
            NumberOfTickets = numberOfTickets,
            TotalPrice = pyramid.EntryFeeUSD * numberOfTickets,
            Status = "Confirmed"
        };

        if (IsSupabaseConnected && _supabaseClient != null)
        {
            try
            {
                var insertRes = await _supabaseClient.From<TourBooking>().Insert(booking);
                if (insertRes.Models.Any())
                {
                    booking = insertRes.Models.First();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert booking into Supabase. Storing locally.");
                booking.Id = _nextBookingId++;
            }
        }
        else
        {
            booking.Id = _nextBookingId++;
        }

        Bookings.Insert(0, booking);
        NotifyStateChanged();
        return booking;
    }

    /// <summary>
    /// Synchronous create booking wrapper for existing page bindings.
    /// </summary>
    public TourBooking? CreateBooking(int pyramidId, DateTime tourDate, int numberOfTickets)
    {
        return CreateBookingAsync(pyramidId, tourDate, numberOfTickets).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Cancel an active booking by ID in Supabase and local state.
    /// </summary>
    public async Task<bool> CancelBookingAsync(int bookingId)
    {
        await EnsureInitializedAsync();

        var booking = Bookings.FirstOrDefault(b => b.Id == bookingId);
        if (booking != null)
        {
            booking.Status = "Cancelled";

            if (IsSupabaseConnected && _supabaseClient != null)
            {
                try
                {
                    await _supabaseClient.From<TourBooking>()
                        .Where(b => b.Id == bookingId)
                        .Set(b => b.Status, "Cancelled")
                        .Update();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update booking status in Supabase.");
                }
            }

            NotifyStateChanged();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Synchronous cancel booking wrapper.
    /// </summary>
    public bool CancelBooking(int bookingId)
    {
        return CancelBookingAsync(bookingId).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Delete a booking entirely from Supabase and local state.
    /// </summary>
    public async Task<bool> DeleteBookingAsync(int bookingId)
    {
        await EnsureInitializedAsync();

        if (IsSupabaseConnected && _supabaseClient != null)
        {
            try
            {
                await _supabaseClient.From<TourBooking>()
                    .Where(b => b.Id == bookingId)
                    .Delete();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete booking from Supabase.");
            }
        }

        var removed = Bookings.RemoveAll(b => b.Id == bookingId) > 0;
        if (removed)
        {
            NotifyStateChanged();
        }
        return removed;
    }

    /// <summary>
    /// Synchronous delete booking wrapper.
    /// </summary>
    public bool DeleteBooking(int bookingId)
    {
        return DeleteBookingAsync(bookingId).GetAwaiter().GetResult();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
