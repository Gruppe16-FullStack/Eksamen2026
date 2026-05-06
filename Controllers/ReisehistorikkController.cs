using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pendlerapp.Data;
using Pendlerapp.Models;

namespace Pendlerapp.Controllers
{
    [Authorize]
    public class ReisehistorikkController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReisehistorikkController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var brukerId = _userManager.GetUserId(User);
            var historikk = await _context.Reisehistorikker
                .Include(r => r.Favoritt)
                .Where(r => r.Favoritt != null && r.Favoritt.BrukerId == brukerId)
                .OrderByDescending(r => r.FaktiskAvgangstid)
                .ToListAsync();
            return View(historikk);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brukerId = _userManager.GetUserId(User);
            var reisehistorikk = await _context.Reisehistorikker
                .Include(r => r.Favoritt)
                .FirstOrDefaultAsync(m => m.Id == id && m.Favoritt != null && m.Favoritt.BrukerId == brukerId);

            if (reisehistorikk == null)
            {
                return NotFound();
            }

            return View(reisehistorikk);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brukerId = _userManager.GetUserId(User);
            var reisehistorikk = await _context.Reisehistorikker
                .Include(r => r.Favoritt)
                .FirstOrDefaultAsync(m => m.Id == id && m.Favoritt != null && m.Favoritt.BrukerId == brukerId);

            if (reisehistorikk == null)
            {
                return NotFound();
            }

            return View(reisehistorikk);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var brukerId = _userManager.GetUserId(User);
            var reisehistorikk = await _context.Reisehistorikker
                .Include(r => r.Favoritt)
                .FirstOrDefaultAsync(r => r.Id == id && r.Favoritt != null && r.Favoritt.BrukerId == brukerId);

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