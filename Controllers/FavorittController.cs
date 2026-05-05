using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pendlerapp.Data;
using Pendlerapp.Models;
using Pendlerapp.Services;

namespace Pendlerapp.Controllers
{
    public class FavorittController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EnturService _enturService;

        public FavorittController(ApplicationDbContext context, EnturService enturService)
        {
            _context = context;
            _enturService = enturService;
        }

        // GET: Favoritt
        public async Task<IActionResult> Index()
        {
            return View(await _context.Favoritter
            .Include(f => f.Reisehistorikker)
            .ToListAsync());
        }

        // GET: Favoritt/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var favoritt = await _context.Favoritter
                .Include(f => f.Reisehistorikker)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (favoritt == null)
            {
                return NotFound();
            }

            return View(favoritt);
        }

        // GET: Favoritt/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Favoritt/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Navn,FraStoppested,FraStoppestedId,TilStoppested,TilStoppestedId,BrukerId")] Favoritt favoritt)
        {
            favoritt.Opprettet = DateTime.Now;
            if (ModelState.IsValid)
            {
                _context.Add(favoritt);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(favoritt);
        }

        // GET: Favoritt/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var favoritt = await _context.Favoritter.FindAsync(id);
            if (favoritt == null)
            {
                return NotFound();
            }
            return View(favoritt);
        }

        // POST: Favoritt/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Navn,FraStoppested,FraStoppestedId,TilStoppested,TilStoppestedId,Opprettet,BrukerId")] Favoritt favoritt)
        {
            if (id != favoritt.Id)
            {
                return NotFound();
            }

            favoritt.BrukerId = string.Empty;
            ModelState.ClearValidationState("BrukerId");
            ModelState.MarkFieldValid("BrukerId");

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

        // GET: Favoritt/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var favoritt = await _context.Favoritter
                .FirstOrDefaultAsync(m => m.Id == id);
            if (favoritt == null)
            {
                return NotFound();
            }

            return View(favoritt);
        }

        // POST: Favoritt/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var favoritt = await _context.Favoritter.FindAsync(id);
            if (favoritt != null)
            {
                _context.Favoritter.Remove(favoritt);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Favoritt/HentAvganger/5
        /// <summary>
        /// Henter avgangstider fra Entur for en favoritt uten å lagre.
        /// </summary>
        public async Task<IActionResult> HentAvganger(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var favoritt = await _context.Favoritter.FindAsync(id);
            if (favoritt == null)
            {
                return NotFound();
            }

            var json = await _enturService.GetDepartures(favoritt.FraStoppestedId);

            ViewBag.Avganger = json;
            ViewBag.Favoritt = favoritt;
            return View();
        }

        // POST: Favoritt/VelgAvgang
        /// <summary>
        /// Lagrer valgt avgang i Reisehistorikk.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VelgAvgang(int favorittId, DateTime avgangstid)
        {
            var historikk = new Reisehistorikk
            {
                FavorittId = favorittId,
                Brukt = DateTime.Now,
                FaktiskAvgangstid = avgangstid
            };

            _context.Add(historikk);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool FavorittExists(int id)
        {
            return _context.Favoritter.Any(e => e.Id == id);
        }
    }
}