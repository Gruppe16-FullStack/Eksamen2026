using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pendlerapp.Data;
using Pendlerapp.Models;
using Pendlerapp.Services;

namespace Pendlerapp.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly EnturService _enturService;

    public HomeController(ApplicationDbContext context, UserManager<IdentityUser> userManager, EnturService enturService)
    {
        _context = context;
        _userManager = userManager;
        _enturService = enturService;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            var brukerId = _userManager.GetUserId(User);
            var topFavoritter = await _context.Favoritter
                .Include(f => f.Reisehistorikker)
                .Where(f => f.BrukerId == brukerId)
                .OrderByDescending(f => f.Reisehistorikker.Count)
                .Take(2)
                .ToListAsync();

            ViewBag.TopFavoritter = topFavoritter;
        }

        return View();
    }

    /// <summary>
    /// Søker etter stoppesteder via Entur Geocoder API.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> SokStoppested(string navn)
    {
        if (string.IsNullOrWhiteSpace(navn))
        {
            return Json(new List<object>());
        }

        var json = await _enturService.SearchStopPlace(navn);
        using var doc = JsonDocument.Parse(json);
        var features = doc.RootElement.GetProperty("features");

        var resultater = new List<object>();
        foreach (var feature in features.EnumerateArray())
        {
            var properties = feature.GetProperty("properties");
            var kategori = properties.TryGetProperty("category", out var cat) ? cat.ToString() : "";

            if (kategori.Contains("StopPlace") || kategori.Contains("onstreet") || kategori.Contains("railStation") || kategori.Contains("busStation"))
            {
                resultater.Add(new
                {
                    id = properties.GetProperty("id").GetString(),
                    navn = properties.GetProperty("name").GetString(),
                    label = properties.GetProperty("label").GetString()
                });
            }
        }

        return Json(resultater);
    }

    /// <summary>
    /// Henter avganger for et stoppested via Entur Journey Planner API.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> HentAvgangerForStoppested(string stopId, int antall = 5)
    {
        if (string.IsNullOrWhiteSpace(stopId))
        {
            return Json(new { feil = "Mangler stopId" });
        }

        var json = await _enturService.GetDepartures(stopId, antall);
        return Content(json, "application/json");
    }

    /// <summary>
    /// Henter reiseruter mellom to stoppesteder.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> HentTur(string fraId, string tilId)
    {
        if (string.IsNullOrWhiteSpace(fraId) || string.IsNullOrWhiteSpace(tilId))
        {
            return Json(new { feil = "Mangler fraId eller tilId" });
        }

        var json = await _enturService.GetTrip(fraId, tilId);
        return Content(json, "application/json");
    }

    /// <summary>
    /// Henter alle stopp etter avgangspunktet for en serviceJourney.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> HentStopp(string serviceJourneyId, string fraStopNavn)
    {
        if (string.IsNullOrWhiteSpace(serviceJourneyId))
        {
            return Json(new List<object>());
        }

        var json = await _enturService.GetServiceJourneyStops(serviceJourneyId);
        using var doc = JsonDocument.Parse(json);
        var passingTimes = doc.RootElement
            .GetProperty("data")
            .GetProperty("serviceJourney")
            .GetProperty("passingTimes");

        var stopp = new List<object>();
        bool funnetFra = false;

        foreach (var pt in passingTimes.EnumerateArray())
        {
            var quay = pt.GetProperty("quay");
            var navn = quay.GetProperty("name").GetString() ?? "";
            var quayId = quay.GetProperty("id").GetString() ?? "";
            var stopPlaceId = quay.TryGetProperty("stopPlace", out var sp)
                ? sp.GetProperty("id").GetString() ?? ""
                : "";

            if (!funnetFra && navn.Contains(fraStopNavn, StringComparison.OrdinalIgnoreCase))
            {
                funnetFra = true;
                continue;
            }

            if (funnetFra)
            {
                stopp.Add(new { id = stopPlaceId, quayId, navn });
            }
        }

        return Json(stopp);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}