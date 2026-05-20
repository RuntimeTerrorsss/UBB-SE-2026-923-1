using BookingBoardGames.Data.Enum;
using BookingBoardGames.Sharing.DTO;
using BookingBoardGames.Sharing.Services;
using BookingBoardGames.Web.Models.Search;
using Microsoft.AspNetCore.Mvc;

namespace BookingBoardGames.Web.Controllers
{
    public class SearchController : BaseController
    {
        private readonly InterfaceSearchAndFilterService searchService;

        public SearchController(InterfaceSearchAndFilterService searchService)
        {
            this.searchService = searchService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new SearchFilterViewModel());
        }

        [HttpGet]
        public async Task<IActionResult> Filter(SearchFilterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ErrorMessage = "Please correct the filter values.";
                return View("Index", model);
            }

            if (model.StartDate.HasValue && model.EndDate.HasValue
                && model.StartDate > model.EndDate)
            {
                model.ErrorMessage = "Start date must be before end date.";
                return View("Index", model);
            }

            var filter = new FilterCriteria
            {
                Name = model.Name,
                City = model.City,
                MaximumPrice = model.MaximumPrice,
                PlayerCount = model.MinimumPlayers,
                SortOption = model.SortOption switch
                {
                    "price_asc" => SortOption.PriceAscending,
                    "price_desc" => SortOption.PriceDescending,
                    "location" => SortOption.Location,
                    _ => SortOption.None
                },
                AvailabilityRange = (model.StartDate.HasValue && model.EndDate.HasValue)
                    ? new TimeRange(model.StartDate.Value, model.EndDate.Value)
                    : null
            };

            var results = await searchService.SearchGamesByFilter(filter);

            model.TotalPages = (int)Math.Ceiling(results.Length / (double)model.PageSize);
            model.Page = Math.Clamp(model.Page, 1, Math.Max(1, model.TotalPages));
            model.Results = results
                .Skip((model.Page - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToList();

            return View("Index", model);
        }
    }
}