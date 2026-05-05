using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pendlerapp.Data;
using Pendlerapp.Models;

namespace Pendlerapp.Controllers
{
    public class FavorittController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FavorittController(ApplicationDbContext context)
        {
            _context = context;
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        private bool FavorittExists(int id)
        {
            return _context.Favoritter.Any(e => e.Id == id);
        }
    }
}
