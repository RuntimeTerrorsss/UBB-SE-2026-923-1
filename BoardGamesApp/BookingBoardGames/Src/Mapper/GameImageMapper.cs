using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardGames.Src.Mapper
{
    public static class GameImageMapper
    {
        private static readonly Dictionary<string, string> ImageUrls = new()
    {
        { "Catan", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
    };

        private const string FallbackUrl = "https://placehold.co/220x180?text=No+Image";

        public static string GetImageUrl(string gameName)
        {
            return ImageUrls.TryGetValue(gameName, out var url) ? url : FallbackUrl;
        }
    }
}
