using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pendlerapp.Data;
using Pendlerapp.Models;
using Pendlerapp.Services;

namespace Pendlerapp.Controllers
{
    [Authorize]
    public class FavorittController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EnturService _enturService;
        private readonly UserManager<IdentityUser> _userManager;

        public FavorittController(ApplicationDbContext context, EnturService enturService, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _enturService = enturService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var brukerId = _userManager.GetUserId(User);
            return View(await _context.Favoritter
                .Include(f => f.Reisehistorikker)
                .Where(f => f.BrukerId == brukerId)
                .ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brukerId = _userManager.GetUserId(User);
            var favoritt = await _context.Favoritter
                .Include(f => f.Reisehistorikker)
                .FirstOrDefaultAsync(m => m.Id == id && m.BrukerId == brukerId);

            if (favoritt == null)
            {
                return NotFound();
            }

            return View(favoritt);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Navn,FraStoppested,FraStoppestedId,TilStoppested,TilStoppestedId")] Favoritt favoritt)
        {
            favoritt.Opprettet = DateTime.Now;
            favoritt.BrukerId = _userManager.GetUserId(User) ?? string.Empty;
            if (ModelState.IsValid)
            {
                _context.Add(favoritt);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(favoritt);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brukerId = _userManager.GetUserId(User);
            var favoritt = await _context.Favoritter
                .FirstOrDefaultAsync(f => f.Id == id && f.BrukerId == brukerId);

            if (favoritt == null)
            {
                return NotFound();
            }
            return View(favoritt);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Navn,FraStoppested,FraStoppestedId,TilStoppested,TilStoppestedId,Opprettet,BrukerId")] Favoritt favoritt)
        {
            if (id != favoritt.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(favoritt);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FavorittExists(favoritt.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(favoritt);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brukerId = _userManager.GetUserId(User);
            var favoritt = await _context.Favoritter
                .FirstOrDefaultAsync(m => m.Id == id && m.BrukerId == brukerId);

            if (favoritt == null)
            {
                return NotFound();
            }

            return View(favoritt);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var brukerId = _userManager.GetUserId(User);
            var favoritt = await _context.Favoritter
                .FirstOrDefaultAsync(f => f.Id == id && f.BrukerId == brukerId);

            if (favoritt != null)
            {
                _context.Favoritter.Remove(favoritt);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Henter avgangstider fra Entur for en favoritt uten å lagre.
        /// </summary>
        public async Task<IActionResult> HentAvganger(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brukerId = _userManager.GetUserId(User);
            var favoritt = await _context.Favoritter
                .FirstOrDefaultAsync(f => f.Id == id && f.BrukerId == brukerId);

            if (favoritt == null)
            {
                return NotFound();
            }

            string json;
            bool harTil = !string.IsNullOrEmpty(favoritt.TilStoppestedId);

            if (harTil)
            {
                json = await _enturService.GetTrip(favoritt.FraStoppestedId, favoritt.TilStoppestedId);
                ViewBag.ErTrip = true;
            }
            else
            {
                json = await _enturService.GetDepartures(favoritt.FraStoppestedId);
                ViewBag.ErTrip = false;
            }

            ViewBag.Avganger = json;
            ViewBag.Favoritt = favoritt;
            return View();
            }

        /// <summary>
        /// Lagrer valgt avgang i Reisehistorikk.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VelgAvgang(int favorittId, DateTime avgangstid, DateTime planlagtAvgangstid)
        {
            var historikk = new Reisehistorikk
            {
                FavorittId = favorittId,
                Brukt = DateTime.Now,
                FaktiskAvgangstid = avgangstid.ToLocalTime(),
                PlanlagtAvgangstid = planlagtAvgangstid.ToLocalTime()
            };

            _context.Add(historikk);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
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

        private bool FavorittExists(int id)
        {
            return _context.Favoritter.Any(e => e.Id == id);
        }

        /// <summary>
        /// Oppretter en favoritt via JSON-kall fra JavaScript.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> OpprettFavorittJson([FromBody] OpprettFavorittDto dto)
        {
            var favoritt = new Favoritt
            {
                Navn = dto.Navn,
                FraStoppested = dto.FraStoppested,
                FraStoppestedId = dto.FraStoppestedId,
                TilStoppested = dto.TilStoppested,
                TilStoppestedId = dto.TilStoppestedId,
                Opprettet = DateTime.Now,
                BrukerId = _userManager.GetUserId(User) ?? string.Empty
            };

            _context.Add(favoritt);
            await _context.SaveChangesAsync();

            return Json(new { id = favoritt.Id, navn = favoritt.Navn });
        }

        /// <summary>
        /// Lagrer valgt avgang i Reisehistorikk via JSON-kall.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> VelgAvgangJson([FromBody] VelgAvgangDto dto)
        {
            var historikk = new Reisehistorikk
            {
                FavorittId = dto.FavorittId,
                Brukt = DateTime.Now,
                FaktiskAvgangstid = dto.Avgangstid.ToLocalTime(),
                PlanlagtAvgangstid = dto.PlanlagtAvgangstid.ToLocalTime()
            };

            _context.Add(historikk);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}