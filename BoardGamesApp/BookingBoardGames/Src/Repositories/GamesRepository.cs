using BookingBoardGames.Data;
using BookingBoardGames.Src.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using BookingBoardGames.Src.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Repository responsible for reading game/listing data from the database.
/// Important:
/// - This repository only reads data.
/// - It is used by the service layer, not directly by the UI.
/// How ADO.NET handles connections:
/// - When you write using var connection = new SqlConnection(...) and call .Open(), Microsoft checks the pool, so the pool of connections is handled by .net
/// - If there is a free connection, it gives it to you.
/// - When your "using" block finishes, it calls .Close().
/// - Microsoft intercepts your .Close() command. It doesn't actually destroy the connection to the database. It just wipes the data clean and parks it back in the hidden pool for.  the next person to use.
/// </summary>
public class GamesRepository : InterfaceGamesRepository
{
    /// <summary>
    /// Represents the ID used for unauthenticated users.
    /// </summary>
    public const int AnonymousUserId = -1;

    private readonly AppDbContext _context;

    public GamesRepository(AppDbContext context) { _context = context; }

    /// <summary>
    /// Gets a single game by its database id.
    /// </summary>
    /// <param name="id">The unique id of the game.</param>
    /// <returns>The game object if found; otherwise, null.</returns>
    /// <remarks>
    /// Use this when you already know the exact game id and need full game details.
    /// </remarks>
    public Game? GetGameById(int id)
    {
        return _context.Games.FirstOrDefault(g => g.Id == id);
    }

    public decimal GetPriceGameById(int gameId)
    {
        return _context.Games.Where(g => g.Id == gameId).Select(g => g.PricePerDay).FirstOrDefault();
    }

    /// <summary>
    /// Gets all active games that are visible in the system.
    /// </summary>
    /// <returns>A list of all active games.</returns>
    public List<Game> GetAll()
    {
        return GetAllActiveGames(AnonymousUserId);

    }

    /// <summary>
    /// Gets games that match the provided filter criteria.
    /// </summary>
    /// <param name="filter">
    /// Object containing user-entered search/filter values.
    /// All fields may be empty/null.
    /// </param>
    /// <returns>A list of games matching the filter.</returns>
    /// <remarks>
    /// Use this for:
    /// - search page
    /// - filter panel
    /// - search + filters combined
    /// Behavior:
    /// - null/empty fields are ignored
    /// - only active games are returned
    /// - user's own games are excluded if UserId is provided
    /// - if an availability range is provided, only games available in that range are returned.
    /// </remarks>
    public List<Game> GetGamesByFilter(FilterCriteria filter)
        {
        var userId = filter.UserId ?? AnonymousUserId;
        var query = _context.Games.Include(g => g.Owner).Where(g => g.IsActive && g.OwnerId != userId);
        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(g => g.Name.Contains(filter.Name));
        if (!string.IsNullOrWhiteSpace(filter.City))
            query = query.Where(g => g.Owner!.City == filter.City);
        if (filter.MaximumPrice.HasValue)
            query = query.Where(g => g.PricePerDay <= filter.MaximumPrice.Value);
        if (filter.PlayerCount.HasValue)
            query = query.Where(g => g.MinimumPlayerNumber <= filter.PlayerCount.Value && g.MaximumPlayerNumber >= filter.PlayerCount.Value);
        if (filter.AvailabilityRange != null)
        { var start = filter.AvailabilityRange.StartTime; var end = filter.AvailabilityRange.EndTime; query = query.Where(g => !g.Rentals.Any(r => r.StartDate < end && r.EndDate > start)); }
        return query.ToList();
    }

    /// <summary>
    /// Gets games that are available starting today and continuing through tomorrow.
    /// </summary>
    /// <param name="userId">
    /// Current authenticated user id OR -1 for user not logged in.
    /// Used to exclude the user's own games from the feed.
    /// </param>
    /// <returns>A list of games for the "Available Tonight" section.</returns>
    public List<Game> GetGamesForFeedAvailableTonight(int userId)
    {
        var today = DateTime.Today; var tomorrow = today.AddDays(1);
        return _context.Games.Include(g => g.Owner).Where(g => g.IsActive && g.OwnerId != userId && !g.Rentals.Any(r => r.StartDate < tomorrow && r.EndDate > today)).ToList();
    }

        /// <summary>
        /// Gets all remaining active games that are not part of the "Available Tonight" section.
        /// </summary>
        /// <param name="userId">
        /// Current authenticated user id.
        /// Used to exclude the user's own games from the feed.
        /// </param>
        /// <returns>A list of games for the "Available Tonight" section.</returns>
    public List<Game> GetRemainingGamesForFeed(int userId)
    {
        var today = DateTime.Today; var tomorrow = today.AddDays(1);
        return _context.Games.Where(g => g.IsActive && g.OwnerId != userId && g.Rentals.Any(r => r.StartDate < tomorrow && r.EndDate > today)).ToList();
    }

    // Used to convert game data to Game object
    private static Game ConvertGameDataToGameObject(SqlDataReader reader)
    {
        return new Game
        {
            Id = Convert.ToInt32(reader["game_id"]),
            Name = Convert.ToString(reader["name"]) ?? string.Empty,
            PricePerDay = Convert.ToDecimal(reader["price"]),
            MinimumPlayerNumber = Convert.ToInt32(reader["minimum_player_number"]),
            MaximumPlayerNumber = Convert.ToInt32(reader["maximum_player_number"]),
            Description = Convert.ToString(reader["description"]) ?? string.Empty,
            Image = reader["image"] == DBNull.Value ? null : (byte[])reader["image"],
            IsActive = Convert.ToBoolean(reader["is_active"]),
            OwnerId = Convert.ToInt32(reader["owner_id"]),
        };
    }

    /// Gets all active games from the database.
    /// <param name="userId">
    /// Current authenticated user id OR -1 for user not logged in.
    /// Used to exclude the user's own games from the results.
    /// </param>
    /// <returns>A list of all active games.</returns>
    private List<Game> GetAllActiveGames(int userId)
    {
        return _context.Games.Include(g => g.Owner).Where(g => g.IsActive && g.OwnerId != userId).ToList();
    }
}
