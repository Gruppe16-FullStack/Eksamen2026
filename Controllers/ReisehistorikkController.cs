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
    public class ReisehistorikkController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReisehistorikkController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reisehistorikk
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Reisehistorikker.Include(r => r.Favoritt);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Reisehistorikk/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reisehistorikk = await _context.Reisehistorikker
                .Include(r => r.Favoritt)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reisehistorikk == null)
            {
                return NotFound();
            }

            return View(reisehistorikk);
        }

        // GET: Reisehistorikk/Create
        public IActionResult Create()
        {
            ViewData["FavorittId"] = new SelectList(_context.Favoritter, "Id", "FraStoppested");
            return View();
        }

        // POST: Reisehistorikk/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FavorittId,Brukt,FaktiskAvgangstid")] Reisehistorikk reisehistorikk)
        {
            if (ModelState.IsValid)
            {
                _context.Add(reisehistorikk);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FavorittId"] = new SelectList(_context.Favoritter, "Id", "FraStoppested", reisehistorikk.FavorittId);
            return View(reisehistorikk);
        }

        // GET: Reisehistorikk/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reisehistorikk = await _context.Reisehistorikker.FindAsync(id);
            if (reisehistorikk == null)
            {
                return NotFound();
            }
            ViewData["FavorittId"] = new SelectList(_context.Favoritter, "Id", "FraStoppested", reisehistorikk.FavorittId);
            return View(reisehistorikk);
        }

        // POST: Reisehistorikk/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FavorittId,Brukt,FaktiskAvgangstid")] Reisehistorikk reisehistorikk)
        {
            if (id != reisehistorikk.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reisehistorikk);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReisehistorikkExists(reisehistorikk.Id))
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
            ViewData["FavorittId"] = new SelectList(_context.Favoritter, "Id", "FraStoppested", reisehistorikk.FavorittId);
            return View(reisehistorikk);
        }

        // GET: Reisehistorikk/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reisehistorikk = await _context.Reisehistorikker
                .Include(r => r.Favoritt)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reisehistorikk == null)
            {
                return NotFound();
            }

            return View(reisehistorikk);
        }

        // POST: Reisehistorikk/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reisehistorikk = await _context.Reisehistorikker.FindAsync(id);
            if (reisehistorikk != null)
            {
                _context.Reisehistorikker.Remove(reisehistorikk);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReisehistorikkExists(int id)
        {
            return _context.Reisehistorikker.Any(e => e.Id == id);
        }
    }
}
