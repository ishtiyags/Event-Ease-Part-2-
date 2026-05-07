using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // LIST EVENTS
        // =========================
        public async Task<IActionResult> Index()
        {
            var events = _context.Events.Include(e => e.Venue);
            return View(await events.ToListAsync());
        }

        // =========================
        // CREATE (GET)
        // =========================
        public IActionResult Create()
        {
            ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "Name");
            return View();
        }

        // =========================
        // CREATE (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event ev)
        {
            // Remove navigation properties from validation
            ModelState.Remove("Venue");
            ModelState.Remove("Bookings");

            try
            {
                if (ModelState.IsValid)
                {
                    _context.Events.Add(ev);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Event created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Error creating event.";
            }

            // Reload dropdown when returning view
            ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "Name", ev.VenueId);
            return View(ev);
        }

        // =========================
        // DELETE (GET)
        // =========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null) return NotFound();

            return View(ev);
        }

        // =========================
        // DELETE (POST)
        // =========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ev = await _context.Events.FindAsync(id);

            if (ev == null) return NotFound();

            // Prevent delete if bookings exist
            var hasBookings = _context.Bookings.Any(b => b.EventId == id);

            if (hasBookings)
            {
                TempData["Error"] = "Cannot delete event with active bookings.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Events.Remove(ev);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Event deleted successfully!";
            }
            catch (Exception)
            {
                TempData["Error"] = "Error deleting event.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}