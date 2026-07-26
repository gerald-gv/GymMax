
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymMax.Models;
using GymMax.Data;

public class CoachController : Controller
{
    private readonly AppDbContext _context;

    public CoachController(AppDbContext context)
    {
        _context = context;
    }

    // GET: COACHS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Coaches.ToListAsync());
    }

    // GET: COACHS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var coach = await _context.Coaches
            .FirstOrDefaultAsync(m => m.CoachId == id);
        if (coach == null)
        {
            return NotFound();
        }

        return View(coach);
    }

    // GET: COACHS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: COACHS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CoachId,UsuarioId,SedeId,FechaIngreso,Activo,Usuario,Sede")] Coach coach)
    {
        if (ModelState.IsValid)
        {
            _context.Add(coach);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(coach);
    }

    // GET: COACHS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var coach = await _context.Coaches.FindAsync(id);
        if (coach == null)
        {
            return NotFound();
        }
        return View(coach);
    }

    // POST: COACHS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("CoachId,UsuarioId,SedeId,FechaIngreso,Activo,Usuario,Sede")] Coach coach)
    {
        if (id != coach.CoachId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(coach);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CoachExists(coach.CoachId))
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
        return View(coach);
    }

    // GET: COACHS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var coach = await _context.Coaches
            .FirstOrDefaultAsync(m => m.CoachId == id);
        if (coach == null)
        {
            return NotFound();
        }

        return View(coach);
    }

    // POST: COACHS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var coach = await _context.Coaches.FindAsync(id);
        if (coach != null)
        {
            _context.Coaches.Remove(coach);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CoachExists(int id)
    {
        return _context.Coaches.Any(e => e.CoachId == id);
    }
}
